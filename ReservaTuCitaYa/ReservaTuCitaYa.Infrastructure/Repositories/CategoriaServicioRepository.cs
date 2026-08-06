using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class CategoriaServicioRepository(ApplicationDbContext context)
    : ICategoriaServicioRepository
{
    public async Task<PaginaResultado<CategoriaServicioListaDto>> ListarAsync(
        CategoriaServicioFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 50);
        var consulta = context.CategoriasServicio
            .AsNoTracking()
            .Where(categoria => categoria.OrganizacionId == filtro.OrganizacionId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            consulta = consulta.Where(categoria =>
                categoria.Nombre.Contains(busqueda) ||
                (categoria.Descripcion != null && categoria.Descripcion.Contains(busqueda)));
        }

        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(categoria => categoria.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(categoria => !categoria.EstaActivo),
            _ => consulta
        };

        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .OrderBy(categoria => categoria.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(categoria => new CategoriaServicioListaDto(
                categoria.Id,
                categoria.OrganizacionId,
                categoria.Organizacion.NombreComercial,
                categoria.Nombre,
                categoria.Descripcion,
                categoria.Servicios.Count(),
                categoria.EstaActivo))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<CategoriaServicioListaDto>(elementos, pagina, tamano, total);
    }

    public Task<CategoriaServicioDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.CategoriasServicio
            .AsNoTracking()
            .Where(categoria => categoria.Id == id)
            .Select(categoria => new CategoriaServicioDetalleDto(
                categoria.Id,
                categoria.OrganizacionId,
                categoria.Organizacion.NombreComercial,
                categoria.Nombre,
                categoria.Descripcion,
                categoria.EstaActivo,
                categoria.FechaCreacion,
                categoria.FechaModificacion,
                categoria.CreadoPorUsuarioId,
                categoria.ModificadoPorUsuarioId,
                categoria.Servicios.Count(),
                categoria.Servicios.Count(servicio => servicio.EstaActivo)))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<CategoriaServicio?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.CategoriasServicio
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(categoria => categoria.Id == id, cancellationToken);

    public Task<bool> ExisteNombreActivoAsync(
        Guid organizacionId,
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default) =>
        context.CategoriasServicio.AnyAsync(
            categoria => categoria.OrganizacionId == organizacionId &&
                         categoria.Nombre == nombre &&
                         categoria.EstaActivo &&
                         (!excluirId.HasValue || categoria.Id != excluirId.Value),
            cancellationToken);

    public Task<bool> TieneServiciosActivosAsync(
        Guid categoriaId,
        CancellationToken cancellationToken = default) =>
        context.Servicios.AnyAsync(
            servicio => servicio.CategoriaServicioId == categoriaId && servicio.EstaActivo,
            cancellationToken);

    public Task<bool> TieneServiciosAsync(
        Guid categoriaId,
        CancellationToken cancellationToken = default) =>
        context.Servicios.AnyAsync(
            servicio => servicio.CategoriaServicioId == categoriaId,
            cancellationToken);

    public async Task<IReadOnlyList<CategoriaServicioOpcionDto>> ListarActivasAsync(
        Guid organizacionId,
        CancellationToken cancellationToken = default) =>
        await context.CategoriasServicio
            .AsNoTracking()
            .Where(categoria => categoria.OrganizacionId == organizacionId && categoria.EstaActivo)
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaServicioOpcionDto(categoria.Id, categoria.Nombre))
            .ToListAsync(cancellationToken);

    public void Agregar(CategoriaServicio categoria) => context.CategoriasServicio.Add(categoria);

    public async Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictoPersistenciaException(
                "La categoría entra en conflicto con un registro existente.", exception);
        }
    }
}
