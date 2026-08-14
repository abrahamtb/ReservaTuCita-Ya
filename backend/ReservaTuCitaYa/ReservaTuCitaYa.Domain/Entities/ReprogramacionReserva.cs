// Domain/Entities/ReprogramacionReserva.cs
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class ReprogramacionReserva : BaseEntity
{
    public Guid ReservaId { get; set; }

    public DateOnly FechaAnterior { get; set; }
    public TimeOnly HoraInicioAnterior { get; set; }
    public TimeOnly HoraFinServicioAnterior { get; set; }
    public TimeOnly HoraInicioOcupacionAnterior { get; set; }
    public TimeOnly HoraFinOcupacionAnterior { get; set; }
    public Guid? ProfesionalAnteriorId { get; set; }
    public Guid? RecursoAnteriorId { get; set; }

    public DateOnly FechaNueva { get; set; }
    public TimeOnly HoraInicioNueva { get; set; }
    public TimeOnly HoraFinServicioNueva { get; set; }
    public TimeOnly HoraInicioOcupacionNueva { get; set; }
    public TimeOnly HoraFinOcupacionNueva { get; set; }
    public Guid? ProfesionalNuevoId { get; set; }
    public Guid? RecursoNuevoId { get; set; }

    public MotivoReprogramacion Motivo { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaReprogramacion { get; set; }
    public string? UsuarioId { get; set; }

    //public Reserva Reserva { get; set; } = null!;
    public Empleado? ProfesionalAnterior { get; set; }
    public Empleado? ProfesionalNuevo { get; set; }
    public Recurso? RecursoAnterior { get; set; }
    public Recurso? RecursoNuevo { get; set; }
}