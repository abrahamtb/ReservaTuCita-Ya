using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class HorariosRecursoController(IHorarioRecursoService service) : ApiControllerBase
{
    [HttpGet("api/recursos/{recursoId:guid}/horarios")]
    public async Task<ActionResult<HorarioSemanalDto>> Obtener(Guid recursoId, CancellationToken ct)
    {
        var r = await service.ListarAsync(recursoId, ct);
        return r.EsExitoso && r.Valor is not null ? Ok(r.Valor) : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPut("api/recursos/{recursoId:guid}/horarios")]
    public async Task<IActionResult> Actualizar(Guid recursoId, ActualizarHorarioSemanalSolicitud request, CancellationToken ct)
    {
        var r = await service.ActualizarAsync(recursoId, request, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }

    [HttpGet("api/recursos/{recursoId:guid}/excepciones-horario")]
    public async Task<ActionResult<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepciones(
        Guid recursoId, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoExcepcionHorario? tipoExcepcion, [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10, CancellationToken ct = default)
    {
        var r = await service.ListarExcepcionesAsync(
            new ExcepcionHorarioFiltroDto(recursoId, desde, hasta, tipoExcepcion, pagina, tamanoPagina), ct);
        return r.EsExitoso && r.Valor is not null ? Ok(r.Valor) : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPost("api/recursos/{recursoId:guid}/excepciones-horario")]
    public async Task<ActionResult<Guid>> CrearExcepcion(Guid recursoId, CrearExcepcionRecursoSolicitud request, CancellationToken ct)
    {
        var solicitud = new CrearExcepcionRecursoSolicitud
        {
            RecursoId = recursoId, Fecha = request.Fecha, TipoExcepcion = request.TipoExcepcion,
            HoraInicio = request.HoraInicio, HoraFin = request.HoraFin,
            Motivo = request.Motivo, Observaciones = request.Observaciones
        };
        var r = await service.CrearExcepcionAsync(solicitud, ct);
        return r.EsExitoso ? Created($"api/excepciones-horario-recurso/{r.Valor}", r.Valor)
            : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPut("api/excepciones-horario-recurso/{id:guid}")]
    public async Task<IActionResult> ActualizarExcepcion(Guid id, ActualizarExcepcionRecursoSolicitud request, CancellationToken ct)
    {
        var solicitud = new ActualizarExcepcionRecursoSolicitud
        {
            Id = id, Fecha = request.Fecha, TipoExcepcion = request.TipoExcepcion,
            HoraInicio = request.HoraInicio, HoraFin = request.HoraFin,
            Motivo = request.Motivo, Observaciones = request.Observaciones
        };
        var r = await service.ActualizarExcepcionAsync(solicitud, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }

    [HttpDelete("api/excepciones-horario-recurso/{id:guid}")]
    public async Task<IActionResult> EliminarExcepcion(Guid id, CancellationToken ct)
    {
        var r = await service.EliminarExcepcionAsync(id, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }
}
