using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence
{
    public interface IOrganizacionRepository
    {
        Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default);

        async Task<PaginaResultado<OrganizacionListaDto>> ListarPaginadoAsync(
            OrganizacionFiltroDto filtro,
            CancellationToken cancellationToken = default)
        {
            var pagina = Math.Max(1, filtro.Pagina);
            var tamano = Math.Clamp(filtro.TamanoPagina, 1, 50);
            var todos = await ListarAsync(filtro, cancellationToken);
            return new PaginaResultado<OrganizacionListaDto>(
                todos.Skip((pagina - 1) * tamano).Take(tamano).ToArray(),
                pagina,
                tamano,
                todos.Count);
        }

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
