using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Web.ViewModels.Servicios;

namespace ReservaTuCitaYa.Web.Controllers;

[Authorize(Roles = RoleNames.Administracion)]
public sealed class ServiciosController(
    IServicioService servicioService,
    ICategoriaServicioService categoriaService,
    IOrganizacionService organizacionService,
    ILogger<ServiciosController> logger) : Controller
{
    public async Task<IActionResult> Index(
        Guid organizacionId,
        string? busqueda,
        Guid? categoriaServicioId,
        ModalidadServicio? modalidad,
        EstadoFiltro estado = EstadoFiltro.Todos,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        var resultado = await servicioService.ListarAsync(new ServicioFiltroDto(
            organizacionId, busqueda, categoriaServicioId, modalidad, estado, pagina), cancellationToken);
        if (!organizacion.EsExitoso || organizacion.Valor is null ||
            !resultado.EsExitoso || resultado.Valor is null)
            return NotFound();

        return View(new ServicioIndexViewModel
        {
            OrganizacionId = organizacionId,
            Organizacion = organizacion.Valor.NombreComercial,
            Busqueda = busqueda,
            CategoriaServicioId = categoriaServicioId,
            Modalidad = modalidad,
            Estado = estado,
            Categorias = await CrearOpcionesCategoriaAsync(organizacionId, cancellationToken),
            Resultado = resultado.Valor
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await servicioService.ObtenerPorIdAsync(id, cancellationToken);
        return resultado.EsExitoso ? View(resultado.Valor) : NotFound();
    }

    public async Task<IActionResult> Create(Guid organizacionId, CancellationToken cancellationToken)
    {
        var modelo = await ConstruirFormularioAsync(organizacionId, null, cancellationToken);
        return modelo is null ? NotFound() : View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ServicioFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (modelo.MontoAdelanto > modelo.Precio)
            ModelState.AddModelError(nameof(modelo.MontoAdelanto), "El adelanto no puede superar el precio.");
        if (!modelo.EsGrupal && modelo.CapacidadMaxima != 1)
            ModelState.AddModelError(nameof(modelo.CapacidadMaxima), "Un servicio individual debe tener capacidad uno.");

        if (ModelState.IsValid)
        {
            var resultado = await servicioService.CrearAsync(
                CrearSolicitud(modelo), cancellationToken);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "El servicio se creó correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        if (!await RecargarFormularioAsync(modelo, null, cancellationToken))
            return NotFound();
        return View(modelo);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var detalle = await servicioService.ObtenerPorIdAsync(id, cancellationToken);
        if (!detalle.EsExitoso || detalle.Valor is null)
            return NotFound();
        var servicio = detalle.Valor;
        var modelo = await ConstruirFormularioAsync(
            servicio.OrganizacionId, servicio.Id, cancellationToken);
        if (modelo is null)
            return NotFound();

        modelo.Id = servicio.Id;
        modelo.CategoriaServicioId = servicio.CategoriaServicioId;
        modelo.Nombre = servicio.Nombre;
        modelo.Descripcion = servicio.Descripcion;
        modelo.DuracionMinutos = servicio.DuracionMinutos;
        modelo.Precio = servicio.Precio;
        modelo.MontoAdelanto = servicio.MontoAdelanto;
        modelo.Modalidad = servicio.Modalidad;
        modelo.EsGrupal = servicio.EsGrupal;
        modelo.CapacidadMaxima = servicio.CapacidadMaxima;
        modelo.RequiereProfesional = servicio.RequiereProfesional;
        modelo.RequiereRecurso = servicio.RequiereRecurso;
        modelo.PermiteCancelacion = servicio.PermiteCancelacion;
        modelo.PermiteReprogramacion = servicio.PermiteReprogramacion;
        modelo.HorasLimiteCancelacion = servicio.HorasLimiteCancelacion;
        modelo.TiempoPreparacionMinutos = servicio.TiempoPreparacionMinutos;
        modelo.TiempoPosteriorMinutos = servicio.TiempoPosteriorMinutos;
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        ServicioFormularioViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (id != modelo.Id)
            return BadRequest();
        var existente = await servicioService.ObtenerPorIdAsync(id, cancellationToken);
        if (!existente.EsExitoso || existente.Valor is null)
            return NotFound();
        modelo.OrganizacionId = existente.Valor.OrganizacionId;

        if (modelo.MontoAdelanto > modelo.Precio)
            ModelState.AddModelError(nameof(modelo.MontoAdelanto), "El adelanto no puede superar el precio.");
        if (!modelo.EsGrupal && modelo.CapacidadMaxima != 1)
            ModelState.AddModelError(nameof(modelo.CapacidadMaxima), "Un servicio individual debe tener capacidad uno.");

        if (ModelState.IsValid)
        {
            var resultado = await servicioService.ActualizarAsync(
                ActualizarSolicitud(modelo), cancellationToken);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "El servicio se actualizó correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        if (!await RecargarFormularioAsync(modelo, id, cancellationToken))
            return NotFound();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken) =>
        EjecutarAsync(id, () => servicioService.CambiarEstadoAsync(id, cancellationToken),
            "El estado del servicio se actualizó correctamente.", cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken) =>
        EjecutarAsync(id, () => servicioService.EliminarAsync(id, cancellationToken),
            "El servicio se eliminó correctamente.", cancellationToken);

    private async Task<ServicioFormularioViewModel?> ConstruirFormularioAsync(
        Guid organizacionId,
        Guid? servicioId,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionService.ObtenerPorIdAsync(organizacionId, cancellationToken);
        var sedes = await servicioService.ObtenerSedesAsignadasAsync(
            organizacionId, servicioId, cancellationToken);
        if (!organizacion.EsExitoso || organizacion.Valor is null ||
            !sedes.EsExitoso || sedes.Valor is null)
            return null;

        return new ServicioFormularioViewModel
        {
            OrganizacionId = organizacionId,
            Organizacion = organizacion.Valor.NombreComercial,
            Categorias = await CrearOpcionesCategoriaAsync(organizacionId, cancellationToken),
            Sedes = sedes.Valor.Select(sede => new SedeSeleccionViewModel
            {
                SedeId = sede.SedeId,
                Nombre = sede.Nombre,
                Seleccionada = sede.EstaAsignada,
                PrecioEspecial = sede.PrecioEspecial
            }).ToList()
        };
    }

    private async Task<bool> RecargarFormularioAsync(
        ServicioFormularioViewModel modelo,
        Guid? servicioId,
        CancellationToken cancellationToken)
    {
        var servidor = await ConstruirFormularioAsync(
            modelo.OrganizacionId, servicioId, cancellationToken);
        if (servidor is null)
            return false;

        modelo.Organizacion = servidor.Organizacion;
        modelo.Categorias = servidor.Categorias;
        var valoresEnviados = modelo.Sedes.ToDictionary(sede => sede.SedeId);
        modelo.Sedes = servidor.Sedes.Select(sede =>
        {
            if (!valoresEnviados.TryGetValue(sede.SedeId, out var enviada))
                return sede;
            sede.Seleccionada = enviada.Seleccionada;
            sede.PrecioEspecial = enviada.PrecioEspecial;
            return sede;
        }).ToList();
        return true;
    }

    private async Task<IReadOnlyList<SelectListItem>> CrearOpcionesCategoriaAsync(
        Guid organizacionId,
        CancellationToken cancellationToken) =>
        (await categoriaService.ListarActivasAsync(organizacionId, cancellationToken))
            .Select(categoria => new SelectListItem(categoria.Nombre, categoria.Id.ToString()))
            .ToArray();

    private static CrearServicioSolicitud CrearSolicitud(ServicioFormularioViewModel modelo) =>
        new()
        {
            OrganizacionId = modelo.OrganizacionId,
            CategoriaServicioId = modelo.CategoriaServicioId ?? Guid.Empty,
            Nombre = modelo.Nombre,
            Descripcion = modelo.Descripcion,
            DuracionMinutos = modelo.DuracionMinutos,
            Precio = modelo.Precio,
            MontoAdelanto = modelo.MontoAdelanto,
            Modalidad = modelo.Modalidad ?? ModalidadServicio.NoDefinido,
            EsGrupal = modelo.EsGrupal,
            CapacidadMaxima = modelo.CapacidadMaxima,
            RequiereProfesional = modelo.RequiereProfesional,
            RequiereRecurso = modelo.RequiereRecurso,
            PermiteCancelacion = modelo.PermiteCancelacion,
            PermiteReprogramacion = modelo.PermiteReprogramacion,
            HorasLimiteCancelacion = modelo.HorasLimiteCancelacion,
            TiempoPreparacionMinutos = modelo.TiempoPreparacionMinutos,
            TiempoPosteriorMinutos = modelo.TiempoPosteriorMinutos,
            Sedes = modelo.Sedes.Where(sede => sede.Seleccionada)
                .Select(sede => new SedeAsignacionSolicitud
                {
                    SedeId = sede.SedeId,
                    PrecioEspecial = sede.PrecioEspecial
                }).ToArray()
        };

    private static ActualizarServicioSolicitud ActualizarSolicitud(ServicioFormularioViewModel modelo)
    {
        var crear = CrearSolicitud(modelo);
        return new ActualizarServicioSolicitud
        {
            Id = modelo.Id,
            CategoriaServicioId = crear.CategoriaServicioId,
            Nombre = crear.Nombre,
            Descripcion = crear.Descripcion,
            DuracionMinutos = crear.DuracionMinutos,
            Precio = crear.Precio,
            MontoAdelanto = crear.MontoAdelanto,
            Modalidad = crear.Modalidad,
            EsGrupal = crear.EsGrupal,
            CapacidadMaxima = crear.CapacidadMaxima,
            RequiereProfesional = crear.RequiereProfesional,
            RequiereRecurso = crear.RequiereRecurso,
            PermiteCancelacion = crear.PermiteCancelacion,
            PermiteReprogramacion = crear.PermiteReprogramacion,
            HorasLimiteCancelacion = crear.HorasLimiteCancelacion,
            TiempoPreparacionMinutos = crear.TiempoPreparacionMinutos,
            TiempoPosteriorMinutos = crear.TiempoPosteriorMinutos,
            Sedes = crear.Sedes
        };
    }

    private async Task<IActionResult> EjecutarAsync(
        Guid id,
        Func<Task<ResultadoOperacion>> operacion,
        string mensajeExito,
        CancellationToken cancellationToken)
    {
        var existente = await servicioService.ObtenerPorIdAsync(id, cancellationToken);
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
            logger.LogError(excepcion, "Error inesperado al modificar el servicio {ServicioId}.", id);
            TempData["Error"] = "No fue posible completar la operación. Intente nuevamente.";
        }
        return RedirectToAction(nameof(Index), new { organizacionId = existente.Valor.OrganizacionId });
    }
}
