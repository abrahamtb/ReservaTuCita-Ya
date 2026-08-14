using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class CancelacionReserva : BaseEntity
{
    public Guid ReservaId { get; set; }
    public MotivoCancelacion Motivo { get; set; }
    public string? Comentario { get; set; }
    public string? PoliticaAplicada { get; set; }
    public DateTime FechaCancelacion { get; set; }
    public string? UsuarioId { get; set; }
    public Reserva Reserva { get; set; } = null!;
}