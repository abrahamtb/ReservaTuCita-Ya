using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class Organizacion : BaseEntity
    {
        public Guid TipoOrganizacionId { get; set; }
        public TipoOrganizacion TipoOrganizacion { get; set; } = null!;
        public string NombreComercial { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? DireccionPrincipal { get; set; }
        public string? LogoUrl { get; set; }

        public ICollection<Sede> Sedes { get; set; } = new List<Sede>();
    }
}
