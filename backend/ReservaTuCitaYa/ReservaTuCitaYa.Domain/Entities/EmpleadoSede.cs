using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities;

public sealed class EmpleadoSede : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public Guid SedeId { get; set; }
    public Sede Sede { get; set; } = null!;
}
