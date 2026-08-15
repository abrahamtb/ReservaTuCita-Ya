using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ServicioRepository(ApplicationDbContext context) : IServicioRepository
{
    public async Task<PaginaResultado<ServicioListaDto>> ListarAsync(
        ServicioFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 50);
        var consulta = context.Servicios
            .AsNoTracking()
            .Where(servicio => servicio.OrganizacionId == filtro.OrganizacionId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            consulta = consulta.Where(servicio =>
                servicio.Nombre.Contains(busqueda) ||
                (servicio.Descripcion != null && servicio.Descripcion.Contains(busqueda)));
        }

        if (filtro.CategoriaServicioId.HasValue)
            consulta = consulta.Where(servicio => servicio.CategoriaServicioId == filtro.CategoriaServicioId);
        if (filtro.Modalidad.HasValue)
            consulta = consulta.Where(servicio => servicio.Modalidad == filtro.Modalidad);

        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(servicio => servicio.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(servicio => !servicio.EstaActivo),
            _ => consulta
        };

        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .OrderBy(servicio => servicio.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(servicio => new ServicioListaDto(
                servicio.Id,
                servicio.OrganizacionId,
                servicio.Nombre,
                servicio.CategoriaServicio.Nombre,
                servicio.Modalidad,
                servicio.DuracionMinutos,
                servicio.Precio,
                servicio.MontoAdelanto,
                servicio.EsGrupal,
                servicio.CapacidadMaxima,
                servicio.ServiciosSede.Count(relacion => relacion.EstaActivo),
                servicio.EstaActivo))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<ServicioListaDto>(elementos, pagina, tamano, total);
    }

    public Task<ServicioDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Servicios
            .AsNoTracking()
            .Where(servicio => servicio.Id == id)
            .Select(servicio => new ServicioDetalleDto(
                servicio.Id,
                servicio.OrganizacionId,
                servicio.Organizacion.NombreComercial,
                servicio.CategoriaServicioId,
                servicio.CategoriaServicio.Nombre,
                servicio.Nombre,
                servicio.Descripcion,
                servicio.DuracionMinutos,
                servicio.Precio,
                servicio.MontoAdelanto,
                servicio.Modalidad,
                servicio.EsGrupal,
                servicio.CapacidadMaxima,
                servicio.RequiereProfesional,
                servicio.RequiereRecurso,
                servicio.PermiteCancelacion,
                servicio.PermiteReprogramacion,
                servicio.HorasLimiteCancelacion,
                servicio.TiempoPreparacionMinutos,
                servicio.TiempoPosteriorMinutos,
                servicio.EstaActivo,
                servicio.FechaCreacion,
                servicio.FechaModificacion,
                servicio.CreadoPorUsuarioId,
                servicio.ModificadoPorUsuarioId,
                servicio.ServiciosSede
                    .Where(relacion => relacion.EstaActivo)
                    .OrderBy(relacion => relacion.Sede.Nombre)
                    .Select(relacion => new ServicioSedeDetalleDto(
                        relacion.SedeId,
                        relacion.Sede.Nombre,
                        relacion.Sede.EstaActivo,
                        relacion.PrecioEspecial,
                        relacion.PrecioEspecial ?? servicio.Precio))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Servicio?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Servicios
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(servicio => servicio.Id == id, cancellationToken);

    public Task<bool> ExisteNombreActivoAsync(
        Guid organizacionId,
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default) =>
        context.Servicios.AnyAsync(
            servicio => servicio.OrganizacionId == organizacionId &&
                        servicio.Nombre == nombre &&
                        servicio.EstaActivo &&
                        (!excluirId.HasValue || servicio.Id != excluirId.Value),
            cancellationToken);

    public async Task<IReadOnlyList<SedeAsignacionDto>> ListarSedesParaAsignarAsync(
        Guid organizacionId,
        Guid? servicioId = null,
        CancellationToken cancellationToken = default) =>
        await context.Sedes
            .AsNoTracking()
            .Where(sede => sede.OrganizacionId == organizacionId && sede.EstaActivo)
            .OrderBy(sede => sede.Nombre)
            .Select(sede => new SedeAsignacionDto(
                sede.Id,
                sede.Nombre,
                sede.EstaActivo,
                servicioId.HasValue && context.ServiciosSede.Any(relacion =>
                    relacion.ServicioId == servicioId.Value &&
                    relacion.SedeId == sede.Id &&
                    relacion.EstaActivo),
                servicioId.HasValue
                    ? context.ServiciosSede
                        .Where(relacion => relacion.ServicioId == servicioId.Value &&
                                           relacion.SedeId == sede.Id &&
                                           relacion.EstaActivo)
                        .Select(relacion => relacion.PrecioEspecial)
                        .FirstOrDefault()
                    : null))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(
        IReadOnlyCollection<Guid> sedeIds,
        CancellationToken cancellationToken = default) =>
        await context.Sedes
            .IgnoreQueryFilters()
            .Where(sede => sedeIds.Contains(sede.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ServicioSede>> ObtenerRelacionesSedeAsync(
        Guid servicioId,
        CancellationToken cancellationToken = default) =>
        await context.ServiciosSede
            .IgnoreQueryFilters()
            .Where(relacion => relacion.ServicioId == servicioId)
            .ToListAsync(cancellationToken);

    public Task<ServicioSede?> ObtenerServicioSedeAsync(
        Guid servicioId,
        Guid sedeId,
        CancellationToken cancellationToken = default) =>
        context.ServiciosSede
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                relacion =>
                    relacion.ServicioId == servicioId &&
                    relacion.SedeId == sedeId,
                cancellationToken);

    public void Agregar(Servicio servicio) => context.Servicios.Add(servicio);
    public void AgregarRelacion(ServicioSede servicioSede) => context.ServiciosSede.Add(servicioSede);

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
                "El servicio o una asignación de sede entra en conflicto con un registro existente.",
                exception);
        }
    }

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default)
    {
        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operacion(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
