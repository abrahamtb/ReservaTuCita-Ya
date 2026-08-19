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
    public sealed class ReembolsosController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        public ReembolsosController(IPagoService pagoService) => _pagoService = pagoService;

        [HttpGet("{reservaId}")]
        [Authorize(Policy = Permissions.Pagos.Ver)]
        public async Task<ActionResult<IEnumerable<ReembolsoDto>>> ListarReembolsos(Guid reservaId)
            => Ok(await _pagoService.ListarReembolsosAsync(reservaId));

        [HttpPost("{reservaId}")]
        [Authorize(Policy = Permissions.Pagos.Reembolsar)]
        public async Task<ActionResult<ReembolsoDto>> RegistrarReembolso(Guid reservaId, [FromBody] RegistrarReembolsoRequest request)
            => Ok(await _pagoService.RegistrarReembolsoAsync(reservaId, request));
    }
}
