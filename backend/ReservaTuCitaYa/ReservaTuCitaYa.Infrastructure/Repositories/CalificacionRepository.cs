using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class CalificacionRepository : ICalificacionRepository
{
    private readonly ApplicationDbContext _context;

    public CalificacionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Calificacion?> ObtenerPorReservaAsync(Guid reservaId) =>
        _context.Calificaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ReservaId == reservaId);

    public Task<Reserva?> ObtenerReservaParaCalificarAsync(Guid reservaId) =>
        _context.Reservas
            .Include(r => r.Atencion)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reservaId);

    public Task<Empleado?> ObtenerProfesionalAsync(Guid profesionalId) =>
        _context.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == profesionalId && e.EsProfesional);

    public async Task<IReadOnlyCollection<Calificacion>> ListarPorProfesionalAsync(
        Guid profesionalId,
        int pagina,
        int tamanoPagina,
        int? puntuacion)
    {
        var query = _context.Calificaciones
            .AsNoTracking()
            .Where(c => c.Reserva.ProfesionalId == profesionalId);

        if (puntuacion.HasValue)
        {
            query = query.Where(c => c.Puntuacion == puntuacion.Value);
        }

        return await query
            .OrderByDescending(c => c.FechaCalificacion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();
    }

    public Task<int> ContarPorProfesionalAsync(Guid profesionalId, int? puntuacion = null)
    {
        var query = _context.Calificaciones
            .AsNoTracking()
            .Where(c => c.Reserva.ProfesionalId == profesionalId);

        if (puntuacion.HasValue)
        {
            query = query.Where(c => c.Puntuacion == puntuacion.Value);
        }

        return query.CountAsync();
    }

    public async Task CrearAsync(Calificacion calificacion) =>
        await _context.Calificaciones.AddAsync(calificacion);

    public Task<bool> ExistePorReservaAsync(Guid reservaId) =>
        _context.Calificaciones.AnyAsync(c => c.ReservaId == reservaId);

    public Task<bool> ExistePorAtencionAsync(Guid atencionId) =>
        _context.Calificaciones.AnyAsync(c => c.AtencionId == atencionId);

    public Task GuardarCambiosAsync() => _context.SaveChangesAsync();
}
