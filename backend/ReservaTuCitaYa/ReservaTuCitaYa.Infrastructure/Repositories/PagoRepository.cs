using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ReservaTuCitaYa.Infrastructure.Repositories
{
    public sealed class PagoRepository : IPagoRepository
    {
        private readonly ApplicationDbContext _context;

        public PagoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pago>> ListarPorReservaAsync(Guid reservaId)
            => await _context.Pagos
                .Include(p => p.MetodoPago)
                .Where(p => p.ReservaId == reservaId)
                .ToListAsync();

        public async Task<Pago?> ObtenerPorIdAsync(Guid id)
            => await _context.Pagos
                .Include(p => p.MetodoPago)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AgregarAsync(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Pago pago)
        {
            _context.Pagos.Update(pago);
            await _context.SaveChangesAsync();
        }
    }
}
