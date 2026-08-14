using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IReservaRepository
{
    Task<PaginaResultado<ReservaListaDto>> ListarAsync(
        ReservaFiltroDto filtro, CancellationToken ct = default);
    Task<ReservaDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken ct = default);
    Task<ReservaDetalleDto?> ObtenerDetallePorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Reserva?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);

    Task<IReadOnlyList<Reserva>> ObtenerConflictosAsync(
        Guid? profesionalId, Guid? recursoId, DateOnly fecha,
        Guid? excluirReservaId = null, CancellationToken ct = default);
    Task<int> ObtenerCapacidadOcupadaAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly horaInicio,
        Guid? profesionalId, Guid? recursoId, Guid? excluirReservaId = null, CancellationToken ct = default);

    void Agregar(Reserva reserva);
    void AgregarParticipante(ReservaParticipante participante);
    void AgregarHistorial(HistorialReserva historial);
    Task GuardarAsync(CancellationToken ct = default);
    Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion, CancellationToken ct = default);
}