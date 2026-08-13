using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class SedesController(ISedeService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("api/organizaciones/{organizacionId:guid}/sedes")]
    [Authorize(Policy = Permissions.Sedes.Ver)]
    public async Task<ActionResult<IReadOnlyList<SedeListaDto>>> Listar(
        Guid organizacionId,
        [FromQuery] string? busqueda,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        CancellationToken cancellationToken = default)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return Forbid();
        }

        var result = await service.ListarPorOrganizacionAsync(
            new SedeFiltroDto(organizacionId, busqueda, estado), cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/sedes/{id:guid}")]
    [Authorize(Policy = Permissions.Sedes.Ver)]
    public async Task<ActionResult<SedeDetalleDto>> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        if (!result.EsExitoso || result.Valor is null)
        {
            return OperationProblem(result.Error, result.TipoError);
        }

        if (!EsOrganizacionAutorizada(result.Valor.OrganizacionId))
        {
            return NotFound(); // 404 en vez de 403 para no revelar que existe (criterio del ticket)
        }

        return Ok(result.Valor);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/sedes")]
    [Authorize(Policy = Permissions.Sedes.Gestionar)]
    public async Task<ActionResult<SedeDetalleDto>> Crear(
        Guid organizacionId,
        CrearSedeSolicitud request,
        CancellationToken cancellationToken)
    {
        if (!EsOrganizacionAutorizada(organizacionId))
        {
            return Forbid();
        }

        var serverRequest = new CrearSedeSolicitud
        {
            OrganizacionId = organizacionId,
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Correo = request.Correo,
            Referencia = request.Referencia
        };

        var result = await service.CrearAsync(serverRequest, cancellationToken);
        if (!result.EsExitoso)
        {
            return OperationProblem(result.Error, result.TipoError);
        }

        var detail = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detail.Valor);
    }

    [HttpPut("api/sedes/{id:guid}")]
    [Authorize(Policy = Permissions.Sedes.Gestionar)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarSedeSolicitud request,
        CancellationToken cancellationToken)
    {
        var existente = await service.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
        {
            return OperationProblem(existente.Error, existente.TipoError);
        }

        if (!EsOrganizacionAutorizada(existente.Valor.OrganizacionId))
        {
            return NotFound();
        }

        var result = await service.ActualizarAsync(new ActualizarSedeSolicitud
        {
            Id = id,
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Correo = request.Correo,
            Referencia = request.Referencia
        }, cancellationToken);

        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/sedes/{id:guid}/estado")]
    [Authorize(Policy = Permissions.Sedes.Gestionar)]
    public async Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken)
    {
        var existente = await service.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
        {
            return OperationProblem(existente.Error, existente.TipoError);
        }

        if (!EsOrganizacionAutorizada(existente.Valor.OrganizacionId))
        {
            return NotFound();
        }

        var result = await service.CambiarEstadoAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/sedes/{id:guid}")]
    [Authorize(Policy = Permissions.Sedes.Gestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var existente = await service.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
        {
            return OperationProblem(existente.Error, existente.TipoError);
        }

        if (!EsOrganizacionAutorizada(existente.Valor.OrganizacionId))
        {
            return NotFound();
        }

        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    private bool EsOrganizacionAutorizada(Guid organizacionId) =>
        currentUser.IsInRole(RoleNames.Superadministrador) ||
        currentUser.OrganizacionId == organizacionId;
}