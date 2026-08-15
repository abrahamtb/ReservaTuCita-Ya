using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Atenciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Infrastructure.Identity;
using System.Security.Claims;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
public sealed class AtencionesController(
    IAtencionService service,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{reservaId:guid}/atencion/presencia")]
    [Authorize(Policy = Permissions.Atenciones.MarcarPresente)]
    public async Task<ActionResult<MarcarPresenteRespuesta>> MarcarPresente(
        Guid organizacionId,
        Guid reservaId,
        CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await service.MarcarPresenteAsync(
            organizacionId,
            reservaId,
            usuarioId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{reservaId:guid}/atencion/iniciar")]
    [Authorize(Policy = Permissions.Atenciones.Iniciar)]
    public async Task<ActionResult<IniciarAtencionRespuesta>> Iniciar(
    Guid organizacionId,
    Guid reservaId,
    CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await service.IniciarAtencionAsync(
            organizacionId,
            reservaId,
            usuarioId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{reservaId:guid}/atencion/finalizar")]
    [Authorize(Policy = Permissions.Atenciones.Finalizar)]
    public async Task<ActionResult<FinalizarAtencionRespuesta>> Finalizar(
    Guid organizacionId,
    Guid reservaId,
    FinalizarAtencionSolicitud request,
    CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var usuarioId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await service.FinalizarAtencionAsync(
            organizacionId,
            reservaId,
            request,
            usuarioId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/reservas/{reservaId:guid}/atencion/no-asistio")]
    [Authorize(Policy = Permissions.Atenciones.Finalizar)]
    public async Task<ActionResult<MarcarNoAsistioRespuesta>> MarcarNoAsistio(
    Guid organizacionId,
    Guid reservaId,
    CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var usuarioId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await service.MarcarNoAsistioAsync(
            organizacionId,
            reservaId,
            usuarioId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/reservas/{reservaId:guid}/atencion")]
    [Authorize(Policy = Permissions.Atenciones.Ver)]
    public async Task<ActionResult<AtencionDetalleDto>> ObtenerDetalle(
    Guid organizacionId,
    Guid reservaId,
    CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var result = await service.ObtenerDetalleAsync(
            organizacionId,
            reservaId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(
                result.Error,
                result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/profesionales/{profesionalId:guid}/agenda")]
    [Authorize(Policy = Permissions.Atenciones.Ver)]
    public async Task<ActionResult<AgendaProfesionalDto>> ObtenerAgendaProfesional(
    Guid organizacionId,
    Guid profesionalId,
    [FromQuery] DateOnly fecha,
    CancellationToken ct)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return NotFound();
        }

        var result = await service.ObtenerAgendaProfesionalAsync(
            organizacionId,
            profesionalId,
            fecha,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(
                result.Error,
                result.TipoError);
    }

    private bool EsOrganizacionAutorizada(Guid organizacionId) =>
        currentUser.IsInRole(RoleNames.Superadministrador) ||
        currentUser.OrganizacionId == organizacionId;
}
