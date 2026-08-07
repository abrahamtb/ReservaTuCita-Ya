using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface ICategoriaServicioRepository
{
    Task<PaginaResultado<CategoriaServicioListaDto>> ListarAsync(
        CategoriaServicioFiltroDto filtro,
        CancellationToken cancellationToken = default);

    Task<CategoriaServicioDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CategoriaServicio?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreActivoAsync(
        Guid organizacionId,
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TieneServiciosActivosAsync(
        Guid categoriaId,
        CancellationToken cancellationToken = default);

    Task<bool> TieneServiciosAsync(
        Guid categoriaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoriaServicioOpcionDto>> ListarActivasAsync(
        Guid organizacionId,
        CancellationToken cancellationToken = default);

    void Agregar(CategoriaServicio categoria);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
