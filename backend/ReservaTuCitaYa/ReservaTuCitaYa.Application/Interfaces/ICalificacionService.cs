using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Interfaces
{
    public interface ICalificacionService
    {
        Task<CalificacionDto> CrearCalificacionAsync(Guid reservaId, CrearCalificacionRequest request);
        Task<CalificacionDto?> ObtenerPorReservaAsync(Guid reservaId);
        Task<ResumenProfesionalDto> ObtenerResumenProfesionalAsync(Guid profesionalId);
        Task<IReadOnlyCollection<CalificacionDto>> ListarPorProfesionalAsync(Guid profesionalId, int pagina, int tamanoPagina, int? puntuacion);
    }
}
