using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Reportes;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class ReportesController(
    IReporteService reporteService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("api/reportes/reservas")]
    public async Task<ActionResult<ReporteReservasRespuestaDto>> Reservas(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? profesionalId,
        [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado,
        [FromQuery] Guid? clienteId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ObtenerReservasAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            clienteId,
            pagina,
            tamanoPagina,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reportes/ingresos")]
    public async Task<ActionResult<ReporteIngresosRespuestaDto>> Ingresos(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? metodoPagoId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ObtenerIngresosAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            metodoPagoId,
            pagina,
            tamanoPagina,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reportes/atenciones")]
    public async Task<ActionResult<ReporteAtencionesRespuestaDto>> Atenciones(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? profesionalId,
        [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado,
        [FromQuery] ResultadoAtencion? resultado,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ObtenerAtencionesAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            resultado,
            pagina,
            tamanoPagina,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reportes/reservas/exportar")]
    public async Task<IActionResult> ExportarReservas(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? profesionalId,
        [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado,
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ExportarReservasCsvAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            clienteId,
            ct);

        if (!result.EsExitoso || result.Valor is null)
            return OperationProblem(result.Error, result.TipoError);

        var nombreArchivo =
            $"reporte-reservas-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.csv";

        return File(
            result.Valor,
            "text/csv; charset=utf-8",
            nombreArchivo);
    }

    [HttpGet("api/reportes/ingresos/exportar")]
    public async Task<IActionResult> ExportarIngresos(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? metodoPagoId,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ExportarIngresosCsvAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            metodoPagoId,
            ct);

        if (!result.EsExitoso || result.Valor is null)
            return OperationProblem(result.Error, result.TipoError);

        var nombreArchivo =
            $"reporte-ingresos-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.csv";

        return File(
            result.Valor,
            "text/csv; charset=utf-8",
            nombreArchivo);
    }

    [HttpGet("api/reportes/atenciones/exportar")]
    public async Task<IActionResult> ExportarAtenciones(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? profesionalId,
        [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado,
        [FromQuery] ResultadoAtencion? resultado,
        [FromQuery] Guid? organizacionId = null,
        CancellationToken ct = default)
    {
        var organizacion = ResolverOrganizacion(organizacionId);

        if (!organizacion.HasValue)
            return Forbid();

        var result = await reporteService.ExportarAtencionesCsvAsync(
            organizacion.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            profesionalId,
            servicioId,
            estado,
            resultado,
            ct);

        if (!result.EsExitoso || result.Valor is null)
            return OperationProblem(result.Error, result.TipoError);

        var nombreArchivo =
            $"reporte-atenciones-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.csv";

        return File(
            result.Valor,
            "text/csv; charset=utf-8",
            nombreArchivo);
    }

    private Guid? ResolverOrganizacion(Guid? organizacionId)
    {
        if (currentUser.IsInRole(RoleNames.Superadministrador))
        {
            return organizacionId.HasValue &&
                   organizacionId.Value != Guid.Empty
                ? organizacionId.Value
                : null;
        }

        return currentUser.OrganizacionId;
    }
}