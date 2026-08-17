using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Calificaciones
{
    public sealed class CrearCalificacionRequest
    {
        public int Puntuacion { get; set; } 
        public string? Comentario { get; set; } 
    }

    public sealed class CalificacionDto
    {
        public Guid Id { get; set; }
        public Guid ReservaId { get; set; }
        public int Puntuacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCalificacion { get; set; }
    }

    public sealed class ResumenProfesionalDto
    {
        public Guid ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; } = string.Empty;
        public double? Promedio { get; set; }
        public int TotalCalificaciones { get; set; }
        public IReadOnlyCollection<DistribucionEstrellasDto> Distribucion { get; set; } = Array.Empty<DistribucionEstrellasDto>();
    }

    public sealed class DistribucionEstrellasDto
    {
        public int Estrellas { get; set; }
        public int Cantidad { get; set; }
    }
}
