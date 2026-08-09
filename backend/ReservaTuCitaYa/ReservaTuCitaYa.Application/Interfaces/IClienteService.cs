using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IClienteService
{
    Task<ResultadoOperacion<PaginaResultado<ClienteListaDto>>> ListarAsync(
        ClienteFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<ClienteDetalleDto>> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearClienteSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarClienteSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id, bool estaActivo, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default);
}
