using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class CategoriasController(ICategoriaServicioService service) : ApiControllerBase
{
    [HttpGet("api/organizaciones/{organizacionId:guid}/categorias")]
    public async Task<ActionResult<PaginaResultado<CategoriaServicioListaDto>>> Listar(
        Guid organizacionId,
        [FromQuery] string? busqueda,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListarAsync(new CategoriaServicioFiltroDto(
            organizacionId, busqueda, estado, pagina, tamanoPagina), cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/categorias/opciones")]
    public async Task<ActionResult<IReadOnlyList<CategoriaServicioOpcionDto>>> ListarOpciones(
        Guid organizacionId,
        CancellationToken cancellationToken) =>
        Ok(await service.ListarActivasAsync(organizacionId, cancellationToken));

    [HttpGet("api/categorias/{id:guid}")]
    public async Task<ActionResult<CategoriaServicioDetalleDto>> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/categorias")]
    public async Task<ActionResult<CategoriaServicioDetalleDto>> Crear(
        Guid organizacionId,
        CrearCategoriaServicioSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(new CrearCategoriaServicioSolicitud
        {
            OrganizacionId = organizacionId,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion
        }, cancellationToken);
        if (!result.EsExitoso)
            return OperationProblem(result.Error, result.TipoError);

        var detail = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detail.Valor);
    }

    [HttpPut("api/categorias/{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarCategoriaServicioSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(new ActualizarCategoriaServicioSolicitud
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion
        }, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/categorias/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        CambiarEstadoCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CambiarEstadoAsync(
            id, request.ConfirmarServiciosActivos, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/categorias/{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }
}
