using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Identity
{
    public class ApplicationUser: IdentityUser
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public Guid? OrganizacionId { get; set; }
        public bool EstaActivo { get; set; } = true;
        public DateTime? FechaUltimoAcceso { get; set; }
}
}
