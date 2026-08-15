using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Services;

public sealed class HorarioRecursoService(
    IHorarioRecursoRepository horarioRepository,
    IExcepcionHorarioRecursoRepository excepcionRepository,
    IHorarioSedeRepository horarioSedeRepository,
    IRecursoRepository recursoRepository) : IHorarioRecursoService
{
    public const string RecursoInvalido = "El recurso no existe o fue eliminado.";
    public const string RecursoInactivo = "El recurso está inactivo.";
    public const string FueraDeHorarioSede = "El intervalo está fuera del horario de la sede.";
    public const string IntervalosSuperpuestos = "Existen intervalos superpuestos en el horario semanal.";
    public const string ExcepcionIncompatible =
        "No puede coexistir un cierre total con otras excepciones activas en la misma fecha.";

    public async Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(
        Guid recursoId, CancellationToken ct = default)
    {
        if (await ValidarRecursoAsync(recursoId, ct) is { } errorRecurso)
            return ResultadoOperacion<HorarioSemanalDto>.Fallo(errorRecurso, TipoErrorOperacion.NoEncontrado);

        var intervalos = (await horarioRepository.ListarAsync(recursoId, ct))
            .Where(h => h.EstaActivo)
            .Select(h => new IntervaloHorarioDto(h.Id, h.DiaSemana, h.HoraInicio, h.HoraFin))
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
            .ToList();
        return ResultadoOperacion<HorarioSemanalDto>.Exito(new HorarioSemanalDto(intervalos));
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        Guid recursoId, ActualizarHorarioSemanalSolicitud solicitud, CancellationToken ct = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(recursoId, ct);
        if (recurso is null || recurso.EstaEliminado)
            return ResultadoOperacion.Fallo(RecursoInvalido, TipoErrorOperacion.NoEncontrado);
        if (!recurso.EstaActivo)
            return ResultadoOperacion.Fallo(RecursoInactivo, TipoErrorOperacion.Conflicto);

        var errorColeccion = ValidadorIntervalos.ValidarColeccionSemana(
            solicitud.Intervalos, i => i.DiaSemana, i => i.HoraInicio, i => i.HoraFin);
        if (errorColeccion is not null)
            return ResultadoOperacion.Fallo(errorColeccion, TipoErrorOperacion.Conflicto);

        var horarioSede = await horarioSedeRepository.ListarAsync(recurso.SedeId, ct);
        foreach (var intervalo in solicitud.Intervalos)
        {
            var dentroDeSede = horarioSede.Any(h => h.EstaActivo && h.DiaSemana == intervalo.DiaSemana &&
                ValidadorIntervalos.EstaContenidoEn(intervalo.HoraInicio, intervalo.HoraFin, h.HoraInicio, h.HoraFin));
            if (!dentroDeSede)
                return ResultadoOperacion.Fallo(FueraDeHorarioSede, TipoErrorOperacion.Conflicto);
        }

        var existentes = await horarioRepository.ListarAsync(recursoId, ct);

        await horarioRepository.EjecutarEnTransaccionAsync(async innerCt =>
        {
            foreach (var existente in existentes)
            {
                var sigueExistiendo = solicitud.Intervalos.Any(i =>
                    i.DiaSemana == existente.DiaSemana &&
                    i.HoraInicio == existente.HoraInicio && i.HoraFin == existente.HoraFin);
                if (sigueExistiendo)
                {
                    existente.EstaActivo = true;
                    existente.EstaEliminado = false;
                }
                else
                {
                    existente.EstaActivo = false;
                    existente.EstaEliminado = true;
                }
                existente.FechaModificacion = DateTime.UtcNow;
            }
            foreach (var nuevo in solicitud.Intervalos.Where(i => existentes.All(e =>
                e.DiaSemana != i.DiaSemana || e.HoraInicio != i.HoraInicio || e.HoraFin != i.HoraFin)))
                horarioRepository.Agregar(new HorarioRecurso
                {
                    RecursoId = recursoId,
                    DiaSemana = nuevo.DiaSemana,
                    HoraInicio = nuevo.HoraInicio,
                    HoraFin = nuevo.HoraFin
                });
            await horarioRepository.GuardarAsync(innerCt);
        }, ct);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepcionesAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default)
    {
        if (await ValidarRecursoAsync(filtro.EntidadId, ct) is { } errorRecurso)
            return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Fallo(
                errorRecurso, TipoErrorOperacion.NoEncontrado);
        return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Exito(
            await excepcionRepository.ListarAsync(filtro, ct));
    }

    public async Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionRecursoSolicitud solicitud, CancellationToken ct = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);
        if (await ValidarRecursoAsync(solicitud.RecursoId, ct) is { } errorRecurso)
            return ResultadoOperacion<Guid>.Fallo(errorRecurso, TipoErrorOperacion.NoEncontrado);
        var recurso = await recursoRepository.ObtenerParaModificarAsync(solicitud.RecursoId, ct);
        if (!recurso!.EstaActivo)
            return ResultadoOperacion<Guid>.Fallo(RecursoInactivo, TipoErrorOperacion.Conflicto);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(
            solicitud.RecursoId, solicitud.Fecha, ct: ct);
        var errorCompat = ValidarCompatibilidad(solicitud.TipoExcepcion, solicitud.HoraInicio, solicitud.HoraFin, activas);
        if (errorCompat is not null) return ResultadoOperacion<Guid>.Fallo(errorCompat, TipoErrorOperacion.Conflicto);

        var excepcion = new ExcepcionHorarioRecurso { RecursoId = solicitud.RecursoId };
        AsignarCampos(excepcion, solicitud);
        excepcionRepository.Agregar(excepcion);
        await excepcionRepository.GuardarAsync(ct);
        return ResultadoOperacion<Guid>.Exito(excepcion.Id);
    }

    public async Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionRecursoSolicitud solicitud, CancellationToken ct = default)
    {
        var excepcion = await excepcionRepository.ObtenerParaModificarAsync(solicitud.Id, ct);
        if (ValidarRegistro(excepcion) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(
            excepcion!.RecursoId, solicitud.Fecha, excepcion.Id, ct);
        var errorCompat = ValidarCompatibilidad(solicitud.TipoExcepcion, solicitud.HoraInicio, solicitud.HoraFin, activas);
        if (errorCompat is not null) return ResultadoOperacion.Fallo(errorCompat, TipoErrorOperacion.Conflicto);

        AsignarCampos(excepcion, solicitud);
        excepcion.FechaModificacion = DateTime.UtcNow;
        await excepcionRepository.GuardarAsync(ct);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarExcepcionAsync(Guid id, CancellationToken ct = default)
    {
        var excepcion = await excepcionRepository.ObtenerParaModificarAsync(id, ct);
        if (ValidarRegistro(excepcion) is { } estado) return estado;
        excepcion!.EstaActivo = false;
        excepcion.EstaEliminado = true;
        excepcion.FechaModificacion = DateTime.UtcNow;
        await excepcionRepository.GuardarAsync(ct);
        return ResultadoOperacion.Exito();
    }

    private async Task<string?> ValidarRecursoAsync(Guid recursoId, CancellationToken ct)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(recursoId, ct);
        return recurso is null || recurso.EstaEliminado ? RecursoInvalido : null;
    }

    private static string? ValidarCompatibilidad(
        TipoExcepcionHorario tipo, TimeOnly? inicio, TimeOnly? fin,
        IReadOnlyList<ExcepcionHorarioRecurso> activas)
    {
        if (tipo == TipoExcepcionHorario.CerradoTodoElDia)
            return activas.Count > 0 ? ExcepcionIncompatible : null;
        if (activas.Any(a => a.TipoExcepcion == TipoExcepcionHorario.CerradoTodoElDia))
            return ExcepcionIncompatible;
        if (inicio.HasValue && fin.HasValue && activas.Any(a => a.HoraInicio.HasValue && a.HoraFin.HasValue &&
            ValidadorIntervalos.SeSuperponen(inicio.Value, fin.Value, a.HoraInicio.Value, a.HoraFin.Value)))
            return "Ya existe una excepción que se superpone en esta fecha.";
        return null;
    }

    private static ResultadoOperacion? ValidarRegistro(ExcepcionHorarioRecurso? excepcion) =>
        excepcion is null || excepcion.EstaEliminado
            ? ResultadoOperacion.Fallo("La excepción no existe o fue retirada.", TipoErrorOperacion.NoEncontrado)
            : null;

    private static string? Validar(GuardarExcepcionHorarioSolicitud s)
    {
        if (string.IsNullOrWhiteSpace(s.Motivo)) return "El motivo es obligatorio.";
        if (s.Motivo.Trim().Length > 250) return "El motivo no puede superar 250 caracteres.";
        if (s.Observaciones?.Trim().Length > 500) return "Las observaciones no pueden superar 500 caracteres.";
        if (!Enum.IsDefined(s.TipoExcepcion) || s.TipoExcepcion == TipoExcepcionHorario.NoDefinida)
            return "El tipo de excepción no es válido.";
        switch (s.TipoExcepcion)
        {
            case TipoExcepcionHorario.CerradoTodoElDia:
                if (s.HoraInicio is not null || s.HoraFin is not null)
                    return "Un cierre total no debe incluir horas.";
                break;
            case TipoExcepcionHorario.HorarioEspecial:
            case TipoExcepcionHorario.NoDisponibleParcial:
                if (s.HoraInicio is null || s.HoraFin is null)
                    return "Debe indicar hora de inicio y fin.";
                if (s.HoraInicio >= s.HoraFin)
                    return "La hora de inicio debe ser menor a la hora de fin.";
                break;
        }
        return null;
    }

    private static void AsignarCampos(ExcepcionHorarioRecurso e, GuardarExcepcionHorarioSolicitud s)
    {
        e.Fecha = s.Fecha;
        e.TipoExcepcion = s.TipoExcepcion;
        e.HoraInicio = s.HoraInicio;
        e.HoraFin = s.HoraFin;
        e.Motivo = s.Motivo.Trim();
        e.Observaciones = string.IsNullOrWhiteSpace(s.Observaciones) ? null : s.Observaciones.Trim();
    }
}
