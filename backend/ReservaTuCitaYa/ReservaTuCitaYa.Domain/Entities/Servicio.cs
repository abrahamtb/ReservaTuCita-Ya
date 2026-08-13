using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class Servicio : BaseEntity
    {
        public Guid OrganizacionId { get; set; }
        public Organizacion Organizacion { get; set; } = null!;
        public Guid CategoriaServicioId { get; set; }
        public CategoriaServicio CategoriaServicio { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public decimal MontoAdelanto { get; set; }
        public ModalidadServicio Modalidad { get; set; }
        public bool EsGrupal { get; set; }
        public int CapacidadMaxima { get; set; } = 1;
        public bool RequiereProfesional { get; set; }
        public bool RequiereRecurso { get; set; }
        public bool PermiteCancelacion { get; set; }
        public bool PermiteReprogramacion { get; set; }
        public int HorasLimiteCancelacion { get; set; }
        public int TiempoPreparacionMinutos { get; set; }
        public int TiempoPosteriorMinutos { get; set; }

        public ICollection<ServicioSede> ServiciosSede { get; set; } = new List<ServicioSede>();
        public ICollection<ProfesionalServicio> ProfesionalesServicio { get; set; } =
            new List<ProfesionalServicio>();
    }
}
