using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class HistorialReserva : BaseEntity
{
    public Guid ReservaId { get; set; }
    public EstadoReserva? EstadoAnterior { get; set; }
    public EstadoReserva EstadoNuevo { get; set; }
    public TipoAccionReserva TipoAccion { get; set; }
    public string? Motivo { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaAccion { get; set; }
    public string? UsuarioId { get; set; }

    public Reserva Reserva { get; set; } = null!;
}