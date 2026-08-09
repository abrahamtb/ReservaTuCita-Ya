using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class Sede : BaseEntity
    {
        public Guid OrganizacionId { get; set; }
        public Organizacion Organizacion { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Referencia { get; set; }

        public ICollection<ServicioSede> ServiciosSede { get; set; } = new List<ServicioSede>();
        public ICollection<EmpleadoSede> EmpleadosSede { get; set; } = new List<EmpleadoSede>();
    }
}
