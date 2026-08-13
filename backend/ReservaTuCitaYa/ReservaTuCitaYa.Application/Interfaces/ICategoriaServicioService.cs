using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface ICategoriaServicioService
{
    Task<ResultadoOperacion<PaginaResultado<CategoriaServicioListaDto>>> ListarAsync(
        CategoriaServicioFiltroDto filtro,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<CategoriaServicioDetalleDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaServicioOpcionDto>> ListarActivasAsync(
        Guid organizacionId,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearCategoriaServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarCategoriaServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id,
        bool confirmarServiciosActivos,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
