using ReservaTuCitaYa.Application.DTOs.Atenciones;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IAtencionRepository
{
    Task<Reserva?> ObtenerReservaParaModificarAsync(
        Guid reservaId,
        CancellationToken ct = default);

    Task<Atencion?> ObtenerPorReservaIdAsync(
        Guid reservaId,
        CancellationToken ct = default);

    Task<bool> ExisteServicioActivoEnOrganizacionAsync(
        Guid servicioId,
        Guid organizacionId,
        CancellationToken ct = default);

    void Agregar(Atencion atencion);

    void AgregarHistorial(HistorialReserva historial);

    Task GuardarAsync(CancellationToken ct = default);

    Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion,
        CancellationToken ct = default);

    Task<AtencionDetalleDto?> ObtenerDetalleAsync(
        Guid reservaId,
        CancellationToken ct = default);

    Task<AgendaProfesionalDto?> ObtenerAgendaProfesionalAsync(
        Guid organizacionId,
        Guid profesionalId,
        DateOnly fecha,
        CancellationToken ct = default);
}
