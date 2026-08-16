using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Pagos;
using ReservaTuCitaYa.Application.Interfaces;

namespace ReservaTuCitaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ReembolsosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public ReembolsosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet("{reservaId}")]
        public async Task<ActionResult<IEnumerable<ReembolsoDto>>> ListarReembolsos(Guid reservaId)
        {
            var reembolsos = await _pagoService.ListarReembolsosAsync(reservaId);
            return Ok(reembolsos);
        }

        [HttpPost("{reservaId}")]
        public async Task<ActionResult<ReembolsoDto>> RegistrarReembolso(Guid reservaId, [FromBody] RegistrarReembolsoRequest request)
        {
            var reembolso = await _pagoService.RegistrarReembolsoAsync(reservaId, request);
            return Ok(reembolso);
        }
    }
}
