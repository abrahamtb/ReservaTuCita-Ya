using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administracion)]
public sealed class ServiciosController(IServicioService service) : ApiControllerBase
{
    [HttpGet("api/organizaciones/{organizacionId:guid}/servicios")]
    public async Task<ActionResult<PaginaResultado<ServicioListaDto>>> Listar(
        Guid organizacionId,
        [FromQuery] string? busqueda,
        [FromQuery] Guid? categoriaServicioId,
        [FromQuery] ModalidadServicio? modalidad,
        [FromQuery] EstadoFiltro estado = EstadoFiltro.Todos,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListarAsync(new ServicioFiltroDto(
            organizacionId,
            busqueda,
            categoriaServicioId,
            modalidad,
            estado,
            pagina,
            tamanoPagina), cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/organizaciones/{organizacionId:guid}/servicios/sedes-opciones")]
    public async Task<ActionResult<IReadOnlyList<SedeAsignacionDto>>> ListarSedes(
        Guid organizacionId,
        [FromQuery] Guid? servicioId,
        CancellationToken cancellationToken)
    {
        var result = await service.ObtenerSedesAsignadasAsync(
            organizacionId, servicioId, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpGet("api/servicios/{id:guid}")]
    public async Task<ActionResult<ServicioDetalleDto>> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.ObtenerPorIdAsync(id, cancellationToken);
        return result.EsExitoso && result.Valor is not null
            ? Ok(result.Valor)
            : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPost("api/organizaciones/{organizacionId:guid}/servicios")]
    public async Task<ActionResult<ServicioDetalleDto>> Crear(
        Guid organizacionId,
        CrearServicioSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(CopyCreate(request, organizacionId), cancellationToken);
        if (!result.EsExitoso)
            return OperationProblem(result.Error, result.TipoError);

        var detail = await service.ObtenerPorIdAsync(result.Valor, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Valor }, detail.Valor);
    }

    [HttpPut("api/servicios/{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarServicioSolicitud request,
        CancellationToken cancellationToken)
    {
        var result = await service.ActualizarAsync(CopyUpdate(request, id), cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpPatch("api/servicios/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.CambiarEstadoAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    [HttpDelete("api/servicios/{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.EliminarAsync(id, cancellationToken);
        return result.EsExitoso ? NoContent() : OperationProblem(result.Error, result.TipoError);
    }

    private static CrearServicioSolicitud CopyCreate(
        CrearServicioSolicitud request,
        Guid organizacionId) => new()
    {
        OrganizacionId = organizacionId,
        CategoriaServicioId = request.CategoriaServicioId,
        Nombre = request.Nombre,
        Descripcion = request.Descripcion,
        DuracionMinutos = request.DuracionMinutos,
        Precio = request.Precio,
        MontoAdelanto = request.MontoAdelanto,
        Modalidad = request.Modalidad,
        EsGrupal = request.EsGrupal,
        CapacidadMaxima = request.CapacidadMaxima,
        RequiereProfesional = request.RequiereProfesional,
        RequiereRecurso = request.RequiereRecurso,
        PermiteCancelacion = request.PermiteCancelacion,
        PermiteReprogramacion = request.PermiteReprogramacion,
        HorasLimiteCancelacion = request.HorasLimiteCancelacion,
        TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
        TiempoPosteriorMinutos = request.TiempoPosteriorMinutos,
        Sedes = request.Sedes
    };

    private static ActualizarServicioSolicitud CopyUpdate(
        ActualizarServicioSolicitud request,
        Guid id) => new()
    {
        Id = id,
        CategoriaServicioId = request.CategoriaServicioId,
        Nombre = request.Nombre,
        Descripcion = request.Descripcion,
        DuracionMinutos = request.DuracionMinutos,
        Precio = request.Precio,
        MontoAdelanto = request.MontoAdelanto,
        Modalidad = request.Modalidad,
        EsGrupal = request.EsGrupal,
        CapacidadMaxima = request.CapacidadMaxima,
        RequiereProfesional = request.RequiereProfesional,
        RequiereRecurso = request.RequiereRecurso,
        PermiteCancelacion = request.PermiteCancelacion,
        PermiteReprogramacion = request.PermiteReprogramacion,
        HorasLimiteCancelacion = request.HorasLimiteCancelacion,
        TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
        TiempoPosteriorMinutos = request.TiempoPosteriorMinutos,
        Sedes = request.Sedes
    };
}
