using System;
using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class ServicioRecurso : BaseEntity
    {
        public Guid ServicioId { get; set; }

        public Guid RecursoId { get; set; }

        public bool EsObligatorio { get; set; }

        public int CantidadRequerida { get; set; }

        // Relaciones
        public Servicio Servicio { get; set; } = null!;

        public Recurso Recurso { get; set; } = null!;
    }
}