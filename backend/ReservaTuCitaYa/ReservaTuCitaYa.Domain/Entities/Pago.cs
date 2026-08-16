using ReservaTuCitaYa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Entities
{
    public sealed class Pago : BaseEntity
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public Guid ReservaId { get; set; }
        public Guid MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
        public DateOnly FechaPago { get; set; }
        public string? NumeroOperacion { get; set; }
        public string? Observacion { get; set; }

        public bool EstaAnulado { get; set; } = false;
        public DateTime? FechaAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }
        public string? UsuarioAnulacionId { get; set; }

        public MetodoPago MetodoPago { get; set; } = null!;
    }
}
