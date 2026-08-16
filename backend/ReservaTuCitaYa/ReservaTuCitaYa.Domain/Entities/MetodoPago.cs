using ReservaTuCitaYa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReservaTuCitaYa.Domain.Common.Permissions;

namespace ReservaTuCitaYa.Domain.Entities
{
    public sealed class MetodoPago : BaseEntity
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool RequiereNumeroOperacion { get; set; }
        public bool EstaActivo { get; set; } = true;

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public ICollection<ReembolsoReserva> Reembolsos { get; set; } = new List<ReembolsoReserva>();
    }
}
