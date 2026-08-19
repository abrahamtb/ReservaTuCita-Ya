using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Pagos;
using ReservaTuCitaYa.Application.Interfaces;

namespace ReservaTuCitaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet("resumen/{reservaId}")]
        public async Task<ActionResult<ResumenPagoReservaDto>> ObtenerResumen(Guid reservaId)
        {
            var resumen = await _pagoService.ObtenerResumenAsync(reservaId);
            return Ok(resumen);
        }

        [HttpGet("{reservaId}")]
        public async Task<ActionResult<IEnumerable<PagoDto>>> ListarPagos(Guid reservaId)
        {
            var pagos = await _pagoService.ListarPagosAsync(reservaId);
            return Ok(pagos);
        }

        [HttpPost("{reservaId}")]
        public async Task<ActionResult<PagoDto>> RegistrarPago(Guid reservaId, [FromBody] CrearPagoRequest request)
        {
            var pago = await _pagoService.RegistrarPagoAsync(reservaId, request);
            return Ok(pago);
        }

        [HttpPut("anular/{pagoId}")]
        public async Task<ActionResult<PagoDto>> AnularPago(Guid pagoId, [FromBody] AnularPagoRequest request)
        {
            var pago = await _pagoService.AnularPagoAsync(pagoId, request);
            return Ok(pago);
        }
    }

}
