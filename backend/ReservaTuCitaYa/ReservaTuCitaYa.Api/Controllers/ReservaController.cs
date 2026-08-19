using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;
using System.Security.Claims;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ReservasController(IReservaService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas")]
    [Authorize(Policy = Permissions.Reservas.Crear)]
    public async Task<ActionResult<ReservaCreadaDto>> Crear(Guid organizacionId, CrearReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        if (EsCliente && !currentUser.ClienteId.HasValue) return Forbid();

        var solicitud = EsCliente
            ? new CrearReservaSolicitud
            {
                ClienteId = currentUser.ClienteId!.Value,
                ServicioId = request.ServicioId,
                SedeId = request.SedeId,
                ProfesionalId = request.ProfesionalId,
                RecursoId = request.RecursoId,
                Fecha = request.Fecha,
                HoraInicio = request.HoraInicio,
                CantidadParticipantes = request.CantidadParticipantes,
                Participantes = request.Participantes,
                Observaciones = request.Observaciones
            }
            : request;

        var result = await service.CrearAsync(
            organizacionId, solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
        return result.EsExitoso && result.Valor is not null
            ? CreatedAtAction(nameof(Obtener), new { id = result.Valor.Id }, result.Valor)
            : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reservas/{id:guid}")]
    [Authorize(Policy = Permissions.Reservas.Ver)]
    public async Task<ActionResult<ReservaDetalleDto>> Obtener(Guid id, CancellationToken ct)
    {
        var result = await service.ObtenerPorIdAsync(id, ct);
        if (result.EsExitoso && result.Valor is not null && !PuedeVer(result.Valor)) return NotFound();
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/reservas/codigo/{codigo}")]
    [Authorize(Policy = Permissions.Reservas.Ver)]
    public async Task<ActionResult<ReservaDetalleDto>> ObtenerPorCodigo(string codigo, CancellationToken ct)
    {
        var result = await service.ObtenerPorCodigoAsync(codigo, ct);
        if (result.EsExitoso && result.Valor is not null && !PuedeVer(result.Valor)) return NotFound();
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/reservas")]
    [Authorize(Policy = Permissions.Reservas.Ver)]
    public async Task<ActionResult<PaginaResultado<ReservaListaDto>>> Listar(
        Guid organizacionId, [FromQuery] Guid? sedeId, [FromQuery] Guid? clienteId,
        [FromQuery] Guid? profesionalId, [FromQuery] Guid? servicioId,
        [FromQuery] EstadoReserva? estado, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken ct = default)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        if (EsCliente)
        {
            if (!currentUser.ClienteId.HasValue) return Forbid();
            clienteId = currentUser.ClienteId.Value;
        }
        if (currentUser.IsInRole(RoleNames.Profesional))
        {
            if (!currentUser.EmpleadoId.HasValue) return Forbid();
            profesionalId = currentUser.EmpleadoId.Value;
        }

        var result = await service.ListarAsync(new ReservaFiltroDto(
            organizacionId, sedeId, clienteId, profesionalId, servicioId, estado, desde, hasta, pagina, tamanoPagina), ct);
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/organizaciones/{organizacionId:guid}/reservas/{id:guid}/reprogramacion")]
    [Authorize(Policy = Permissions.Reservas.Reprogramar)]
    public async Task<ActionResult<ReprogramarReservaRespuesta>> Reprogramar(
        Guid organizacionId, Guid id, ReprogramarReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId) || !await PuedeModificarAsync(id, ct)) return NotFound();
        var solicitud = new ReprogramarReservaSolicitud
        {
            ReservaId = id, FechaNueva = request.FechaNueva, HoraInicioNueva = request.HoraInicioNueva,
            ProfesionalId = request.ProfesionalId, RecursoId = request.RecursoId,
            Motivo = request.Motivo, Observacion = request.Observacion
        };
        var result = await service.ReprogramarAsync(
            organizacionId, solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{id:guid}/cancelacion")]
    [Authorize(Policy = Permissions.Reservas.Cancelar)]
    public async Task<ActionResult<CancelarReservaRespuesta>> Cancelar(
        Guid organizacionId, Guid id, CancelarReservaSolicitud request, CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId) || !await PuedeModificarAsync(id, ct)) return NotFound();
        var solicitud = new CancelarReservaSolicitud
        {
            ReservaId = id, Motivo = request.Motivo, Comentario = request.Comentario, Confirmacion = request.Confirmacion
        };
        var result = await service.CancelarAsync(
            organizacionId, solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ReservaProblem(result.Error, result.TipoError);
    }

    private bool EsCliente => currentUser.IsInRole(RoleNames.Cliente);

    private bool PuedeVer(ReservaDetalleDto reserva)
    {
        if (!EsOrganizacionAutorizada(reserva.OrganizacionId)) return false;
        if (EsCliente) return currentUser.ClienteId.HasValue && reserva.Cliente.Id == currentUser.ClienteId.Value;
        if (currentUser.IsInRole(RoleNames.Profesional))
            return currentUser.EmpleadoId.HasValue && reserva.Profesional?.Id == currentUser.EmpleadoId.Value;
        return true;
    }

    private async Task<bool> PuedeModificarAsync(Guid id, CancellationToken ct)
    {
        var reserva = await service.ObtenerPorIdAsync(id, ct);
        return reserva.EsExitoso && reserva.Valor is not null && PuedeVer(reserva.Valor);
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
