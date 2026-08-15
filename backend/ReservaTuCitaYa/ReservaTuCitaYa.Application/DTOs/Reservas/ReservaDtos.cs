using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.DTOs.Reservas;

public sealed class ParticipanteSolicitud
{
    public Guid? ClienteId { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public bool EsTitular { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearReservaSolicitud
{
    public Guid ClienteId { get; init; }
    public Guid ServicioId { get; init; }
    public Guid SedeId { get; init; }
    public Guid? ProfesionalId { get; init; }
    public Guid? RecursoId { get; init; }
    public DateOnly Fecha { get; init; }
    public TimeOnly HoraInicio { get; init; }
    public int CantidadParticipantes { get; init; }
    public IReadOnlyList<ParticipanteSolicitud> Participantes { get; init; } = [];
    public string? Observaciones { get; init; }
}

public sealed record EntidadResumenDto(Guid Id, string Nombre);

public sealed record ReservaCreadaDto(
    Guid Id, string Codigo, string Estado,
    EntidadResumenDto Cliente, EntidadResumenDto Servicio, EntidadResumenDto Sede,
    EntidadResumenDto? Profesional, EntidadResumenDto? Recurso,
    DateOnly Fecha, TimeOnly HoraInicio, TimeOnly HoraFinServicio,
    int DuracionMinutos, int CantidadParticipantes,
    decimal PrecioTotal, decimal? AdelantoRequerido);

public sealed record ParticipanteDto(
    Guid Id, Guid? ClienteId, string NombreCompleto, bool EsTitular, string? Observaciones);

public sealed record HistorialReservaDto(
    Guid Id, EstadoReserva? EstadoAnterior, EstadoReserva EstadoNuevo,
    TipoAccionReserva TipoAccion, string? Motivo, string? Observacion, DateTime FechaAccion);

public sealed record ReservaDetalleDto(
    Guid Id, Guid OrganizacionId, string Codigo, string Estado,
    EntidadResumenDto Cliente, EntidadResumenDto Servicio, EntidadResumenDto Sede,
    EntidadResumenDto? Profesional, EntidadResumenDto? Recurso,
    DateOnly Fecha, TimeOnly HoraInicio, TimeOnly HoraFinServicio,
    TimeOnly HoraInicioOcupacion, TimeOnly HoraFinOcupacion,
    int DuracionMinutos, int TiempoPreparacionMinutos, int TiempoPosteriorMinutos,
    int CantidadParticipantes, bool EsGrupal, int CapacidadMaxima,
    decimal PrecioTotal, decimal? AdelantoRequerido, string? Observaciones,
    IReadOnlyList<ParticipanteDto> Participantes,
    IReadOnlyList<HistorialReservaDto> Historial);

public sealed record ReservaListaDto(
    Guid Id, string Codigo, string ClienteNombre, string ServicioNombre, string SedeNombre,
    string? ProfesionalNombre, DateOnly Fecha, TimeOnly HoraInicio, TimeOnly HoraFinServicio,
    string Estado, int CantidadParticipantes);

public sealed record ReservaFiltroDto(
    Guid OrganizacionId, Guid? SedeId, Guid? ClienteId, Guid? ProfesionalId, Guid? ServicioId,
    EstadoReserva? Estado, DateOnly? Desde, DateOnly? Hasta, int Pagina, int TamanoPagina);
