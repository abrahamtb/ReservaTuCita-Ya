using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IReservaService
{
    Task<ResultadoOperacion<ReservaCreadaDto>> CrearAsync(
        Guid organizacionId, CrearReservaSolicitud solicitud, string? usuarioId,
        CancellationToken ct = default);
    Task<ResultadoOperacion<ReservaDetalleDto>> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ResultadoOperacion<ReservaDetalleDto>> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<ResultadoOperacion<PaginaResultado<ReservaListaDto>>> ListarAsync(
        ReservaFiltroDto filtro, CancellationToken ct = default);
}