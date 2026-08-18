using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Domain.Entities;

public sealed class Calificacion : BaseEntity
{
    public Guid ReservaId { get; set; }
    public Guid AtencionId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime FechaCalificacion { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public Atencion Atencion { get; set; } = null!;
}
