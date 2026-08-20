using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Disponibilidad;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Infrastructure.Identity;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.Reservas.Crear)]
public sealed class DisponibilidadController(IDisponibilidadService service) : ApiControllerBase
{
    [HttpGet("api/disponibilidad")]
    public async Task<ActionResult<DisponibilidadRespuestaDto>> Consultar(
        [FromQuery] Guid sedeId, [FromQuery] Guid servicioId,
        [FromQuery] DateOnly fechaDesde, [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? profesionalId, [FromQuery] Guid? recursoId,
        CancellationToken ct)
    {
        var result = await service.ConsultarAsync(
            new ConsultarDisponibilidadSolicitud(sedeId, servicioId, fechaDesde, fechaHasta, profesionalId, recursoId), ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : DisponibilidadProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/disponibilidad/profesionales")]
    public async Task<ActionResult<IReadOnlyList<ProfesionalDisponibleDto>>> Profesionales(
        [FromQuery] Guid sedeId, [FromQuery] Guid servicioId, [FromQuery] DateOnly? fecha, CancellationToken ct)
    {
        var result = await service.ListarProfesionalesCompatiblesAsync(sedeId, servicioId, fecha, ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : DisponibilidadProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/disponibilidad/recursos")]
    public async Task<ActionResult<IReadOnlyList<RecursoDisponibleDto>>> Recursos(
        [FromQuery] Guid sedeId, [FromQuery] Guid servicioId, [FromQuery] DateOnly? fecha, CancellationToken ct)
    {
        var result = await service.ListarRecursosCompatiblesAsync(sedeId, servicioId, fecha, ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : DisponibilidadProblem(result.Error, result.TipoError);
    }

    private ObjectResult DisponibilidadProblem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            DisponibilidadService.SedeInvalida => ("availability-site-invalid", "Sede inválida"),
            DisponibilidadService.ServicioInvalido => ("availability-service-invalid", "Servicio inválido"),
            DisponibilidadService.ServicioNoOfrecidoEnSede => ("availability-service-site-mismatch", "Servicio no ofrecido en la sede"),
            DisponibilidadService.RangoInvalido => ("availability-range-invalid", "Rango de fechas inválido"),
            DisponibilidadService.RangoExcesivo => ("availability-range-too-large", "Rango de fechas excesivo"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}
