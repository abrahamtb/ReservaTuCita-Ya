using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class EmpleadoRepository(ApplicationDbContext context) : IEmpleadoRepository
{
    public async Task<PaginaResultado<EmpleadoListaDto>> ListarAsync(
        EmpleadoFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.Empleados.AsNoTracking()
            .Where(empleado => empleado.OrganizacionId == filtro.OrganizacionId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            consulta = consulta.Where(empleado =>
                empleado.NumeroDocumento.Contains(busqueda) ||
                empleado.Nombres.Contains(busqueda) ||
                empleado.Apellidos.Contains(busqueda) ||
                (empleado.Nombres + " " + empleado.Apellidos).Contains(busqueda) ||
                (empleado.Apellidos + " " + empleado.Nombres).Contains(busqueda) ||
                (empleado.Correo != null && empleado.Correo.Contains(busqueda)) ||
                (empleado.Telefono != null && empleado.Telefono.Contains(busqueda)) ||
                empleado.Cargo.Contains(busqueda) ||
                (empleado.Especialidad != null && empleado.Especialidad.Contains(busqueda)));
        }

        if (filtro.TipoDocumento.HasValue)
            consulta = consulta.Where(e => e.TipoDocumento == filtro.TipoDocumento.Value);
        if (filtro.EsProfesional.HasValue)
            consulta = consulta.Where(e => e.EsProfesional == filtro.EsProfesional.Value);
        if (filtro.SedeId.HasValue)
            consulta = consulta.Where(e => e.Sedes.Any(r =>
                r.SedeId == filtro.SedeId.Value && r.EstaActivo));
        if (filtro.ServicioId.HasValue)
            consulta = consulta.Where(e => e.ServiciosProfesionales.Any(r =>
                r.ServicioId == filtro.ServicioId.Value && r.EstaActivo));

        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(e => e.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(e => !e.EstaActivo),
            _ => consulta
        };

        var total = await consulta.CountAsync(cancellationToken);
        var elementos = await consulta
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres).ThenBy(e => e.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(e => new EmpleadoListaDto(
                e.Id, e.OrganizacionId, e.TipoDocumento, e.NumeroDocumento,
                e.Nombres, e.Apellidos, e.Nombres + " " + e.Apellidos,
                e.Correo, e.Telefono, e.Cargo, e.Especialidad, e.EsProfesional,
                e.Sedes.Count(r => r.EstaActivo),
                e.ServiciosProfesionales.Count(r => r.EstaActivo), e.EstaActivo))
            .ToListAsync(cancellationToken);
        return new PaginaResultado<EmpleadoListaDto>(elementos, pagina, tamano, total);
    }

    public Task<EmpleadoDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Empleados.AsNoTracking().Where(e => e.Id == id)
            .Select(e => new EmpleadoDetalleDto(
                e.Id, e.OrganizacionId, e.TipoDocumento, e.NumeroDocumento,
                e.Nombres, e.Apellidos, e.Nombres + " " + e.Apellidos,
                e.Correo, e.Telefono, e.Direccion, e.FechaNacimiento, e.Cargo,
                e.Especialidad, e.EsProfesional, e.NumeroColegiatura, e.Observaciones,
                e.EstaActivo, e.FechaCreacion, e.FechaModificacion,
                e.CreadoPorUsuarioId, e.ModificadoPorUsuarioId,
                e.Sedes.Where(r => r.EstaActivo).OrderBy(r => r.Sede.Nombre)
                    .Select(r => new EmpleadoSedeDto(
                        r.Id, r.SedeId, r.Sede.Nombre, r.Sede.EstaActivo)).ToList(),
                e.ServiciosProfesionales.Where(r => r.EstaActivo)
                    .OrderBy(r => r.Servicio.Nombre)
                    .Select(r => new ProfesionalServicioDto(
                        r.Id, r.ServicioId, r.Servicio.Nombre, r.Servicio.EstaActivo)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Empleado?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        context.Empleados.IgnoreQueryFilters()
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(
        Guid organizacionId, TipoDocumento tipoDocumento, string numeroDocumento,
        Guid? excluirId = null, CancellationToken cancellationToken = default) =>
        context.Empleados.IgnoreQueryFilters().AnyAsync(e =>
            e.OrganizacionId == organizacionId && e.TipoDocumento == tipoDocumento &&
            e.NumeroDocumento == numeroDocumento &&
            (!excluirId.HasValue || e.Id != excluirId.Value), cancellationToken);

    public async Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        await context.Sedes.IgnoreQueryFilters().Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Servicio>> ObtenerServiciosParaValidarAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        await context.Servicios.IgnoreQueryFilters().Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EmpleadoSede>> ObtenerRelacionesSedeAsync(
        Guid empleadoId, CancellationToken cancellationToken = default) =>
        await context.EmpleadosSede.IgnoreQueryFilters()
            .Where(r => r.EmpleadoId == empleadoId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProfesionalServicio>> ObtenerRelacionesServicioAsync(
        Guid empleadoId, CancellationToken cancellationToken = default) =>
        await context.ProfesionalesServicio.IgnoreQueryFilters()
            .Where(r => r.EmpleadoId == empleadoId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EmpleadoSedeDto>> ListarSedesAsync(
        Guid empleadoId, CancellationToken cancellationToken = default) =>
        await context.EmpleadosSede.AsNoTracking()
            .Where(r => r.EmpleadoId == empleadoId && r.EstaActivo)
            .OrderBy(r => r.Sede.Nombre)
            .Select(r => new EmpleadoSedeDto(r.Id, r.SedeId, r.Sede.Nombre, r.Sede.EstaActivo))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProfesionalServicioDto>> ListarServiciosAsync(
        Guid empleadoId, CancellationToken cancellationToken = default) =>
        await context.ProfesionalesServicio.AsNoTracking()
            .Where(r => r.EmpleadoId == empleadoId && r.EstaActivo)
            .OrderBy(r => r.Servicio.Nombre)
            .Select(r => new ProfesionalServicioDto(
                r.Id, r.ServicioId, r.Servicio.Nombre, r.Servicio.EstaActivo))
            .ToListAsync(cancellationToken);

    public Task<bool> TieneServiciosActivosAsync(
        Guid empleadoId, CancellationToken cancellationToken = default) =>
        context.ProfesionalesServicio.AnyAsync(
            r => r.EmpleadoId == empleadoId && r.EstaActivo, cancellationToken);

    public void Agregar(Empleado empleado) => context.Empleados.Add(empleado);
    public void AgregarRelacion(EmpleadoSede relacion) => context.EmpleadosSede.Add(relacion);
    public void AgregarRelacion(ProfesionalServicio relacion) =>
        context.ProfesionalesServicio.Add(relacion);

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
                "El empleado o una de sus asignaciones entra en conflicto con un registro existente.",
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
