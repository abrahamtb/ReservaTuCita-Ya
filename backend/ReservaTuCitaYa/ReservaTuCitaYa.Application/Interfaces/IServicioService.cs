using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IServicioService
{
    Task<ResultadoOperacion<PaginaResultado<ServicioListaDto>>> ListarAsync(
        ServicioFiltroDto filtro,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<ServicioDetalleDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<IReadOnlyList<SedeAsignacionDto>>> ObtenerSedesAsignadasAsync(
        Guid organizacionId,
        Guid? servicioId = null,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
