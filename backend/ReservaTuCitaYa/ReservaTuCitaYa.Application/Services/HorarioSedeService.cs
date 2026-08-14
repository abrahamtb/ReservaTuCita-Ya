using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Services;

public sealed class HorarioSedeService(
    IHorarioSedeRepository horarioRepository,
    IExcepcionHorarioSedeRepository excepcionRepository,
    ISedeRepository sedeRepository) : IHorarioSedeService
{
    public const string SedeInvalida = "La sede no existe, fue eliminada o está inactiva.";
    public const string IntervalosSuperpuestos = "Existen intervalos superpuestos en el horario semanal.";
    public const string ExcepcionIncompatible =
        "No puede coexistir un cierre total con otras excepciones activas en la misma fecha.";

    public async Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(
        Guid sedeId, CancellationToken ct = default)
    {
        if (await ValidarSedeAsync(sedeId, ct) is { } errorSede)
            return ResultadoOperacion<HorarioSemanalDto>.Fallo(errorSede, TipoErrorOperacion.NoEncontrado);

        var intervalos = (await horarioRepository.ListarAsync(sedeId, ct))
            .Where(h => h.EstaActivo)
            .Select(h => new IntervaloHorarioDto(h.Id, h.DiaSemana, h.HoraInicio, h.HoraFin))
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
            .ToList();
        return ResultadoOperacion<HorarioSemanalDto>.Exito(new HorarioSemanalDto(intervalos));
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        Guid sedeId, ActualizarHorarioSemanalSolicitud solicitud, CancellationToken ct = default)
    {
        if (await ValidarSedeAsync(sedeId, ct) is { } errorSede)
            return ResultadoOperacion.Fallo(errorSede, TipoErrorOperacion.NoEncontrado);

        var errorColeccion = ValidadorIntervalos.ValidarColeccionSemana(
            solicitud.Intervalos, i => i.DiaSemana, i => i.HoraInicio, i => i.HoraFin);
        if (errorColeccion is not null)
            return ResultadoOperacion.Fallo(errorColeccion, TipoErrorOperacion.Conflicto);

        var existentes = await horarioRepository.ListarAsync(sedeId, ct);

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
                horarioRepository.Agregar(new HorarioSede
                {
                    SedeId = sedeId, DiaSemana = nuevo.DiaSemana,
                    HoraInicio = nuevo.HoraInicio, HoraFin = nuevo.HoraFin
                });
            await horarioRepository.GuardarAsync(innerCt);
        }, ct);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepcionesAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default)
    {
        if (await ValidarSedeAsync(filtro.EntidadId, ct) is { } errorSede)
            return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Fallo(
                errorSede, TipoErrorOperacion.NoEncontrado);
        return ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>.Exito(
            await excepcionRepository.ListarAsync(filtro, ct));
    }

    public async Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionSedeSolicitud solicitud, CancellationToken ct = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);
        if (await ValidarSedeAsync(solicitud.SedeId, ct) is { } errorSede)
            return ResultadoOperacion<Guid>.Fallo(errorSede, TipoErrorOperacion.NoEncontrado);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(solicitud.SedeId, solicitud.Fecha, ct: ct);
        var errorCompat = ValidarCompatibilidad(solicitud.TipoExcepcion, solicitud.HoraInicio, solicitud.HoraFin, activas);
        if (errorCompat is not null) return ResultadoOperacion<Guid>.Fallo(errorCompat, TipoErrorOperacion.Conflicto);

        var excepcion = new ExcepcionHorarioSede { SedeId = solicitud.SedeId };
        AsignarCampos(excepcion, solicitud);
        excepcionRepository.Agregar(excepcion);
        await excepcionRepository.GuardarAsync(ct);
        return ResultadoOperacion<Guid>.Exito(excepcion.Id);
    }

    public async Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionSedeSolicitud solicitud, CancellationToken ct = default)
    {
        var excepcion = await excepcionRepository.ObtenerParaModificarAsync(solicitud.Id, ct);
        if (ValidarRegistro(excepcion) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);

        var activas = await excepcionRepository.ObtenerActivasEnFechaAsync(
            excepcion!.SedeId, solicitud.Fecha, excepcion.Id, ct);
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

    private async Task<string?> ValidarSedeAsync(Guid sedeId, CancellationToken ct)
    {
        var sede = await sedeRepository.ObtenerParaModificarAsync(sedeId, ct);
        return sede is null || sede.EstaEliminado || !sede.EstaActivo ? SedeInvalida : null;
    }

    private static string? ValidarCompatibilidad(
        TipoExcepcionHorario tipo, TimeOnly? inicio, TimeOnly? fin,
        IReadOnlyList<ExcepcionHorarioSede> activas)
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
    private static ResultadoOperacion? ValidarRegistro(ExcepcionHorarioSede? excepcion) =>
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

    private static void AsignarCampos(ExcepcionHorarioSede e, GuardarExcepcionHorarioSolicitud s)
    {
        e.Fecha = s.Fecha;
        e.TipoExcepcion = s.TipoExcepcion;
        e.HoraInicio = s.HoraInicio;
        e.HoraFin = s.HoraFin;
        e.Motivo = s.Motivo.Trim();
        e.Observaciones = string.IsNullOrWhiteSpace(s.Observaciones) ? null : s.Observaciones.Trim();
    }
}