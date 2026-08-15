using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.DTOs.Reservas;

public sealed class CancelarReservaSolicitud
{
    public Guid ReservaId { get; init; }
    public MotivoCancelacion Motivo { get; init; }
    public string? Comentario { get; init; }
    public bool Confirmacion { get; init; }
}

public sealed record CancelarReservaRespuesta(
    Guid ReservaId, string Codigo, string Estado,
    DateTime FechaCancelacion, MotivoCancelacion Motivo, string? PoliticaAplicada);