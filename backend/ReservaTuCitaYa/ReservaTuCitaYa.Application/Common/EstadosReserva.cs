using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Common;

public static class EstadosReserva
{
    public static readonly IReadOnlySet<EstadoReserva> OcupanHorario = new HashSet<EstadoReserva>
    {
        EstadoReserva.Pendiente, EstadoReserva.Confirmada,
        EstadoReserva.Presente, EstadoReserva.EnAtencion, EstadoReserva.Reprogramada
    };
}
