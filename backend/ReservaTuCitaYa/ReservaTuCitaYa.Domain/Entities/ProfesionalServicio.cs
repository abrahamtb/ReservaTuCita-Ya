using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities;

public sealed class ProfesionalServicio : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public Guid ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;
}
