using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class DisponibilidadRepository(ApplicationDbContext context) : IDisponibilidadRepository
{
    public Task<Sede?> ObtenerSedeAsync(Guid sedeId, CancellationToken ct = default) =>
        context.Sedes.AsNoTracking().SingleOrDefaultAsync(s => s.Id == sedeId, ct);

    public Task<Servicio?> ObtenerServicioAsync(Guid servicioId, CancellationToken ct = default) =>
        context.Servicios.AsNoTracking().SingleOrDefaultAsync(s => s.Id == servicioId, ct);

    public Task<ServicioSede?> ObtenerServicioSedeAsync(
        Guid servicioId, Guid sedeId, CancellationToken ct = default) =>
        context.ServiciosSede.AsNoTracking()
            .SingleOrDefaultAsync(s => s.ServicioId == servicioId && s.SedeId == sedeId, ct);

    public async Task<IReadOnlyList<Empleado>> ObtenerProfesionalesCompatiblesAsync(
        Guid servicioId, Guid sedeId, CancellationToken ct = default) =>
        await context.Empleados.AsNoTracking()
            .Where(e => e.EsProfesional && e.EstaActivo &&
                        context.Sedes.Where(s => s.Id == sedeId).Select(s => s.OrganizacionId).Contains(e.OrganizacionId) &&
                        e.Sedes.Any(r => r.SedeId == sedeId && r.EstaActivo) &&
                        e.ServiciosProfesionales.Any(r => r.ServicioId == servicioId && r.EstaActivo))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Recurso>> ObtenerRecursosCompatiblesAsync(
        Guid servicioId, Guid sedeId, CancellationToken ct = default) =>
        await context.Recurso.AsNoTracking()
            .Where(r => r.SedeId == sedeId && r.EstaActivo &&
                        context.Sedes.Where(s => s.Id == sedeId).Select(s => s.OrganizacionId).Contains(r.OrganizacionId) &&
                        r.Servicios.Any(s => s.ServicioId == servicioId && s.EstaActivo))
            .ToListAsync(ct);

    public Task<Empleado?> ObtenerProfesionalAsync(Guid empleadoId, CancellationToken ct = default) =>
        context.Empleados.AsNoTracking().SingleOrDefaultAsync(e => e.Id == empleadoId, ct);

    public Task<Recurso?> ObtenerRecursoAsync(Guid recursoId, CancellationToken ct = default) =>
        context.Recurso.AsNoTracking().SingleOrDefaultAsync(r => r.Id == recursoId, ct);

    public async Task<IReadOnlyList<HorarioSede>> ObtenerHorariosSedeAsync(
        Guid sedeId, CancellationToken ct = default) =>
        await context.HorarioSede.AsNoTracking()
            .Where(h => h.SedeId == sedeId && h.EstaActivo).ToListAsync(ct);

    public async Task<IReadOnlyList<ExcepcionHorarioSede>> ObtenerExcepcionesSedeAsync(
        Guid sedeId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await context.ExcepcionHorarioSede.AsNoTracking()
            .Where(e => e.SedeId == sedeId && e.Fecha >= desde && e.Fecha <= hasta)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HorarioProfesional>> ObtenerHorariosProfesionalesAsync(
        IReadOnlyCollection<Guid> empleadoIds, Guid sedeId, CancellationToken ct = default) =>
        await context.HorarioProfesional.AsNoTracking()
            .Where(h => empleadoIds.Contains(h.EmpleadoId) && h.SedeId == sedeId && h.EstaActivo)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExcepcionHorarioProfesional>> ObtenerExcepcionesProfesionalesAsync(
        IReadOnlyCollection<Guid> empleadoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await context.ExcepcionHorarioProfesional.AsNoTracking()
            .Where(e => empleadoIds.Contains(e.EmpleadoId) && e.Fecha >= desde && e.Fecha <= hasta)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HorarioRecurso>> ObtenerHorariosRecursosAsync(
        IReadOnlyCollection<Guid> recursoIds, CancellationToken ct = default) =>
        await context.HorarioRecurso.AsNoTracking()
            .Where(h => recursoIds.Contains(h.RecursoId) && h.EstaActivo).ToListAsync(ct);

    public async Task<IReadOnlyList<ExcepcionHorarioRecurso>> ObtenerExcepcionesRecursosAsync(
        IReadOnlyCollection<Guid> recursoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await context.ExcepcionesHorarioRecurso.AsNoTracking()
            .Where(e => recursoIds.Contains(e.RecursoId) && e.Fecha >= desde && e.Fecha <= hasta)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BloqueoRecurso>> ObtenerBloqueosAsync(
        IReadOnlyCollection<Guid> recursoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await context.BloqueoRecurso.AsNoTracking()
            .Where(b => recursoIds.Contains(b.RecursoId) && b.EstaActivo &&
                        b.FechaHoraInicio.Date <= hasta.ToDateTime(TimeOnly.MaxValue) &&
                        b.FechaHoraFin.Date >= desde.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Reserva>> ObtenerReservasActivasAsync(
        Guid sedeId, DateOnly desde, DateOnly hasta, Guid? excluirReservaId = null,
        CancellationToken ct = default)
    {
        var consulta = context.Reservas.AsNoTracking().Where(r =>
            r.SedeId == sedeId && r.Fecha >= desde && r.Fecha <= hasta &&
            EstadosReserva.OcupanHorario.Contains(r.EstadoReserva));
        if (excluirReservaId.HasValue)
            consulta = consulta.Where(r => r.Id != excluirReservaId.Value);
        return await consulta.ToListAsync(ct);
    }
}
