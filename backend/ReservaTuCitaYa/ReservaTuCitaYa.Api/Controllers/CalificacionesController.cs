using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/calificaciones")]
public sealed class CalificacionesController : ControllerBase
{
    private readonly ICalificacionService _service;

    public CalificacionesController(ICalificacionService service)
    {
        _service = service;
    }

    [HttpPost("reservas/{reservaId:guid}")]
    [Authorize(Policy = Permissions.Calificaciones.Crear)]
    public async Task<IActionResult> Crear(Guid reservaId, [FromBody] CrearCalificacionRequest request)
    {
        var calificacion = await _service.CrearCalificacionAsync(reservaId, request);
        return CreatedAtAction(nameof(ObtenerPorReserva), new { reservaId }, calificacion);
    }

    [HttpGet("reservas/{reservaId:guid}")]
    [Authorize(Policy = Permissions.Calificaciones.Ver)]
    public async Task<IActionResult> ObtenerPorReserva(Guid reservaId)
    {
        var calificacion = await _service.ObtenerPorReservaAsync(reservaId);
        return calificacion is null ? NotFound() : Ok(calificacion);
    }

    [HttpGet("profesionales/{profesionalId:guid}/resumen")]
    [Authorize(Policy = Permissions.Calificaciones.Ver)]
    public async Task<IActionResult> ObtenerResumenProfesional(Guid profesionalId)
    {
        var resumen = await _service.ObtenerResumenProfesionalAsync(profesionalId);
        return Ok(resumen);
    }

    [HttpGet("profesionales/{profesionalId:guid}")]
    [Authorize(Policy = Permissions.Calificaciones.Ver)]
    public async Task<IActionResult> ListarPorProfesional(
        Guid profesionalId,
        int pagina = 1,
        int tamanoPagina = 10,
        int? puntuacion = null)
    {
        var calificaciones = await _service.ListarPorProfesionalAsync(
            profesionalId,
            pagina,
            tamanoPagina,
            puntuacion);

        return Ok(calificaciones);
    }
}
