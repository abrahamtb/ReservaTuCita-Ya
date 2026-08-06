using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
[Route("api/organizaciones")]
public sealed class OrganizacionesController(IOrganizacionService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginaResultado<OrganizacionListaDto>>> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListarPaginadoAsync(
            new OrganizacionFiltroDto(busqueda, estado, pagina, tamanoPagina),
            cancellationToken));

    [HttpGet("tipos")]
    public async Task<ActionResult<IReadOnlyList<TipoOrganizacionOpcionDto>>> ListarTipos(
        CancellationToken cancellationToken) =>
        Ok(await service.ListarTiposActivosAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizacionDetalleDto>> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizacionDetalleDto>> Crear(
        CrearOrganizacionSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(request, cancellationToken);
        if (!result.EsExitoso)
            return OperationProblem(result.Error, result.TipoError);

        var detail = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detail.Valor);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarOrganizacionSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(CopyWithId(request, id), cancellationToken);
        return result.EsExitoso
            ? NoContent()
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.CambiarEstadoAsync(id, cancellationToken);
        return result.EsExitoso
            ? NoContent()
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso
            ? NoContent()
            : OperationProblem(result.Error, result.TipoError);
    }

    private static ActualizarOrganizacionSolicitud CopyWithId(
        ActualizarOrganizacionSolicitud request,
        Guid id) => new()
    {
        Id = id,
        TipoOrganizacionId = request.TipoOrganizacionId,
        NombreComercial = request.NombreComercial,
        RazonSocial = request.RazonSocial,
        NumeroDocumento = request.NumeroDocumento,
        Telefono = request.Telefono,
        Correo = request.Correo,
        DireccionPrincipal = request.DireccionPrincipal,
        LogoUrl = request.LogoUrl
    };
}
