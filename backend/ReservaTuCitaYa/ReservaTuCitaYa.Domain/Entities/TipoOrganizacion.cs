using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class TipoOrganizacion : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public ICollection<Organizacion> Organizaciones { get; set; } = new List<Organizacion>();
    }
}
