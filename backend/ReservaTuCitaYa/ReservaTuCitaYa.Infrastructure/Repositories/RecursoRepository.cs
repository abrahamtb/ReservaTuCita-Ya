using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class RecursoRepository(ApplicationDbContext context) : IRecursoRepository
{
    public async Task<PaginaResultado<RecursoListaDto>> ListarAsync(
        RecursoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.Recurso.AsNoTracking()
            .Where(r => r.SedeId == filtro.SedeId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            consulta = consulta.Where(r =>
                r.Nombre.Contains(busqueda) ||
                (r.Codigo != null && r.Codigo.Contains(busqueda)) ||
                (r.Descripcion != null && r.Descripcion.Contains(busqueda)));
        }
        if (!string.IsNullOrWhiteSpace(filtro.TipoRecurso))
            consulta = consulta.Where(r => r.TipoRecurso == filtro.TipoRecurso);
        if (filtro.ServicioId.HasValue)
            consulta = consulta.Where(r => r.Servicios.Any(s =>
                s.ServicioId == filtro.ServicioId.Value && s.EstaActivo));

        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(r => r.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(r => !r.EstaActivo),
            _ => consulta
        };

        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .OrderBy(r => r.Nombre).ThenBy(r => r.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(r => new RecursoListaDto(
                r.Id, r.SedeId, r.Nombre, r.Codigo, r.TipoRecurso, r.Capacidad,
                r.UbicacionInterna, r.Servicios.Count(s => s.EstaActivo), r.EstaActivo))
            .ToListAsync(cancellationToken);
        return new PaginaResultado<RecursoListaDto>(elementos, pagina, tamano, total);
    }

    public Task<RecursoDetalleDto?> ObtenerDetalleAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        context.Recurso.AsNoTracking().Where(r => r.Id == id)
            .Select(r => new RecursoDetalleDto(
                r.Id, r.OrganizacionId, r.SedeId, r.Sede.Nombre,
                r.Nombre, r.Codigo, r.Descripcion, r.TipoRecurso, r.Capacidad,
                r.UbicacionInterna, r.Observaciones, r.EstaActivo,
                r.FechaCreacion, r.FechaModificacion,
                r.Servicios.Where(s => s.EstaActivo).OrderBy(s => s.Servicio.Nombre)
                    .Select(s => new RecursoServicioDto(
                        s.Id, s.ServicioId, s.Servicio.Nombre, s.EsObligatorio,
                        s.CantidadRequerida, s.Servicio.EstaActivo)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Recurso?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        context.Recurso.IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExisteCodigoAsync(
        Guid sedeId, string codigo, Guid? excluirId = null,
        CancellationToken cancellationToken = default) =>
        context.Recurso.IgnoreQueryFilters().AnyAsync(r =>
            r.SedeId == sedeId && r.Codigo == codigo &&
            (!excluirId.HasValue || r.Id != excluirId.Value), cancellationToken);

    // Ajusta "ServiciosSede" al nombre real de tu DbSet/relación de RG-017
    public async Task<IReadOnlyList<Servicio>> ObtenerServiciosParaValidarAsync(
        Guid sedeId, IReadOnlyCollection<Guid> servicioIds,
        CancellationToken cancellationToken = default) =>
        await context.Servicios.IgnoreQueryFilters()
            .Where(s => servicioIds.Contains(s.Id) &&
                        context.ServiciosSede.Any(ss => ss.ServicioId == s.Id && ss.SedeId == sedeId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ServicioRecurso>> ObtenerRelacionesServicioAsync(
        Guid recursoId, CancellationToken cancellationToken = default) =>
        await context.ServiciosRecurso.IgnoreQueryFilters()
            .Where(s => s.RecursoId == recursoId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RecursoServicioDto>> ListarServiciosAsync(
        Guid recursoId, CancellationToken cancellationToken = default) =>
        await context.ServiciosRecurso.AsNoTracking()
            .Where(s => s.RecursoId == recursoId && s.EstaActivo)
            .OrderBy(s => s.Servicio.Nombre)
            .Select(s => new RecursoServicioDto(
                s.Id, s.ServicioId, s.Servicio.Nombre, s.EsObligatorio,
                s.CantidadRequerida, s.Servicio.EstaActivo))
            .ToListAsync(cancellationToken);

    public void Agregar(Recurso recurso) => context.Recurso.Add(recurso);
    public void AgregarRelacion(ServicioRecurso relacion) => context.ServiciosRecurso.Add(relacion);

    public async Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictoPersistenciaException(
                "El recurso o una de sus asignaciones entra en conflicto con un registro existente.",
                exception);
        }
    }

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion, CancellationToken cancellationToken = default)
    {
        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operacion(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch { await transaccion.RollbackAsync(cancellationToken); throw; }
    }
}