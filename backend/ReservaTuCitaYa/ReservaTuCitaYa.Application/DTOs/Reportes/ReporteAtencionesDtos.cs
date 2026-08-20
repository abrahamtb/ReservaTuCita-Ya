using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Reportes;

public sealed record ReporteAtencionesFiltroDto(
    Guid OrganizacionId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    Guid? SedeId,
    Guid? ProfesionalId,
    Guid? ServicioId,
    EstadoReserva? Estado,
    ResultadoAtencion? Resultado,
    int Pagina,
    int TamanoPagina);

public sealed record ReporteAtencionesIndicadoresDto(
    int ReservasProgramadas,
    int Atendidas,
    int NoAsistieron,
    int AtencionParcialInterrumpida,
    decimal? PorcentajeAsistencia,
    bool SinDatos);

public sealed record ReporteAtencionFilaDto(
    Guid ReservaId,
    string CodigoReserva,
    DateOnly Fecha,
    TimeOnly HoraProgramada,
    string Cliente,
    string Servicio,
    string? Profesional,
    DateTime? HoraLlegada,
    DateTime? HoraInicioReal,
    DateTime? HoraFinReal,
    int? DuracionRealMinutos,
    string? Resultado,
    string Estado);

public sealed record ReporteAtencionesRespuestaDto(
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    ReporteAtencionesIndicadoresDto Indicadores,
    IReadOnlyList<ReporteAtencionFilaDto> Elementos,
    int PaginaActual,
    int TamanoPagina,
    int TotalElementos,
    int TotalPaginas);