using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;
using System.Security.Claims;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class ReservasController(IReservaService service) : ApiControllerBase
{
    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas")]
    public async Task<ActionResult<ReservaCreadaDto>> Crear(
        Guid organizacionId, CrearReservaSolicitud request, CancellationToken ct)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await service.CrearAsync(organizacionId, request, usuarioId, ct);
        return result.EsExitoso && result.Valor is not null
            ? CreatedAtAction(nameof(Obtener), new { id = result.Valor.Id }, result.Valor)
            : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reservas/{id:guid}")]
    public async Task<ActionResult<ReservaDetalleDto>> Obtener(Guid id, CancellationToken ct)
    {
        var result = await service.ObtenerPorIdAsync(id, ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reservas/codigo/{codigo}")]
    public async Task<ActionResult<ReservaDetalleDto>> ObtenerPorCodigo(string codigo, CancellationToken ct)
    {
        var result = await service.ObtenerPorCodigoAsync(codigo, ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/reservas")]
    public async Task<ActionResult<PaginaResultado<ReservaListaDto>>> Listar(
        Guid organizacionId, [FromQuery] Guid? sedeId, [FromQuery] Guid? clienteId,
        [FromQuery] Guid? profesionalId, [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken ct = default)
    {
        var result = await service.ListarAsync(new ReservaFiltroDto(
            organizacionId, sedeId, clienteId, profesionalId, servicioId,
            estado, desde, hasta, pagina, tamanoPagina), ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    private ObjectResult ReservaProblem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            ReservaService.HorarioOcupado => ("reservation-slot-conflict", "Horario no disponible"),
            ReservaService.CapacidadExcedida => ("reservation-capacity-exceeded", "Capacidad excedida"),
            ReservaService.SinCombinacionDisponible => ("reservation-no-combination", "Sin combinación disponible"),
            ReservaService.ServicioNoOfrecidoEnSede => ("reservation-service-site-mismatch", "Servicio no ofrecido en la sede"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}