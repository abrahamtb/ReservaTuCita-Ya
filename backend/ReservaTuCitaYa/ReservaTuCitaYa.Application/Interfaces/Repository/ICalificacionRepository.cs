using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Interfaces.Repository
{
    public interface ICalificacionRepository
    {
        Task<Calificacion?> ObtenerPorReservaAsync(Guid reservaId);
        Task CrearAsync(Calificacion calificacion);
        Task<bool> ExistePorReservaAsync(Guid reservaId);
        Task<bool> ExistePorAtencionAsync(Guid atencionId);
        Task GuardarCambiosAsync();
    }
}
