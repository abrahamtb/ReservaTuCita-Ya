using ReservaTuCitaYa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class UsuarioCliente : BaseEntity
    {
        public string UsuarioId { get; set; } = string.Empty;
        public Guid ClienteId { get; set; }
    }
}
