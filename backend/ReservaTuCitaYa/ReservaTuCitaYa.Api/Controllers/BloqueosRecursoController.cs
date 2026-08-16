using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Recursos;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Infrastructure.Identity;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class BloqueosRecursoController(IBloqueoRecursoService service) : ApiControllerBase
{
    [HttpGet("api/recursos/{recursoId:guid}/bloqueos")]
    public async Task<ActionResult<IReadOnlyList<BloqueoRecursoDto>>> Listar(
        Guid recursoId, CancellationToken cancellationToken)
    {
        var result = await service.ListarPorRecursoAsync(recursoId, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : BloqueoProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/recursos/{recursoId:guid}/bloqueos")]
    public async Task<ActionResult<Guid>> Crear(
        Guid recursoId, CrearBloqueoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(new CrearBloqueoSolicitud
        {
            RecursoId = recursoId,
            FechaHoraInicio = request.FechaHoraInicio,
            FechaHoraFin = request.FechaHoraFin,
            TipoBloqueo = request.TipoBloqueo,
            Motivo = request.Motivo,
            Observaciones = request.Observaciones
        }, cancellationToken);
        return result.EsExitoso
            ? CreatedAtAction(nameof(Listar), new { recursoId }, result.Valor)
            : BloqueoProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/bloqueos/{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id, ActualizarBloqueoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(new ActualizarBloqueoSolicitud
        {
            Id = id,
            FechaHoraInicio = request.FechaHoraInicio,
            FechaHoraFin = request.FechaHoraFin,
            TipoBloqueo = request.TipoBloqueo,
            Motivo = request.Motivo,
            Observaciones = request.Observaciones
        }, cancellationToken);
        return result.EsExitoso ? NoContent() : BloqueoProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/bloqueos/{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : BloqueoProblem(result.Error, result.TipoError);
    }

    private ObjectResult BloqueoProblem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            BloqueoRecursoService.BloqueoSolapado => ("blocking-period-overlap", "Bloqueo solapado"),
            BloqueoRecursoService.RecursoInvalido => ("resource-invalid", "Recurso inválido"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}