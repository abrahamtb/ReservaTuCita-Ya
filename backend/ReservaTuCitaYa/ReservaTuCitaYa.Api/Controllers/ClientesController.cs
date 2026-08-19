using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Clientes;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ClientesController(IClienteService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("api/organizaciones/{organizacionId:guid}/clientes")]
    [Authorize(Policy = Permissions.Clientes.Ver)]
    public async Task<ActionResult<PaginaResultado<ClienteListaDto>>> Listar(
        Guid organizacionId, [FromQuery] string? busqueda, [FromQuery] TipoDocumento? tipoDocumento,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos, [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        var result = await service.ListarAsync(new ClienteFiltroDto(
            organizacionId, busqueda, tipoDocumento, estado, pagina, tamanoPagina), cancellationToken);
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ClienteProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/clientes/{id:guid}")]
    [Authorize(Policy = Permissions.Clientes.Ver)]
    public async Task<ActionResult<ClienteDetalleDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        if (result.EsExitoso && result.Valor is not null && !EsOrganizacionAutorizada(result.Valor.OrganizacionId)) return NotFound();
        return result.EsExitoso && result.Valor is not null ? Ok(result.Valor) : ClienteProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/clientes")]
    [Authorize(Policy = Permissions.Clientes.Crear)]
    public async Task<ActionResult<ClienteDetalleDto>> Crear(Guid organizacionId, CrearClienteRequest request, CancellationToken cancellationToken)
    {
        if (!EsOrganizacionAutorizada(organizacionId)) return NotFound();
        var result = await service.CrearAsync(new CrearClienteSolicitud
        {
            OrganizacionId = organizacionId, TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento, Nombres = request.Nombres, Apellidos = request.Apellidos,
            Correo = request.Correo, Telefono = request.Telefono, Direccion = request.Direccion,
            FechaNacimiento = request.FechaNacimiento, Observaciones = request.Observaciones
        }, cancellationToken);
        if (!result.EsExitoso) return ClienteProblem(result.Error, result.TipoError);
        var detail = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detail.Valor);
    }

    [HttpPut("api/clientes/{id:guid}")]
    [Authorize(Policy = Permissions.Clientes.Editar)]
    public async Task<IActionResult> Actualizar(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await EsClienteAutorizadoAsync(id, cancellationToken)) return NotFound();
        var result = await service.ActualizarAsync(new ActualizarClienteSolicitud
        {
            Id = id, TipoDocumento = request.TipoDocumento, NumeroDocumento = request.NumeroDocumento,
            Nombres = request.Nombres, Apellidos = request.Apellidos, Correo = request.Correo,
            Telefono = request.Telefono, Direccion = request.Direccion, FechaNacimiento = request.FechaNacimiento,
            Observaciones = request.Observaciones
        }, cancellationToken);
        return result.EsExitoso ? NoContent() : ClienteProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/clientes/{id:guid}/estado")]
    [Authorize(Policy = Permissions.Clientes.Editar)]
    public async Task<IActionResult> CambiarEstado(Guid id, CambiarEstadoClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await EsClienteAutorizadoAsync(id, cancellationToken)) return NotFound();
        var result = await service.CambiarEstadoAsync(id, request.EstaActivo, cancellationToken);
        return result.EsExitoso ? NoContent() : ClienteProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/clientes/{id:guid}")]
    [Authorize(Policy = Permissions.Clientes.Eliminar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        if (!await EsClienteAutorizadoAsync(id, cancellationToken)) return NotFound();
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : ClienteProblem(result.Error, result.TipoError);
    }

    private bool EsOrganizacionAutorizada(Guid organizacionId) =>
        currentUser.IsInRole(RoleNames.Superadministrador) || currentUser.OrganizacionId == organizacionId;

    private async Task<bool> EsClienteAutorizadoAsync(Guid id, CancellationToken ct)
    {
        var existente = await service.ObtenerPorIdAsync(id, ct);
        return existente.EsExitoso && existente.Valor is not null && EsOrganizacionAutorizada(existente.Valor.OrganizacionId);
    }

    private ObjectResult ClienteProblem(string? detail, TipoErrorOperacion errorType) =>
        errorType == TipoErrorOperacion.Conflicto
            ? OperationProblem(detail, errorType, "client-document-duplicate", "Cliente duplicado")
            : OperationProblem(detail, errorType);
}
