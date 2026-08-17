using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using ReservaTuCitaYa.Application.Interfaces;

namespace ReservaTuCitaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CalificacionesController : ControllerBase
    {
        private readonly ICalificacionService _service;

        public CalificacionesController(ICalificacionService service)
        {
            _service = service;
        }

        [HttpPost("reservas/{reservaId}")]
        public async Task<IActionResult> Crear(Guid reservaId, [FromBody] CrearCalificacionRequest request)
        {
            var calificacion = await _service.CrearCalificacionAsync(reservaId, request);
            return CreatedAtAction(nameof(ObtenerPorReserva), new { reservaId }, calificacion);
        }

        [HttpGet("reservas/{reservaId}")]
        public async Task<IActionResult> ObtenerPorReserva(Guid reservaId)
        {
            var calificacion = await _service.ObtenerPorReservaAsync(reservaId);
            if (calificacion is null)
                return NotFound();

            return Ok(calificacion);
        }

        [HttpGet("profesionales/{profesionalId}/resumen")]
        public async Task<IActionResult> ObtenerResumenProfesional(Guid profesionalId)
        {
            var resumen = await _service.ObtenerResumenProfesionalAsync(profesionalId);
            return Ok(resumen);
        }

        [HttpGet("profesionales/{profesionalId}")]
        public async Task<IActionResult> ListarPorProfesional(Guid profesionalId, int pagina = 1, int tamanoPagina = 10, int? puntuacion = null)
        {
            var calificaciones = await _service.ListarPorProfesionalAsync(profesionalId, pagina, tamanoPagina, puntuacion);
            return Ok(calificaciones);
        }
    }
}
