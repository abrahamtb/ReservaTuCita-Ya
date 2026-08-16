using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ReservaTuCitaYa.Infrastructure.Repositories
{
    public sealed class ReembolsoRepository : IReembolsoRepository
    {
        private readonly ApplicationDbContext _context;

        public ReembolsoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReembolsoReserva>> ListarPorReservaAsync(Guid reservaId)
            => await _context.ReembolsosReserva
                .Include(r => r.MetodoPago)
                .Where(r => r.ReservaId == reservaId)
                .ToListAsync();

        public async Task<ReembolsoReserva?> ObtenerPorIdAsync(Guid id)
            => await _context.ReembolsosReserva
                .Include(r => r.MetodoPago)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task AgregarAsync(ReembolsoReserva reembolso)
        {
            _context.ReembolsosReserva.Add(reembolso);
            await _context.SaveChangesAsync();
        }
    }
}
