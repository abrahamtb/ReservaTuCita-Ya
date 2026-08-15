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
public sealed class HorariosProfesionalController(IHorarioProfesionalService service) : ApiControllerBase
{
    [HttpGet("api/profesionales/{profesionalId:guid}/horarios")]
    public async Task<ActionResult<HorarioSemanalDto>> Obtener(Guid profesionalId, [FromQuery] Guid? sedeId, CancellationToken ct)
    {
        var r = await service.ListarAsync(profesionalId, sedeId, ct);
        return r.EsExitoso && r.Valor is not null ? Ok(r.Valor) : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPut("api/profesionales/{profesionalId:guid}/sedes/{sedeId:guid}/horarios")]
    public async Task<IActionResult> Actualizar(Guid profesionalId, Guid sedeId, ActualizarHorarioSemanalSolicitud request, CancellationToken ct)
    {
        var r = await service.ActualizarAsync(profesionalId, sedeId, request, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }

    [HttpGet("api/profesionales/{profesionalId:guid}/excepciones-horario")]
    public async Task<ActionResult<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepciones(
        Guid profesionalId, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoExcepcionHorario? tipoExcepcion, [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10, CancellationToken ct = default)
    {
        var r = await service.ListarExcepcionesAsync(
            new ExcepcionHorarioFiltroDto(profesionalId, desde, hasta, tipoExcepcion, pagina, tamanoPagina), ct);
        return r.EsExitoso && r.Valor is not null ? Ok(r.Valor) : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPost("api/profesionales/{profesionalId:guid}/sedes/{sedeId:guid}/excepciones-horario")]
    public async Task<ActionResult<Guid>> CrearExcepcion(
        Guid profesionalId, Guid sedeId, CrearExcepcionProfesionalSolicitud request, CancellationToken ct)
    {
        var solicitud = new CrearExcepcionProfesionalSolicitud
        {
            EmpleadoId = profesionalId, SedeId = sedeId, Fecha = request.Fecha,
            TipoExcepcion = request.TipoExcepcion, HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin, Motivo = request.Motivo, Observaciones = request.Observaciones
        };
        var r = await service.CrearExcepcionAsync(solicitud, ct);
        return r.EsExitoso ? Created($"api/excepciones-horario-profesional/{r.Valor}", r.Valor)
            : OperationProblem(r.Error, r.TipoError);
    }

    [HttpPut("api/excepciones-horario-profesional/{id:guid}")]
    public async Task<IActionResult> ActualizarExcepcion(Guid id, ActualizarExcepcionProfesionalSolicitud request, CancellationToken ct)
    {
        var solicitud = new ActualizarExcepcionProfesionalSolicitud
        {
            Id = id, Fecha = request.Fecha, TipoExcepcion = request.TipoExcepcion,
            HoraInicio = request.HoraInicio, HoraFin = request.HoraFin,
            Motivo = request.Motivo, Observaciones = request.Observaciones
        };
        var r = await service.ActualizarExcepcionAsync(solicitud, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }

    [HttpDelete("api/excepciones-horario-profesional/{id:guid}")]
    public async Task<IActionResult> EliminarExcepcion(Guid id, CancellationToken ct)
    {
        var r = await service.EliminarExcepcionAsync(id, ct);
        return r.EsExitoso ? NoContent() : OperationProblem(r.Error, r.TipoError);
    }
}
