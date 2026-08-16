using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Reportes;

public sealed record ReporteIngresosFiltroDto(
    Guid OrganizacionId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    Guid? SedeId,
    Guid? MetodoPagoId,
    int Pagina,
    int TamanoPagina);

public sealed record ReporteIngresosIndicadoresDto(
    decimal IngresosBrutos,
    decimal Reembolsos,
    decimal IngresosNetos,
    int CantidadPagos,
    decimal? TicketPromedio);

public sealed record ReporteMovimientoFilaDto(
    DateOnly Fecha,
    string CodigoMovimiento,
    string CodigoReserva,
    string Cliente,
    string Sede,
    string Tipo,
    string? Metodo,
    string? NumeroOperacion,
    decimal Monto);

public sealed record ReporteIngresosRespuestaDto(
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    ReporteIngresosIndicadoresDto Indicadores,
    IReadOnlyList<ReporteMovimientoFilaDto> Elementos,
    int PaginaActual,
    int TamanoPagina,
    int TotalElementos,
    int TotalPaginas);
