using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Repositories
{
    public sealed class CalificacionRepository : ICalificacionRepository
    {
        private readonly ApplicationDbContext _context;

        public CalificacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Calificacion?> ObtenerPorReservaAsync(Guid reservaId)
        {
            return await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.ReservaId == reservaId);
        }

        public async Task CrearAsync(Calificacion calificacion)
        {
            await _context.Calificaciones.AddAsync(calificacion);
        }

        public async Task<bool> ExistePorReservaAsync(Guid reservaId)
        {
            return await _context.Calificaciones.AnyAsync(c => c.ReservaId == reservaId);
        }

        public async Task<bool> ExistePorAtencionAsync(Guid atencionId)
        {
            return await _context.Calificaciones.AnyAsync(c => c.AtencionId == atencionId);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
