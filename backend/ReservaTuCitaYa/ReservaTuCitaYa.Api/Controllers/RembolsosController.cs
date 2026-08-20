using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Pagos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ReembolsosController(IPagoService pagoService, ApplicationDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("{reservaId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Ver)]
    public async Task<ActionResult<IEnumerable<ReembolsoDto>>> ListarReembolsos(Guid reservaId)
    {
        if (!await PuedeAccederReservaAsync(reservaId)) return NotFound();
        return Ok(await pagoService.ListarReembolsosAsync(reservaId));
    }

    [HttpPost("{reservaId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Reembolsar)]
    public async Task<ActionResult<ReembolsoDto>> RegistrarReembolso(Guid reservaId, [FromBody] RegistrarReembolsoRequest request)
    {
        if (!await PuedeAccederReservaAsync(reservaId)) return NotFound();
        return Ok(await pagoService.RegistrarReembolsoAsync(reservaId, request));
    }

    private async Task<bool> PuedeAccederReservaAsync(Guid reservaId)
    {
        var reserva = await db.Reservas.AsNoTracking().Where(item => item.Id == reservaId)
            .Select(item => new { item.OrganizacionId, item.ClienteId }).FirstOrDefaultAsync();
        if (reserva is null) return false;
        if (currentUser.IsInRole(RoleNames.Superadministrador)) return true;
        if (currentUser.OrganizacionId != reserva.OrganizacionId) return false;
        return !currentUser.IsInRole(RoleNames.Cliente) || currentUser.ClienteId == reserva.ClienteId;
    }
}
