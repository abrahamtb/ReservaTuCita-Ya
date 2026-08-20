using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Reportes;

public sealed record ReporteReservasFiltroDto(
    Guid OrganizacionId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    Guid? SedeId,
    Guid? ProfesionalId,
    Guid? ServicioId,
    EstadoReserva? Estado,
    Guid? ClienteId,
    int Pagina,
    int TamanoPagina);

public sealed record ReporteReservasIndicadoresDto(
    int TotalReservas,
    int ConfirmadasReprogramadas,
    int Atendidas,
    int Canceladas,
    int NoAsistieron);

public sealed record ReporteReservaEstadoDto(
    string Estado,
    int Cantidad);

public sealed record ReporteReservaFilaDto(
    Guid ReservaId,
    string Codigo,
    DateOnly Fecha,
    TimeOnly Hora,
    string Cliente,
    string Servicio,
    string Sede,
    string? Profesional,
    string Estado,
    int CantidadParticipantes,
    decimal PrecioTotal);

public sealed record ReporteReservasRespuestaDto(
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    ReporteReservasIndicadoresDto Indicadores,
    IReadOnlyList<ReporteReservaEstadoDto> ReservasPorEstado,
    IReadOnlyList<ReporteReservaFilaDto> Elementos,
    int PaginaActual,
    int TamanoPagina,
    int TotalElementos,
    int TotalPaginas);