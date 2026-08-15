using ReservaTuCitaYa.Domain.Common;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class ReservaParticipante : BaseEntity
{
    public Guid ReservaId { get; set; }
    public Guid? ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public bool EsTitular { get; set; }
    public string? Observaciones { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public Cliente? Cliente { get; set; }
}