using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class Recurso : BaseEntity
    {
        public Guid OrganizacionId { get; set; }
        public Guid SedeId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? UbicacionInterna { get; set; }
        public int Capacidad { get; set; }
        public EstadoRecurso EstadoRecurso { get; set; }

        // Relaciones
        public Organizacion Organizacion { get; set; } = null!;
        public Sede Sede { get; set; } = null!;
    }
}
