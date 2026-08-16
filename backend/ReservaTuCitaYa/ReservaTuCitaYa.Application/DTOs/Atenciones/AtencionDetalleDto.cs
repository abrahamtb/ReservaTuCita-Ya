using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Atenciones;

public sealed record AtencionDetalleDto(
    Guid Id,
    Guid ReservaId,
    Guid OrganizacionId,
    string CodigoReserva,
    string EstadoReserva,

    EntidadResumenDto Cliente,
    EntidadResumenDto Servicio,
    EntidadResumenDto Sede,
    EntidadResumenDto? Profesional,

    DateOnly Fecha,
    TimeOnly HoraInicioProgramada,
    TimeOnly HoraFinProgramada,

    DateTime? FechaHoraPresencia,
    DateTime? FechaHoraInicioReal,
    DateTime? FechaHoraFinReal,

    int? MinutosEspera,
    int? DuracionRealMinutos,

    ResultadoAtencion? Resultado,
    string? Observaciones,
    string? Recomendaciones,

    EntidadResumenDto? ProximoServicio,
    DateOnly? ProximaFechaSugerida);