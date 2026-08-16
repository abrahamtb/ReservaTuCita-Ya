using ReservaTuCitaYa.Application.DTOs.Reportes;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IReporteRepository
{
    Task<ReporteReservasRespuestaDto> ObtenerReservasAsync(
        ReporteReservasFiltroDto filtro,
        CancellationToken ct = default);

    Task<ReporteIngresosRespuestaDto> ObtenerIngresosAsync(
        ReporteIngresosFiltroDto filtro,
        CancellationToken ct = default);

    Task<ReporteAtencionesRespuestaDto> ObtenerAtencionesAsync(
        ReporteAtencionesFiltroDto filtro,
        CancellationToken ct = default);

    Task<IReadOnlyList<ReporteReservaFilaDto>> ExportarReservasAsync(
        ReporteReservasFiltroDto filtro,
        CancellationToken ct = default);

    Task<IReadOnlyList<ReporteMovimientoFilaDto>> ExportarIngresosAsync(
        ReporteIngresosFiltroDto filtro,
        CancellationToken ct = default);

    Task<IReadOnlyList<ReporteAtencionFilaDto>> ExportarAtencionesAsync(
        ReporteAtencionesFiltroDto filtro,
        CancellationToken ct = default);
}