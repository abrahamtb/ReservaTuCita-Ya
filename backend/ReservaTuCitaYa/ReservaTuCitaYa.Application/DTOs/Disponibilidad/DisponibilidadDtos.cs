namespace ReservaTuCitaYa.Application.DTOs.Disponibilidad;

public sealed record ConsultarDisponibilidadSolicitud(
    Guid SedeId, Guid ServicioId, DateOnly FechaDesde, DateOnly FechaHasta,
    Guid? ProfesionalId, Guid? RecursoId);

public sealed record HorarioDisponibleDto(
    TimeOnly HoraInicio, TimeOnly HoraFinServicio, TimeOnly HoraFinOcupacion,
    Guid? ProfesionalId, string? ProfesionalNombre,
    Guid? RecursoId, string? RecursoNombre,
    int? CapacidadDisponible);

public sealed record DisponibilidadDiaDto(
    DateOnly Fecha, bool EstaDisponible, IReadOnlyList<HorarioDisponibleDto> Horarios);

public sealed record DisponibilidadRespuestaDto(
    Guid SedeId, Guid ServicioId, int DuracionMinutos,
    int TiempoPreparacionMinutos, int TiempoPosteriorMinutos,
    IReadOnlyList<DisponibilidadDiaDto> Dias);

public sealed record ProfesionalDisponibleDto(Guid Id, string NombreCompleto);
public sealed record RecursoDisponibleDto(Guid Id, string Nombre);