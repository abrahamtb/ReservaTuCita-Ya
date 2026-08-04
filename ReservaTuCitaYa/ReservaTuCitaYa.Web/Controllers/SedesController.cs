using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Web.ViewModels.Sedes;

namespace ReservaTuCitaYa.Web.Controllers;

[Authorize(Roles = RoleNames.Administracion)]
public sealed class SedesController(
    ISedeService sedeService,
    IOrganizacionService organizacionService,
    ILogger<SedesController> logger) : Controller
{
    public async Task<IActionResult> Index(
        Guid organizacionId,
        string? busqueda,
        EstadoFiltro estado = EstadoFiltro.Todos,
        CancellationToken cancellationToken = default)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        if (!organizacion.EsExitoso || organizacion.Valor is null)
        {
            return NotFound();
        }

        var resultado = await sedeService.ListarPorOrganizacionAsync(
            new SedeFiltroDto(organizacionId, busqueda, estado), cancellationToken);
        if (!resultado.EsExitoso)
        {
            return NotFound();
        }

        return View(new SedeIndexViewModel
        {
            OrganizacionId = organizacionId,
            Organizacion = organizacion.Valor.NombreComercial,
            Busqueda = busqueda,
            Estado = estado,
            Sedes = resultado.Valor ?? []
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await sedeService.ObtenerPorIdAsync(id, cancellationToken);
        return resultado.EsExitoso ? View(resultado.Valor) : NotFound();
    }

    public async Task<IActionResult> Create(Guid organizacionId, CancellationToken cancellationToken)
    {
        var modelo = await CrearModeloAsync(organizacionId, cancellationToken);
        return modelo is null ? NotFound() : View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SedeFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(
            modelo.OrganizacionId, cancellationToken);
        if (!organizacion.EsExitoso || organizacion.Valor is null)
        {
            return NotFound();
        }
        modelo.Organizacion = organizacion.Valor.NombreComercial;

        if (ModelState.IsValid)
        {
            var resultado = await sedeService.CrearAsync(new CrearSedeSolicitud
            {
                OrganizacionId = modelo.OrganizacionId,
                Nombre = modelo.Nombre,
                Direccion = modelo.Direccion,
                Telefono = modelo.Telefono,
                Correo = modelo.Correo,
                Referencia = modelo.Referencia
            }, cancellationToken);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La sede se creó correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        return View(modelo);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await sedeService.ObtenerPorIdAsync(id, cancellationToken);
        if (!resultado.EsExitoso || resultado.Valor is null)
        {
            return NotFound();
        }

        var sede = resultado.Valor;
        return View(new SedeFormularioViewModel
        {
            Id = sede.Id,
            OrganizacionId = sede.OrganizacionId,
            Organizacion = sede.Organizacion,
            Nombre = sede.Nombre,
            Direccion = sede.Direccion,
            Telefono = sede.Telefono,
            Correo = sede.Correo,
            Referencia = sede.Referencia
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        SedeFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        var existente = await sedeService.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
        {
            return NotFound();
        }
        modelo.OrganizacionId = existente.Valor.OrganizacionId;
        modelo.Organizacion = existente.Valor.Organizacion;

        if (ModelState.IsValid)
        {
            var resultado = await sedeService.ActualizarAsync(new ActualizarSedeSolicitud
            {
                Id = id,
                Nombre = modelo.Nombre,
                Direccion = modelo.Direccion,
                Telefono = modelo.Telefono,
                Correo = modelo.Correo,
                Referencia = modelo.Referencia
            }, cancellationToken);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La sede se actualizó correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken)
    {
        var sede = await sedeService.ObtenerPorIdAsync(id, cancellationToken);
        if (!sede.EsExitoso || sede.Valor is null)
        {
            return NotFound();
        }
        return await EjecutarOperacionAsync(
            () => sedeService.CambiarEstadoAsync(id, cancellationToken),
            sede.Valor.OrganizacionId,
            "El estado de la sede se actualizó correctamente.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var sede = await sedeService.ObtenerPorIdAsync(id, cancellationToken);
        if (!sede.EsExitoso || sede.Valor is null)
        {
            return NotFound();
        }
        return await EjecutarOperacionAsync(
            () => sedeService.EliminarAsync(id, cancellationToken),
            sede.Valor.OrganizacionId,
            "La sede se eliminó correctamente.");
    }

    private async Task<SedeFormularioViewModel?> CrearModeloAsync(
        Guid organizacionId,
        CancellationToken cancellationToken)
    {
        var resultado = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        return resultado.EsExitoso && resultado.Valor is not null
            ? new SedeFormularioViewModel
            {
                OrganizacionId = organizacionId,
                Organizacion = resultado.Valor.NombreComercial
            }
            : null;
    }

    private async Task<IActionResult> EjecutarOperacionAsync(
        Func<Task<ResultadoOperacion>> operacion,
        Guid organizacionId,
        string mensajeExito)
    {
        try
        {
            var resultado = await operacion();
            TempData[resultado.EsExitoso ? "Exito" : "Error"] =
                resultado.EsExitoso ? mensajeExito : resultado.Error;
        }
        catch (Exception excepcion)
        {
            logger.LogError(excepcion, "Ocurrió un error al modificar una sede.");
            TempData["Error"] = "No fue posible completar la operación. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Index), new { organizacionId });
    }
}
