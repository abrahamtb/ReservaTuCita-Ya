using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorUsuarioId { get; set; }
        public Guid? ModificadoPorUsuarioId { get; set; }
        public bool EstaActivo { get; set; } = true;
        public bool EstaEliminado { get; set; } = false;
    }
}
