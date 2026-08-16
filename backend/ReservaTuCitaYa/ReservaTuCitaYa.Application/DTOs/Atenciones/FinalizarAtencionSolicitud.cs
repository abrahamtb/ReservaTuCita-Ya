using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Atenciones;

public sealed class FinalizarAtencionSolicitud
{
    public ResultadoAtencion Resultado { get; init; }

    public string? Observaciones { get; init; }

    public string? Recomendaciones { get; init; }

    public Guid? ProximoServicioId { get; init; }

    public DateOnly? ProximaFechaSugerida { get; init; }
}