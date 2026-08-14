using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Services;

public sealed class HorarioProfesionalService(
    IHorarioProfesionalRepository horarioRepository,
    IExcepcionHorarioProfesionalRepository excepcionRepository,
    IHorarioSedeRepository horarioSedeRepository,
    IEmpleadoRepository empleadoRepository,
    ISedeRepository sedeRepository) : IHorarioProfesionalService
{
    public const string EmpleadoInvalido = "El empleado no existe o fue eliminado.";
    public const string NoEsProfesional = "El empleado debe estar marcado como profesional.";
    public const string ProfesionalInactivo = "El profesional está inactivo.";
    public const string SedeInvalida = "La sede no existe, fue eliminada o está inactiva.";
    public const string NoAsignadoASede = "El profesional no está asignado a esta sede.";
    public const string FueraDeHorarioSede = "El intervalo está fuera del horario de la sede.";
    public const string CruceEntreSedes = "El profesional tiene un cruce de horario con otra sede.";
    public const string IntervalosSuperpuestos = "Existen intervalos superpuestos en el horario semanal.";
    public const string ExcepcionIncompatible =
        "No puede coexistir un cierre total con otras excepciones activas en la misma fecha.";

    public async Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(
        Guid empleadoId, Guid? sedeId, CancellationToken ct = default)
    {
        if (await ValidarProfesionalAsync(empleadoId, ct) is { } errorEmp)
            return ResultadoOperacion<HorarioSemanalDto>.Fallo(errorEmp, TipoErrorOperacion.NoEncontrado);

        var horarios = sedeId.HasValue
            ? await horarioRepository.ListarPorSedeAsync(empleadoId, sedeId.Value, ct)
            : await horarioRepository.ListarPorEmpleadoAsync(empleadoId, ct);

        var intervalos = horarios.Where(h => h.EstaActivo)
            .Select(h => new IntervaloHorarioDto(h.Id, h.DiaSemana, h.HoraInicio, h.HoraFin))
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
            .ToList();
        return ResultadoOperacion<HorarioSemanalDto>.Exito(new HorarioSemanalDto(intervalos));
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        Guid empleadoId, Guid sedeId, ActualizarHorarioSemanalSolicitud solicitud,
        CancellationToken ct = default)
    {
        if (await ValidarProfesionalAsync(empleadoId, ct) is { } errorEmp)
            return ResultadoOperacion.Fallo(errorEmp, TipoErrorOperacion.NoEncontrado);

        var empleado = await empleadoRepository.ObtenerParaModificarAsync(empleadoId, ct);
        if (!empleado!.EsProfesional)
            return ResultadoOperacion.Fallo(NoEsProfesional, TipoErrorOperacion.Conflicto);
        if (!empleado.EstaActivo)
            return ResultadoOperacion.Fallo(ProfesionalInactivo, TipoErrorOperacion.Conflicto);

        var sede = await sedeRepository.ObtenerParaModificarAsync(sedeId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo)
            return ResultadoOperacion.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        var relacionesSede = await empleadoRepository.ObtenerRelacionesSedeAsync(empleadoId, ct);
        if (relacionesSede.All(r => r.SedeId != sedeId || !r.EstaActivo))
            return ResultadoOperacion.Fallo(NoAsignadoASede, TipoErrorOperacion.Conflicto);

        var errorColeccion = ValidadorIntervalos.ValidarColeccionSemana(
            solicitud.Intervalos, i => i.DiaSemana, i => i.HoraInicio, i => i.HoraFin);
        if (errorColeccion is not null)
            return ResultadoOperacion.Fallo(errorColeccion, TipoErrorOperacion.Conflicto);

        var horarioSede = await horarioSedeRepository.ListarAsync(sedeId, ct);
        foreach (var intervalo in solicitud.Intervalos)
        {
            var dentroDeSede = horarioSede.Any(h => h.EstaActivo && h.DiaSemana == intervalo.DiaSemana &&
                ValidadorIntervalos.EstaContenidoEn(intervalo.HoraInicio, intervalo.HoraFin, h.HoraInicio, h.HoraFin));
            if (!dentroDeSede)
                return ResultadoOperacion.Fallo(FueraDeHorarioSede, TipoErrorOperacion.Conflicto);
        }

        var todosLosHorarios = await horarioRepository.ListarPorEmpleadoAsync(empleadoId, ct);
        var otrasSedesActivos = todosLosHorarios.Where(h => h.SedeId != sedeId && h.EstaActivo).ToList();
        foreach (var nuevo in solicitud.Intervalos)
            if (otrasSedesActivos.Any(o => o.DiaSemana == nuevo.DiaSemana &&
                ValidadorIntervalos.SeSuperponen(nuevo.HoraInicio, nuevo.HoraFin, o.HoraInicio, o.HoraFin)))
                return ResultadoOperacion.Fallo(CruceEntreSedes, TipoErrorOperacion.Conflicto);

        var existentes = await horarioRepository.ListarPorSedeAsync(empleadoId, sedeId, ct);

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
                horarioRepository.Agregar(new HorarioProfesional
                {
                    EmpleadoId = empleadoId,
                    SedeId = sedeId,
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
        if (await ValidarProfesionalAsync(filtro.EntidadId, ct) is { } errorEmp)
            return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Fallo(
                errorEmp, TipoErrorOperacion.NoEncontrado);
        return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Exito(
            await excepcionRepository.ListarAsync(filtro, ct));
    }

    public async Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionProfesionalSolicitud solicitud, CancellationToken ct = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);
        if (await ValidarProfesionalAsync(solicitud.EmpleadoId, ct) is { } errorEmp)
            return ResultadoOperacion<Guid>.Fallo(errorEmp, TipoErrorOperacion.NoEncontrado);

        var relacionesSede = await empleadoRepository.ObtenerRelacionesSedeAsync(solicitud.EmpleadoId, ct);
        if (relacionesSede.All(r => r.SedeId != solicitud.SedeId || !r.EstaActivo))
            return ResultadoOperacion<Guid>.Fallo(NoAsignadoASede, TipoErrorOperacion.Conflicto);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(
            solicitud.EmpleadoId, solicitud.Fecha, ct: ct);
        var errorCompat = ValidarCompatibilidad(solicitud.TipoExcepcion, solicitud.HoraInicio, solicitud.HoraFin, activas);
        if (errorCompat is not null) return ResultadoOperacion<Guid>.Fallo(errorCompat, TipoErrorOperacion.Conflicto);

        var excepcion = new ExcepcionHorarioProfesional
        {
            EmpleadoId = solicitud.EmpleadoId,
            SedeId = solicitud.SedeId
        };
        AsignarCampos(excepcion, solicitud);
        excepcionRepository.Agregar(excepcion);
        await excepcionRepository.GuardarAsync(ct);
        return ResultadoOperacion<Guid>.Exito(excepcion.Id);
    }

    public async Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionProfesionalSolicitud solicitud, CancellationToken ct = default)
    {
        var excepcion = await excepcionRepository.ObtenerParaModificarAsync(solicitud.Id, ct);
        if (ValidarRegistro(excepcion) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(
            excepcion!.EmpleadoId, solicitud.Fecha, excepcion.Id, ct);
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

    private async Task<string?> ValidarProfesionalAsync(Guid empleadoId, CancellationToken ct)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(empleadoId, ct);
        if (empleado is null || empleado.EstaEliminado) return EmpleadoInvalido;
        if (!empleado.EsProfesional) return NoEsProfesional;
        return null;
    }

    private static string? ValidarCompatibilidad(
        TipoExcepcionHorario tipo, TimeOnly? inicio, TimeOnly? fin,
        IReadOnlyList<ExcepcionHorarioProfesional> activas)
    {
        if (tipo == TipoExcepcionHorario.CerradoTodoElDia)
            return activas.Count > 0 ? ExcepcionIncompatible : null;
        if (activas.Any(a => a.TipoExcepcion == TipoExcepcionHorario.CerradoTodoElDia))
            return ExcepcionIncompatible;
        if (tipo == TipoExcepcionHorario.NoDisponibleParcial)
        {
            var solapa = activas.Any(a =>
                a.TipoExcepcion == TipoExcepcionHorario.NoDisponibleParcial &&
                ValidadorIntervalos.SeSuperponen(inicio!.Value, fin!.Value, a.HoraInicio!.Value, a.HoraFin!.Value));
            if (solapa) return "Ya existe una indisponibilidad parcial que se superpone en esta fecha.";
        }
        return null;
    }

    private static ResultadoOperacion? ValidarRegistro(ExcepcionHorarioProfesional? excepcion) =>
        excepcion is null || excepcion.EstaEliminado
            ? ResultadoOperacion.Fallo("La excepción no existe o fue retirada.", TipoErrorOperacion.NoEncontrado)
            : null;

    private static string? Validar(GuardarExcepcionHorarioSolicitud s)
    {
        if (string.IsNullOrWhiteSpace(s.Motivo)) return "El motivo es obligatorio.";
        if (s.Motivo.Trim().Length > 250) return "El motivo no puede superar 250 caracteres.";
        if (s.Observaciones?.Trim().Length > 500) return "Las observaciones no pueden superar 500 caracteres.";
        if (!Enum.IsDefined(s.TipoExcepcion)) return "El tipo de excepción no es válido.";
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

    private static void AsignarCampos(ExcepcionHorarioProfesional e, GuardarExcepcionHorarioSolicitud s)
    {
        e.Fecha = s.Fecha;
        e.TipoExcepcion = s.TipoExcepcion;
        e.HoraInicio = s.HoraInicio;
        e.HoraFin = s.HoraFin;
        e.Motivo = s.Motivo.Trim();
        e.Observaciones = string.IsNullOrWhiteSpace(s.Observaciones) ? null : s.Observaciones.Trim();
    }
}