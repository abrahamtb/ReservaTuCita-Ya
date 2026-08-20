using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Interfaces.Repository
{
    public interface IReembolsoRepository
    {
        Task<IEnumerable<ReembolsoReserva>> ListarPorReservaAsync(Guid reservaId);
        Task<ReembolsoReserva?> ObtenerPorIdAsync(Guid id);
        Task AgregarAsync(ReembolsoReserva reembolso);
    }
}
