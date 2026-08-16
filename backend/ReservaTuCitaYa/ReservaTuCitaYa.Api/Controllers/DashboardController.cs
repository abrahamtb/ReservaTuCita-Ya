using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Dashboard;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class DashboardController(
    IDashboardService dashboardService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardResumenDto>> Obtener(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] Guid? sedeId,
        [FromQuery] Guid? organizacionId,
        CancellationToken ct)
    {
        Guid organizacionAutorizada;

        // Superadministrador no está ligado a una organización específica.
        if (currentUser.IsInRole(RoleNames.Superadministrador))
        {
            if (!organizacionId.HasValue ||
                organizacionId.Value == Guid.Empty)
            {
                return BadRequest(new
                {
                    detail =
                        "El Superadministrador debe indicar una organización."
                });
            }

            organizacionAutorizada = organizacionId.Value;
        }
        else
        {
            if (!currentUser.OrganizacionId.HasValue)
            {
                return Forbid();
            }

            // Un administrador normal SIEMPRE usa
            // su organización del contexto autenticado.
            organizacionAutorizada =
                currentUser.OrganizacionId.Value;
        }

        var result = await dashboardService.ObtenerAsync(
            organizacionAutorizada,
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