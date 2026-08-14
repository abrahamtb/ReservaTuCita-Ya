using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Services;

public sealed class BloqueoRecursoService(
    IBloqueoRecursoRepository bloqueoRepository,
    IRecursoRepository recursoRepository) : IBloqueoRecursoService
{
    public const string RecursoInvalido = "El recurso no existe o fue eliminado.";
    public const string BloqueoSolapado = "El periodo se solapa con otro bloqueo existente para este recurso.";

    public async Task<ResultadoOperacion<IReadOnlyList<BloqueoRecursoDto>>> ListarPorRecursoAsync(
        Guid recursoId, CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(recursoId, cancellationToken);
        if (recurso is null || recurso.EstaEliminado)
            return ResultadoOperacion<IReadOnlyList<BloqueoRecursoDto>>.Fallo(
                RecursoInvalido, TipoErrorOperacion.NoEncontrado);
        return ResultadoOperacion<IReadOnlyList<BloqueoRecursoDto>>.Exito(
            await bloqueoRepository.ListarPorRecursoAsync(recursoId, cancellationToken));
    }

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearBloqueoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);

        var recurso = await recursoRepository.ObtenerParaModificarAsync(solicitud.RecursoId, cancellationToken);
        if (recurso is null || recurso.EstaEliminado)
            return ResultadoOperacion<Guid>.Fallo(RecursoInvalido, TipoErrorOperacion.NoEncontrado);

        if (await bloqueoRepository.ExisteSolapamientoAsync(
                solicitud.RecursoId, solicitud.FechaHoraInicio, solicitud.FechaHoraFin,
                cancellationToken: cancellationToken))
            return ResultadoOperacion<Guid>.Fallo(BloqueoSolapado, TipoErrorOperacion.Conflicto);

        var bloqueo = new BloqueoRecurso { RecursoId = solicitud.RecursoId };
        AsignarCampos(bloqueo, solicitud);
        bloqueoRepository.Agregar(bloqueo);
        await bloqueoRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion<Guid>.Exito(bloqueo.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarBloqueoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var bloqueo = await bloqueoRepository.ObtenerParaModificarAsync(solicitud.Id, cancellationToken);
        if (ValidarRegistro(bloqueo) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);

        if (await bloqueoRepository.ExisteSolapamientoAsync(
                bloqueo!.RecursoId, solicitud.FechaHoraInicio, solicitud.FechaHoraFin,
                bloqueo.Id, cancellationToken))
            return ResultadoOperacion.Fallo(BloqueoSolapado, TipoErrorOperacion.Conflicto);

        AsignarCampos(bloqueo, solicitud);
        bloqueo.FechaModificacion = DateTime.UtcNow;
        await bloqueoRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var bloqueo = await bloqueoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(bloqueo) is { } estado) return estado;
        bloqueo!.EstaActivo = false;
        bloqueo.EstaEliminado = true;
        bloqueo.FechaModificacion = DateTime.UtcNow;
        await bloqueoRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private static ResultadoOperacion? ValidarRegistro(BloqueoRecurso? bloqueo) =>
        bloqueo is null || bloqueo.EstaEliminado
            ? ResultadoOperacion.Fallo("El bloqueo no existe o fue retirado.", TipoErrorOperacion.NoEncontrado)
            : null;

    private static string? Validar(GuardarBloqueoSolicitud s)
    {
        if (s.FechaHoraInicio >= s.FechaHoraFin) return "La fecha de inicio debe ser menor a la fecha de fin.";
        if (string.IsNullOrWhiteSpace(s.Motivo)) return "El motivo es obligatorio.";
        if (s.Motivo.Trim().Length > 250) return "El motivo no puede superar 250 caracteres.";
        if (s.Observaciones?.Trim().Length > 500) return "Las observaciones no pueden superar 500 caracteres.";
        if (!Enum.IsDefined(s.TipoBloqueo)) return "El tipo de bloqueo no es válido.";
        return null;
    }

    private static void AsignarCampos(BloqueoRecurso b, GuardarBloqueoSolicitud s)
    {
        b.FechaHoraInicio = s.FechaHoraInicio;
        b.FechaHoraFin = s.FechaHoraFin;
        b.TipoBloqueo = s.TipoBloqueo;
        b.Motivo = s.Motivo.Trim();
        b.Observaciones = string.IsNullOrWhiteSpace(s.Observaciones) ? null : s.Observaciones.Trim();
    }
}