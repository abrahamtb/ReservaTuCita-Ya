using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence
{
    public interface IOrganizacionRepository
    {
        Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default);

        Task<OrganizacionDetalleDto?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Organizacion?> ObtenerParaModificarAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> ExisteDocumentoAsync(
            string numeroDocumento,
            Guid? excluirId = null,
            CancellationToken cancellationToken = default);

        Task<bool> TipoValidoAsync(
            Guid tipoOrganizacionId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TipoOrganizacionOpcionDto>> ListarTiposActivosAsync(
            CancellationToken cancellationToken = default);

        void Agregar(Organizacion organizacion);

        Task GuardarAsync(CancellationToken cancellationToken = default);
    }
}
