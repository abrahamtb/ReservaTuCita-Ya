using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities;

public sealed class Atencion : BaseEntity
{
    public Guid ReservaId { get; set; }

    public DateTime? FechaHoraPresencia { get; set; }
    public DateTime? FechaHoraInicioReal { get; set; }
    public DateTime? FechaHoraFinReal { get; set; }

    public ResultadoAtencion? ResultadoAtencion { get; set; }

    public string? Observaciones { get; set; }
    public string? Recomendaciones { get; set; }

    public Guid? ProximoServicioId { get; set; }
    public DateOnly? ProximaFechaSugerida { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public Servicio? ProximoServicio { get; set; }
}