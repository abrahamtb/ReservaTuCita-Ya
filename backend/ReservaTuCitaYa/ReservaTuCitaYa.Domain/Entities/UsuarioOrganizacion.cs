using ReservaTuCitaYa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class UsuarioOrganizacion : BaseEntity
    {
        public string UsuarioId { get; set; } = string.Empty;
        public Guid OrganizacionId { get; set; }
        public bool EsPrincipal { get; set; }

        public Organizacion Organizacion { get; set; } = null!;
    }
}
