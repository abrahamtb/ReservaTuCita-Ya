using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.DTOs.Reservas;

public sealed class ReprogramarReservaSolicitud
{
    public Guid ReservaId { get; init; }
    public DateOnly FechaNueva { get; init; }
    public TimeOnly HoraInicioNueva { get; init; }
    public Guid? ProfesionalId { get; init; }
    public Guid? RecursoId { get; init; }
    public MotivoReprogramacion Motivo { get; init; }
    public string? Observacion { get; init; }
}

public sealed record ProgramacionResumenDto(
    DateOnly Fecha, TimeOnly HoraInicio, TimeOnly? HoraFinServicio,
    Guid? ProfesionalId, string? ProfesionalNombre,
    Guid? RecursoId, string? RecursoNombre);

public sealed record ReprogramarReservaRespuesta(
    Guid Id, string Codigo, string Estado,
    ProgramacionResumenDto ProgramacionAnterior,
    ProgramacionResumenDto ProgramacionNueva);

public sealed record ReprogramacionHistorialDto(
    Guid Id, DateOnly FechaAnterior, TimeOnly HoraInicioAnterior,
    DateOnly FechaNueva, TimeOnly HoraInicioNueva,
    MotivoReprogramacion Motivo, string? Observacion, DateTime FechaReprogramacion);