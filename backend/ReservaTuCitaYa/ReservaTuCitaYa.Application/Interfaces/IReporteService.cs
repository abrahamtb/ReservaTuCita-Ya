using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Reportes;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IReporteService
{
    Task<ResultadoOperacion<ReporteReservasRespuestaDto>> ObtenerReservasAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        Guid? clienteId,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default);

    Task<ResultadoOperacion<ReporteIngresosRespuestaDto>> ObtenerIngresosAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? metodoPagoId,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default);

    Task<ResultadoOperacion<ReporteAtencionesRespuestaDto>> ObtenerAtencionesAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        ResultadoAtencion? resultado,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default);

    Task<ResultadoOperacion<byte[]>> ExportarReservasCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        Guid? clienteId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<byte[]>> ExportarIngresosCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? metodoPagoId,
        CancellationToken ct = default);

    Task<ResultadoOperacion<byte[]>> ExportarAtencionesCsvAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        Guid? profesionalId,
        Guid? servicioId,
        EstadoReserva? estado,
        ResultadoAtencion? resultado,
        CancellationToken ct = default);


}