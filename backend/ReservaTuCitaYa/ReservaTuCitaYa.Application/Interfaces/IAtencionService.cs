using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Atenciones;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IAtencionService
{
    Task<ResultadoOperacion<MarcarPresenteRespuesta>> MarcarPresenteAsync(
        Guid organizacionId,
        Guid reservaId,
        string? usuarioId,
        CancellationToken ct = default);
    Task<ResultadoOperacion<IniciarAtencionRespuesta>> IniciarAtencionAsync(
        Guid organizacionId,
        Guid reservaId,
        string? usuarioId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<FinalizarAtencionRespuesta>> FinalizarAtencionAsync(
        Guid organizacionId,
        Guid reservaId,
        FinalizarAtencionSolicitud solicitud,
        string? usuarioId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<MarcarNoAsistioRespuesta>> MarcarNoAsistioAsync(
        Guid organizacionId,
        Guid reservaId,
        string? usuarioId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<AtencionDetalleDto>> ObtenerDetalleAsync(
        Guid organizacionId,
        Guid reservaId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<AgendaProfesionalDto>> ObtenerAgendaProfesionalAsync(
        Guid organizacionId,
        Guid profesionalId,
        DateOnly fecha,
        CancellationToken ct = default);
}