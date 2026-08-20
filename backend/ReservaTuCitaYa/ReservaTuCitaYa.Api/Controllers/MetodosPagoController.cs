using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaTuCitaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MetodosPagoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MetodosPagoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetodoPago>>> ListarMetodos()
        {
            var metodos = await _context.MetodosPago
                .Where(m => m.EstaActivo)
                .ToListAsync();

            return Ok(metodos);
        }
    }
}
