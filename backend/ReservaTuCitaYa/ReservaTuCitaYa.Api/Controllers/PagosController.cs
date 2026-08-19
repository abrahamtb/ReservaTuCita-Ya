using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Pagos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        public PagosController(IPagoService pagoService) => _pagoService = pagoService;

        [HttpGet("resumen/{reservaId}")]
        [Authorize(Policy = Permissions.Pagos.Ver)]
        public async Task<ActionResult<ResumenPagoReservaDto>> ObtenerResumen(Guid reservaId)
            => Ok(await _pagoService.ObtenerResumenAsync(reservaId));

        [HttpGet("{reservaId}")]
        [Authorize(Policy = Permissions.Pagos.Ver)]
        public async Task<ActionResult<IEnumerable<PagoDto>>> ListarPagos(Guid reservaId)
            => Ok(await _pagoService.ListarPagosAsync(reservaId));

        [HttpPost("{reservaId}")]
        [Authorize(Policy = Permissions.Pagos.Registrar)]
        public async Task<ActionResult<PagoDto>> RegistrarPago(Guid reservaId, [FromBody] CrearPagoRequest request)
            => Ok(await _pagoService.RegistrarPagoAsync(reservaId, request));

        [HttpPut("anular/{pagoId}")]
        [Authorize(Policy = Permissions.Pagos.Anular)]
        public async Task<ActionResult<PagoDto>> AnularPago(Guid pagoId, [FromBody] AnularPagoRequest request)
            => Ok(await _pagoService.AnularPagoAsync(pagoId, request));
    }
}
