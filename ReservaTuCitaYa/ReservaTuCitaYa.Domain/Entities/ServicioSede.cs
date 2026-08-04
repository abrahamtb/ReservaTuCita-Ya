using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities
{
    public class ServicioSede : BaseEntity
    {
        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
        public Guid SedeId { get; set; }
        public Sede Sede { get; set; } = null!;
        public decimal? PrecioEspecial { get; set; }
    }
}
