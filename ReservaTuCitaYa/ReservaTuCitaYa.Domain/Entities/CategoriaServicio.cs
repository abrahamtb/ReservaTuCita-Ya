using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class CategoriaServicio : BaseEntity
    {
        public Guid OrganizacionId { get; set; }
        public Organizacion Organizacion { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}
