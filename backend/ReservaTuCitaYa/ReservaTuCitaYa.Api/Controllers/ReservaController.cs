using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.Abstractions;
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
public sealed class ReservasController(IReservaService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas")]
    public async Task<ActionResult<ReservaCreadaDto>> Crear(
        Guid organizacionId, CrearReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
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
        if (result.EsExitoso && result.Valor is not null && !EsOrganizacionAutorizada(result.Valor.OrganizacionId))
            return NotFound();
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reservas/codigo/{codigo}")]
    public async Task<ActionResult<ReservaDetalleDto>> ObtenerPorCodigo(string codigo, CancellationToken ct)
    {
        var result = await service.ObtenerPorCodigoAsync(codigo, ct);
        if (result.EsExitoso && result.Valor is not null && !EsOrganizacionAutorizada(result.Valor.OrganizacionId))
            return NotFound();
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
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        var result = await service.ListarAsync(new ReservaFiltroDto(
            organizacionId, sedeId, clienteId, profesionalId, servicioId,
            estado, desde, hasta, pagina, tamanoPagina), ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/organizaciones/{organizacionId:guid}/reservas/{id:guid}/reprogramacion")]
    public async Task<ActionResult<ReprogramarReservaRespuesta>> Reprogramar(
        Guid organizacionId, Guid id, ReprogramarReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        var solicitud = new ReprogramarReservaSolicitud
        {
            ReservaId = id,
            FechaNueva = request.FechaNueva,
            HoraInicioNueva = request.HoraInicioNueva,
            ProfesionalId = request.ProfesionalId,
            RecursoId = request.RecursoId,
            Motivo = request.Motivo,
            Observacion = request.Observacion
        };
        var result = await service.ReprogramarAsync(
            organizacionId, solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{id:guid}/cancelacion")]
    public async Task<ActionResult<CancelarReservaRespuesta>> Cancelar(
        Guid organizacionId, Guid id, CancelarReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        var solicitud = new CancelarReservaSolicitud
        {
            ReservaId = id,
            Motivo = request.Motivo,
            Comentario = request.Comentario,
            Confirmacion = request.Confirmacion
        };
        var result = await service.CancelarAsync(
            organizacionId, solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
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
            ReservaService.EstadoNoPermitido => ("reservation-state-conflict", "Estado de reserva no permitido"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }

    private bool EsOrganizacionAutorizada(Guid organizacionId) =>
        currentUser.IsInRole(RoleNames.Superadministrador) || currentUser.OrganizacionId == organizacionId;
}
