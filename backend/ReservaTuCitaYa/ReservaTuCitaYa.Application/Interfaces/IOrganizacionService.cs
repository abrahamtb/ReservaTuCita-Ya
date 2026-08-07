using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;

namespace ReservaTuCitaYa.Application.Interfaces
{
    public interface IOrganizacionService
    {
        Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default);

        Task<PaginaResultado<OrganizacionListaDto>> ListarPaginadoAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<OrganizacionDetalleDto>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TipoOrganizacionOpcionDto>> ListarTiposActivosAsync(
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<Guid>> CrearAsync(
            CrearOrganizacionSolicitud solicitud,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> ActualizarAsync(
            ActualizarOrganizacionSolicitud solicitud,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> CambiarEstadoAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
