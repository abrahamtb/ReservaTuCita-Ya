using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Web.ViewModels.Organizaciones;

namespace ReservaTuCitaYa.Web.Controllers;

[Authorize(Roles = RoleNames.Administracion)]
public sealed class OrganizacionesController(
    IOrganizacionService organizacionService,
    ILogger<OrganizacionesController> logger) : Controller
{
    public async Task<IActionResult> Index(
        string? busqueda,
        EstadoFiltro estado = EstadoFiltro.Todos,
        CancellationToken cancellationToken = default)
    {
        var organizaciones = await organizacionService.ListarAsync(
            new OrganizacionFiltroDto(busqueda, estado), cancellationToken);

        return View(new OrganizacionIndexViewModel
        {
            Busqueda = busqueda,
            Estado = estado,
            Organizaciones = organizaciones
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await organizacionService.ObtenerPorIdAsync(id, cancellationToken);
        return resultado.EsExitoso ? View(resultado.Valor) : NotFound();
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var modelo = new OrganizacionFormularioViewModel();
        await CargarTiposAsync(modelo, cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        OrganizacionFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var resultado = await organizacionService.CrearAsync(new CrearOrganizacionSolicitud
            {
                TipoOrganizacionId = modelo.TipoOrganizacionId!.Value,
                NombreComercial = modelo.NombreComercial,
                RazonSocial = modelo.RazonSocial,
                NumeroDocumento = modelo.NumeroDocumento,
                Telefono = modelo.Telefono,
                Correo = modelo.Correo,
                DireccionPrincipal = modelo.DireccionPrincipal,
                LogoUrl = modelo.LogoUrl
            }, cancellationToken);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La organización se creó correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        await CargarTiposAsync(modelo, cancellationToken);
        return View(modelo);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await organizacionService.ObtenerPorIdAsync(id, cancellationToken);
        if (!resultado.EsExitoso || resultado.Valor is null)
        {
            return NotFound();
        }

        var organizacion = resultado.Valor;
        var modelo = new OrganizacionFormularioViewModel
        {
            Id = organizacion.Id,
            TipoOrganizacionId = organizacion.TipoOrganizacionId,
            NombreComercial = organizacion.NombreComercial,
            RazonSocial = organizacion.RazonSocial,
            NumeroDocumento = organizacion.NumeroDocumento,
            Telefono = organizacion.Telefono,
            Correo = organizacion.Correo,
            DireccionPrincipal = organizacion.DireccionPrincipal,
            LogoUrl = organizacion.LogoUrl
        };
        await CargarTiposAsync(modelo, cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        OrganizacionFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var resultado = await organizacionService.ActualizarAsync(new ActualizarOrganizacionSolicitud
            {
                Id = id,
                TipoOrganizacionId = modelo.TipoOrganizacionId!.Value,
                NombreComercial = modelo.NombreComercial,
                RazonSocial = modelo.RazonSocial,
                NumeroDocumento = modelo.NumeroDocumento,
                Telefono = modelo.Telefono,
                Correo = modelo.Correo,
                DireccionPrincipal = modelo.DireccionPrincipal,
                LogoUrl = modelo.LogoUrl
            }, cancellationToken);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La organización se actualizó correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        await CargarTiposAsync(modelo, cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken) =>
        EjecutarOperacionAsync(
            () => organizacionService.CambiarEstadoAsync(id, cancellationToken),
            "El estado de la organización se actualizó correctamente.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken) =>
        EjecutarOperacionAsync(
            () => organizacionService.EliminarAsync(id, cancellationToken),
            "La organización se eliminó correctamente.");

    private async Task CargarTiposAsync(
        OrganizacionFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        modelo.TiposOrganizacion = (await organizacionService.ListarTiposActivosAsync(cancellationToken))
            .Select(tipo => new SelectListItem(tipo.Nombre, tipo.Id.ToString()))
            .ToArray();
    }

    private async Task<IActionResult> EjecutarOperacionAsync(
        Func<Task<Application.Common.ResultadoOperacion>> operacion,
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
            logger.LogError(excepcion, "Ocurrió un error al modificar una organización.");
            TempData["Error"] = "No fue posible completar la operación. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
