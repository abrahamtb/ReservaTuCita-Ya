using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Api.Contracts.Empleados;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize]
public sealed class EmpleadosController(IEmpleadoService service) : ApiControllerBase
{
    [HttpGet("api/organizaciones/{organizacionId:guid}/empleados")]
    [Authorize(Policy = Permissions.Empleados.Ver)]
    public async Task<ActionResult<PaginaResultado<EmpleadoListaDto>>> Listar(
        Guid organizacionId,
        [FromQuery] string? busqueda,
        [FromQuery] TipoDocumento? tipoDocumento,
        [FromQuery] bool? esProfesional,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        [FromQuery] Guid? sedeId = null,
        [FromQuery] Guid? servicioId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListarAsync(new EmpleadoFiltroDto(
            organizacionId, busqueda, tipoDocumento, esProfesional, estado,
            sedeId, servicioId, pagina, tamanoPagina), cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/empleados/{id:guid}")]
    [Authorize(Policy = Permissions.Empleados.Ver)]
    public async Task<ActionResult<EmpleadoDetalleDto>> Obtener(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/empleados")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<ActionResult<EmpleadoDetalleDto>> Crear(
        Guid organizacionId, CrearEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(new CrearEmpleadoSolicitud
        {
            OrganizacionId = organizacionId,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            FechaNacimiento = request.FechaNacimiento,
            Cargo = request.Cargo,
            Especialidad = request.Especialidad,
            EsProfesional = request.EsProfesional,
            NumeroColegiatura = request.NumeroColegiatura,
            Observaciones = request.Observaciones,
            SedeIds = request.SedeIds,
            ServicioIds = request.ServicioIds
        }, cancellationToken);
        if (!result.EsExitoso) return EmpleadoProblem(result.Error, result.TipoError);
        var detalle = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detalle.Valor);
    }

    [HttpPut("api/empleados/{id:guid}")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<IActionResult> Actualizar(
        Guid id, ActualizarEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(new ActualizarEmpleadoSolicitud
        {
            Id = id,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            FechaNacimiento = request.FechaNacimiento,
            Cargo = request.Cargo,
            Especialidad = request.Especialidad,
            EsProfesional = request.EsProfesional,
            NumeroColegiatura = request.NumeroColegiatura,
            Observaciones = request.Observaciones
        }, cancellationToken);
        return result.EsExitoso ? NoContent() : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/empleados/{id:guid}/estado")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<IActionResult> CambiarEstado(
        Guid id, CambiarEstadoEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CambiarEstadoAsync(id, request.EstaActivo, cancellationToken);
        return result.EsExitoso ? NoContent() : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/empleados/{id:guid}")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/empleados/{id:guid}/sedes")]
    [Authorize(Policy = Permissions.Empleados.Ver)]
    public async Task<ActionResult<IReadOnlyList<EmpleadoSedeDto>>> ListarSedes(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ListarSedesAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/empleados/{id:guid}/sedes")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<IActionResult> ReemplazarSedes(
        Guid id, ReemplazarSedesEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var result = await service.ReemplazarSedesAsync(id, request.SedeIds, cancellationToken);
        return result.EsExitoso ? NoContent() : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/empleados/{id:guid}/servicios")]
    [Authorize(Policy = Permissions.Empleados.Ver)]
    public async Task<ActionResult<IReadOnlyList<ProfesionalServicioDto>>> ListarServicios(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ListarServiciosAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor) : EmpleadoProblem(result.Error, result.TipoError);
    }

    [HttpPut("api/empleados/{id:guid}/servicios")]
    [Authorize(Policy = Permissions.Empleados.Gestionar)]
    public async Task<IActionResult> ReemplazarServicios(
        Guid id, ReemplazarServiciosProfesionalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.ReemplazarServiciosAsync(
            id, request.ServicioIds, cancellationToken);
        return result.EsExitoso ? NoContent() : EmpleadoProblem(result.Error, result.TipoError);
    }

    private ObjectResult EmpleadoProblem(string? detail, TipoErrorOperacion errorType)
    {
        var (type, title) = detail switch
        {
            EmpleadoService.DocumentoDuplicado =>
                ("employee-document-duplicate", "Empleado duplicado"),
            EmpleadoService.SedeOrganizacionInvalida =>
                ("employee-site-organization-mismatch", "Sede de otra organización"),
            EmpleadoService.ServicioOrganizacionInvalida =>
                ("professional-service-organization-mismatch", "Servicio de otra organización"),
            EmpleadoService.EmpleadoNoProfesional =>
                ("employee-not-professional", "Empleado no profesional"),
            EmpleadoService.ProfesionalConServicios =>
                ("employee-has-professional-services", "Profesional con servicios"),
            _ => ((string?)null, (string?)null)
        };
        return OperationProblem(detail, errorType, type, title);
    }
}
