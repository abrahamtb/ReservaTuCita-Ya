// Api/Controllers/HorariosSedeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class HorariosSedeController(IHorarioSedeService service) : ApiControllerBase
{
    [HttpGet("api/sedes/{sedeId:guid}/horarios")]
    public async Task<ActionResult<HorarioSemanalDto>> Obtener(Guid sedeId, CancellationToken ct)
    {
        var result = await service.ListarAsync(sedeId, ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : Problem(result.Error, result.TipoError);
    }

    [HttpPut("api/sedes/{sedeId:guid}/horarios")]
    public async Task<IActionResult> Actualizar(
        Guid sedeId, ActualizarHorarioSemanalSolicitud request, CancellationToken ct)
    {
        var result = await service.ActualizarAsync(sedeId, request, ct);
        return result.EsExitoso ? NoContent() : Problem(result.Error, result.TipoError);
    }

    [HttpGet("api/sedes/{sedeId:guid}/excepciones-horario")]
    public async Task<ActionResult<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepciones(
        Guid sedeId, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoExcepcionHorario? tipoExcepcion,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken ct = default)
    {
        var result = await service.ListarExcepcionesAsync(
            new ExcepcionHorarioFiltroDto(sedeId, desde, hasta, tipoExcepcion, pagina, tamanoPagina), ct);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : Problem(result.Error, result.TipoError);
    }

    [HttpPost("api/sedes/{sedeId:guid}/excepciones-horario")]
    public async Task<ActionResult<Guid>> CrearExcepcion(
        Guid sedeId, CrearExcepcionSedeSolicitud request, CancellationToken ct)
    {
        var solicitud = new CrearExcepcionSedeSolicitud
        {
            SedeId = sedeId,
            Fecha = request.Fecha,
            TipoExcepcion = request.TipoExcepcion,
            HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin,
            Motivo = request.Motivo,
            Observaciones = request.Observaciones
        };
        var result = await service.CrearExcepcionAsync(solicitud, ct);
        return result.EsExitoso
            ? Created($"api/excepciones-horario-sede/{result.Valor}", result.Valor)
            : Problem(result.Error, result.TipoError);
    }

    [HttpPut("api/excepciones-horario-sede/{id:guid}")]
    public async Task<IActionResult> ActualizarExcepcion(
        Guid id, ActualizarExcepcionSedeSolicitud request, CancellationToken ct)
    {
        var solicitud = new ActualizarExcepcionSedeSolicitud
        {
            Id = id,
            Fecha = request.Fecha,
            TipoExcepcion = request.TipoExcepcion,
            HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin,
            Motivo = request.Motivo,
            Observaciones = request.Observaciones
        };
        var result = await service.ActualizarExcepcionAsync(solicitud, ct);
        return result.EsExitoso ? NoContent() : Problem(result.Error, result.TipoError);
    }

    [HttpDelete("api/excepciones-horario-sede/{id:guid}")]
    public async Task<IActionResult> EliminarExcepcion(Guid id, CancellationToken ct)
    {
        var result = await service.EliminarExcepcionAsync(id, ct);
        return result.EsExitoso ? NoContent() : Problem(result.Error, result.TipoError);
    }

    private ObjectResult Problem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            HorarioSedeService.IntervalosSuperpuestos => ("schedule-overlap", "Horario superpuesto"),
            HorarioSedeService.ExcepcionIncompatible => ("exception-incompatible", "Excepción incompatible"),
            HorarioSedeService.SedeInvalida => ("site-invalid", "Sede inválida"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}
