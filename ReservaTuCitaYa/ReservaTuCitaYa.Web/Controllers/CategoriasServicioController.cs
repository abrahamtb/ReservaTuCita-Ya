using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Web.ViewModels.CategoriasServicio;

namespace ReservaTuCitaYa.Web.Controllers;

[Authorize(Roles = RoleNames.Administracion)]
public sealed class CategoriasServicioController(
    ICategoriaServicioService categoriaService,
    IOrganizacionService organizacionService,
    ILogger<CategoriasServicioController> logger) : Controller
{
    public async Task<IActionResult> Index(
        Guid organizacionId,
        string? busqueda,
        EstadoFiltro estado = EstadoFiltro.Todos,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        var resultado = await categoriaService.ListarAsync(
            new CategoriaServicioFiltroDto(organizacionId, busqueda, estado, pagina), cancellationToken);
        if (!organizacion.EsExitoso || organizacion.Valor is null ||
            !resultado.EsExitoso || resultado.Valor is null)
            return NotFound();

        return View(new CategoriaServicioIndexViewModel
        {
            OrganizacionId = organizacionId,
            Organizacion = organizacion.Valor.NombreComercial,
            Busqueda = busqueda,
            Estado = estado,
            Resultado = resultado.Valor
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await categoriaService.ObtenerPorIdAsync(id, cancellationToken);
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
        CategoriaServicioFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        var servidor = await CrearModeloAsync(modelo.OrganizacionId, cancellationToken);
        if (servidor is null)
            return NotFound();
        modelo.Organizacion = servidor.Organizacion;

        if (ModelState.IsValid)
        {
            var resultado = await categoriaService.CrearAsync(new CrearCategoriaServicioSolicitud
            {
                OrganizacionId = modelo.OrganizacionId,
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion
            }, cancellationToken);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La categoría se creó correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(modelo);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await categoriaService.ObtenerPorIdAsync(id, cancellationToken);
        if (!resultado.EsExitoso || resultado.Valor is null)
            return NotFound();
        var categoria = resultado.Valor;
        return View(new CategoriaServicioFormularioViewModel
        {
            Id = categoria.Id,
            OrganizacionId = categoria.OrganizacionId,
            Organizacion = categoria.Organizacion,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        CategoriaServicioFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (id != modelo.Id)
            return BadRequest();
        var existente = await categoriaService.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
            return NotFound();
        modelo.OrganizacionId = existente.Valor.OrganizacionId;
        modelo.Organizacion = existente.Valor.Organizacion;

        if (ModelState.IsValid)
        {
            var resultado = await categoriaService.ActualizarAsync(
                new ActualizarCategoriaServicioSolicitud
                {
                    Id = id,
                    Nombre = modelo.Nombre,
                    Descripcion = modelo.Descripcion
                }, cancellationToken);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "La categoría se actualizó correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        bool confirmarServiciosActivos,
        CancellationToken cancellationToken) =>
        await EjecutarAsync(
            id,
            () => categoriaService.CambiarEstadoAsync(
                id, confirmarServiciosActivos, cancellationToken),
            "El estado de la categoría se actualizó correctamente.",
            cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken) =>
        await EjecutarAsync(
            id,
            () => categoriaService.EliminarAsync(id, cancellationToken),
            "La categoría se eliminó correctamente.",
            cancellationToken);

    private async Task<CategoriaServicioFormularioViewModel?> CrearModeloAsync(
        Guid organizacionId,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        return organizacion.EsExitoso && organizacion.Valor is not null
            ? new CategoriaServicioFormularioViewModel
            {
                OrganizacionId = organizacionId,
                Organizacion = organizacion.Valor.NombreComercial
            }
            : null;
    }

    private async Task<IActionResult> EjecutarAsync(
        Guid id,
        Func<Task<ResultadoOperacion>> operacion,
        string mensajeExito,
        CancellationToken cancellationToken)
    {
        var existente = await categoriaService.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
            return NotFound();

        try
        {
            var resultado = await operacion();
            TempData[resultado.EsExitoso ? "Exito" : "Error"] =
                resultado.EsExitoso ? mensajeExito : resultado.Error;
        }
        catch (Exception excepcion)
        {
            logger.LogError(excepcion, "Error inesperado al modificar la categoría {CategoriaId}.", id);
            TempData["Error"] = "No fue posible completar la operación. Intente nuevamente.";
        }
        return RedirectToAction(nameof(Index), new { organizacionId = existente.Valor.OrganizacionId });
    }
}
