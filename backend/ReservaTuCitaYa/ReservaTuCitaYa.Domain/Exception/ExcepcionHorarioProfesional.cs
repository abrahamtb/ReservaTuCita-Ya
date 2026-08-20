using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class ExcepcionHorarioProfesional : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Guid SedeId { get; set; }
    public DateOnly Fecha { get; set; }
    public TipoExcepcionHorario TipoExcepcion { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public Sede Sede { get; set; } = null!;
}