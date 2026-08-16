using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Dashboard;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
public sealed class DashboardController(
    IDashboardService dashboardService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardResumenDto>> Obtener(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        CancellationToken ct)
    {
        if (!currentUser.OrganizacionId.HasValue)
        {
            return Forbid();
        }

        var result = await dashboardService.ObtenerAsync(
            currentUser.OrganizacionId.Value,
            fechaDesde,
            fechaHasta,
            sedeId,
            ct);

        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(
                result.Error,
                result.TipoError);
    }
}