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
public sealed class PagosController(IPagoService pagoService, ApplicationDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("resumen/{reservaId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Ver)]
    public async Task<ActionResult<ResumenPagoReservaDto>> ObtenerResumen(Guid reservaId)
    {
        if (!await PuedeAccederReservaAsync(reservaId)) return NotFound();
        return Ok(await pagoService.ObtenerResumenAsync(reservaId));
    }

    [HttpGet("{reservaId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Ver)]
    public async Task<ActionResult<IEnumerable<PagoDto>>> ListarPagos(Guid reservaId)
    {
        if (!await PuedeAccederReservaAsync(reservaId)) return NotFound();
        return Ok(await pagoService.ListarPagosAsync(reservaId));
    }

    [HttpPost("{reservaId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Registrar)]
    public async Task<ActionResult<PagoDto>> RegistrarPago(Guid reservaId, [FromBody] CrearPagoRequest request)
    {
        if (!await PuedeAccederReservaAsync(reservaId)) return NotFound();
        return Ok(await pagoService.RegistrarPagoAsync(reservaId, request));
    }

    [HttpPut("anular/{pagoId:guid}")]
    [Authorize(Policy = Permissions.Pagos.Anular)]
    public async Task<ActionResult<PagoDto>> AnularPago(Guid pagoId, [FromBody] AnularPagoRequest request)
    {
        var reservaId = await db.Pagos.AsNoTracking().Where(pago => pago.Id == pagoId).Select(pago => (Guid?)pago.ReservaId).FirstOrDefaultAsync();
        if (!reservaId.HasValue || !await PuedeAccederReservaAsync(reservaId.Value)) return NotFound();
        return Ok(await pagoService.AnularPagoAsync(pagoId, request));
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
