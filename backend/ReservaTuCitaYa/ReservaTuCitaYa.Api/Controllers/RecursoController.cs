// Api/Controllers/RecursosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Recursos;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;
namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class RecursosController(IRecursoService service) : ApiControllerBase
{
    [HttpGet("api/sedes/{sedeId:guid}/recursos")]
    public async Task<ActionResult<PaginaResultado<RecursoListaDto>>> Listar(
        Guid sedeId, [FromQuery] string? busqueda, [FromQuery] string? tipoRecurso,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        [FromQuery] Guid? servicioId = null, [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        var result = await service.ListarAsync(new RecursoFiltroDto(
            sedeId, busqueda, tipoRecurso, estado, servicioId, pagina, tamanoPagina), cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/recursos/{id:guid}")]
    public async Task<ActionResult<RecursoDetalleDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/sedes/{sedeId:guid}/recursos")]
    public async Task<ActionResult<RecursoDetalleDto>> Crear(
        Guid sedeId, CrearRecursosRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(new CrearRecursoSolicitud
        {
            SedeId = sedeId,
            Nombre = request.Nombre,
            Codigo = request.Codigo,
            Descripcion = request.Descripcion,
            TipoRecurso = request.TipoRecurso,
            Capacidad = request.Capacidad,
            UbicacionInterna = request.UbicacionInterna,
            Observaciones = request.Observaciones,
            Servicios = request.Servicios
        }, cancellationToken);
        if (!result.EsExitoso) return RecursoProblem(result.Error, result.TipoError);
        var detalle = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detalle.Valor);
    }

    [HttpPut("api/recursos/{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarRecursosRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(new ActualizarRecursoSolicitud
        {
            Id = id,
            Nombre = request.Nombre,
            Codigo = request.Codigo,
            Descripcion = request.Descripcion,
            TipoRecurso = request.TipoRecurso,
            Capacidad = request.Capacidad,
            UbicacionInterna = request.UbicacionInterna,
            Observaciones = request.Observaciones
        }, cancellationToken);

        return result.EsExitoso
            ? NoContent()
            : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/recursos/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id, CambiarEstadoRecursosRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CambiarEstadoAsync(id, request.EstaActivo, cancellationToken);
        return result.EsExitoso ? NoContent() : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/recursos/{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/recursos/{id:guid}/servicios")]
    public async Task<ActionResult<IReadOnlyList<RecursoServicioDto>>> ListarServicios(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ListarServiciosAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : RecursoProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/recursos/{id:guid}/servicios")]
    public async Task<IActionResult> ReemplazarServicios(
        Guid id, ReemplazarServiciosRecursosRequest request, CancellationToken cancellationToken)
    {
        var result = await service.ReemplazarServiciosAsync(id, request.Servicios, cancellationToken);
        return result.EsExitoso ? NoContent() : RecursoProblem(result.Error, result.TipoError);
    }

    private ObjectResult RecursoProblem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            RecursoService.CodigoDuplicado => ("resource-code-duplicate", "Código duplicado"),
            RecursoService.SedeInvalida => ("resource-site-invalid", "Sede inválida"),
            RecursoService.ServicioSedeInvalido => ("resource-service-site-mismatch", "Servicio no ofrecido en la sede"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}