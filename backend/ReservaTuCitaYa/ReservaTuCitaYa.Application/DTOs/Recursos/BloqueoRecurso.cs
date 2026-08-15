// Application/DTOs/Recursos/BloqueoRecursoDtos.cs
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Recursos;

public sealed record BloqueoRecursoDto(
    Guid Id, Guid RecursoId, DateTime FechaHoraInicio, DateTime FechaHoraFin,
    TipoBloqueo TipoBloqueo, string Motivo, string? Observaciones);

public abstract class GuardarBloqueoSolicitud
{
    public DateTime FechaHoraInicio { get; init; }
    public DateTime FechaHoraFin { get; init; }
    public TipoBloqueo TipoBloqueo { get; init; }
    public string Motivo { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
}

public sealed class CrearBloqueoSolicitud : GuardarBloqueoSolicitud
{
    public Guid RecursoId { get; init; }
}

public sealed class ActualizarBloqueoSolicitud : GuardarBloqueoSolicitud
{
    public Guid Id { get; init; }
}