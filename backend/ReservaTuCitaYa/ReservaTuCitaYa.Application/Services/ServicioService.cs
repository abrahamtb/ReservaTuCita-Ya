using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services;

public sealed class ServicioService(
    IServicioRepository servicioRepository,
    ICategoriaServicioRepository categoriaRepository,
    IOrganizacionRepository organizacionRepository) : IServicioService
{
    public async Task<ResultadoOperacion<PaginaResultado<ServicioListaDto>>> ListarAsync(
        ServicioFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(
            filtro.OrganizacionId, cancellationToken);
        if (organizacion is null || organizacion.EstaEliminado)
            return ResultadoOperacion<PaginaResultado<ServicioListaDto>>.Fallo(
                "La organización no existe o fue eliminada.", TipoErrorOperacion.NoEncontrado);

        return ResultadoOperacion<PaginaResultado<ServicioListaDto>>.Exito(
            await servicioRepository.ListarAsync(filtro, cancellationToken));
    }

    public async Task<ResultadoOperacion<ServicioDetalleDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var detalle = await servicioRepository.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null
            ? ResultadoOperacion<ServicioDetalleDto>.Fallo(
                "El servicio no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<ServicioDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<IReadOnlyList<SedeAsignacionDto>>> ObtenerSedesAsignadasAsync(
        Guid organizacionId,
        Guid? servicioId = null,
        CancellationToken cancellationToken = default)
    {
        var errorOrganizacion = await ValidarOrganizacionActivaAsync(
            organizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return ResultadoOperacion<IReadOnlyList<SedeAsignacionDto>>.Fallo(errorOrganizacion);

        return ResultadoOperacion<IReadOnlyList<SedeAsignacionDto>>.Exito(
            await servicioRepository.ListarSedesParaAsignarAsync(
                organizacionId, servicioId, cancellationToken));
    }

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var error = ValidarSolicitud(solicitud);
        if (error is not null)
            return ResultadoOperacion<Guid>.Fallo(error);

        var errorContexto = await ValidarContextoAsync(
            solicitud.OrganizacionId, solicitud.CategoriaServicioId, cancellationToken);
        if (errorContexto is not null)
            return ResultadoOperacion<Guid>.Fallo(errorContexto);

        var nombre = solicitud.Nombre.Trim();
        if (await servicioRepository.ExisteNombreActivoAsync(
                solicitud.OrganizacionId, nombre, cancellationToken: cancellationToken))
        {
            return ResultadoOperacion<Guid>.Fallo(
                "Ya existe un servicio activo con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        var errorSedes = await ValidarSedesAsync(
            solicitud.OrganizacionId, solicitud.Sedes, cancellationToken);
        if (errorSedes is not null)
            return ResultadoOperacion<Guid>.Fallo(errorSedes);

        var servicio = CrearEntidad(solicitud.OrganizacionId, solicitud);
        try
        {
            await servicioRepository.EjecutarEnTransaccionAsync(async token =>
            {
                servicioRepository.Agregar(servicio);
                await SincronizarSedesAsync(servicio.Id, solicitud.Sedes, token);
                await servicioRepository.GuardarAsync(token);
            }, cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<Guid>.Fallo(
                "El nombre del servicio o alguna sede asignada ya está registrada.",
                TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion<Guid>.Exito(servicio.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var servicio = await servicioRepository.ObtenerParaModificarAsync(
            solicitud.Id, cancellationToken);
        var estado = ValidarRegistro(servicio);
        if (estado is not null)
            return estado;

        var error = ValidarSolicitud(solicitud);
        if (error is not null)
            return ResultadoOperacion.Fallo(error);

        var errorContexto = await ValidarContextoAsync(
            servicio!.OrganizacionId, solicitud.CategoriaServicioId, cancellationToken);
        if (errorContexto is not null)
            return ResultadoOperacion.Fallo(errorContexto);

        var nombre = solicitud.Nombre.Trim();
        if (servicio.EstaActivo && await servicioRepository.ExisteNombreActivoAsync(
                servicio.OrganizacionId, nombre, servicio.Id, cancellationToken))
        {
            return ResultadoOperacion.Fallo(
                "Ya existe un servicio activo con ese nombre en la organización.",
                TipoErrorOperacion.Conflicto);
        }

        var errorSedes = await ValidarSedesAsync(
            servicio.OrganizacionId, solicitud.Sedes, cancellationToken);
        if (errorSedes is not null)
            return ResultadoOperacion.Fallo(errorSedes);

        AplicarCambios(servicio, solicitud);
        try
        {
            await servicioRepository.EjecutarEnTransaccionAsync(async token =>
            {
                await SincronizarSedesAsync(servicio.Id, solicitud.Sedes, token);
                await servicioRepository.GuardarAsync(token);
            }, cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion.Fallo(
                "El nombre del servicio o alguna sede asignada ya está registrada.",
                TipoErrorOperacion.Conflicto);
        }

        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var servicio = await servicioRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(servicio);
        if (estado is not null)
            return estado;

        if (!servicio!.EstaActivo)
        {
            var errorContexto = await ValidarContextoAsync(
                servicio.OrganizacionId, servicio.CategoriaServicioId, cancellationToken);
            if (errorContexto is not null)
                return ResultadoOperacion.Fallo(errorContexto);

            if (await servicioRepository.ExisteNombreActivoAsync(
                    servicio.OrganizacionId, servicio.Nombre, servicio.Id, cancellationToken))
            {
                return ResultadoOperacion.Fallo(
                    "No se puede activar el servicio porque ya existe otro activo con el mismo nombre.",
                    TipoErrorOperacion.Conflicto);
            }
        }

        servicio.EstaActivo = !servicio.EstaActivo;
        servicio.FechaModificacion = DateTime.UtcNow;
        await servicioRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var servicio = await servicioRepository.ObtenerParaModificarAsync(id, cancellationToken);
        var estado = ValidarRegistro(servicio);
        if (estado is not null)
            return estado;

        servicio!.EstaActivo = false;
        servicio.EstaEliminado = true;
        servicio.FechaModificacion = DateTime.UtcNow;
        await servicioRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private async Task<string?> ValidarContextoAsync(
        Guid organizacionId,
        Guid categoriaId,
        CancellationToken cancellationToken)
    {
        var errorOrganizacion = await ValidarOrganizacionActivaAsync(
            organizacionId, cancellationToken);
        if (errorOrganizacion is not null)
            return errorOrganizacion;

        var categoria = await categoriaRepository.ObtenerParaModificarAsync(
            categoriaId, cancellationToken);
        if (categoria is null || categoria.EstaEliminado)
            return "La categoría no existe o fue eliminada.";
        if (categoria.OrganizacionId != organizacionId)
            return "La categoría seleccionada pertenece a otra organización.";
        return categoria.EstaActivo
            ? null
            : "La categoría está inactiva y no admite nuevos servicios.";
    }

    private async Task<string?> ValidarOrganizacionActivaAsync(
        Guid organizacionId,
        CancellationToken cancellationToken)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(
            organizacionId, cancellationToken);
        if (organizacion is null || organizacion.EstaEliminado)
            return "La organización no existe o fue eliminada.";
        return organizacion.EstaActivo
            ? null
            : "La organización está inactiva y no admite servicios.";
    }

    private async Task<string?> ValidarSedesAsync(
        Guid organizacionId,
        IReadOnlyList<SedeAsignacionSolicitud> solicitudes,
        CancellationToken cancellationToken)
    {
        if (solicitudes.Any(sede => sede.SedeId == Guid.Empty))
            return "La selección de sedes contiene un identificador inválido.";
        if (solicitudes.GroupBy(sede => sede.SedeId).Any(grupo => grupo.Count() > 1))
            return "Una sede no puede asignarse más de una vez al mismo servicio.";
        if (solicitudes.Any(sede => sede.PrecioEspecial < 0))
            return "El precio especial no puede ser negativo.";
        if (solicitudes.Count == 0)
            return null;

        var ids = solicitudes.Select(sede => sede.SedeId).ToArray();
        var sedes = await servicioRepository.ObtenerSedesParaValidarAsync(ids, cancellationToken);
        if (sedes.Count != ids.Length)
            return "Una o más sedes seleccionadas no existen.";
        if (sedes.Any(sede => sede.OrganizacionId != organizacionId))
            return "No se pueden asignar sedes de otra organización.";
        if (sedes.Any(sede => sede.EstaEliminado || !sede.EstaActivo))
            return "No se pueden asignar sedes inactivas o eliminadas.";
        return null;
    }

    private async Task SincronizarSedesAsync(
        Guid servicioId,
        IReadOnlyList<SedeAsignacionSolicitud> solicitudes,
        CancellationToken cancellationToken)
    {
        var seleccionadas = solicitudes.ToDictionary(sede => sede.SedeId);
        var existentes = await servicioRepository.ObtenerRelacionesSedeAsync(
            servicioId, cancellationToken);
        var procesadas = new HashSet<Guid>();

        foreach (var relacion in existentes)
        {
            if (seleccionadas.TryGetValue(relacion.SedeId, out var seleccion) &&
                procesadas.Add(relacion.SedeId))
            {
                relacion.PrecioEspecial = seleccion.PrecioEspecial;
                relacion.EstaActivo = true;
                relacion.EstaEliminado = false;
                relacion.FechaModificacion = DateTime.UtcNow;
            }
            else if (relacion.EstaActivo && !relacion.EstaEliminado)
            {
                relacion.EstaActivo = false;
                relacion.EstaEliminado = true;
                relacion.FechaModificacion = DateTime.UtcNow;
            }
        }

        foreach (var seleccion in solicitudes.Where(sede => !procesadas.Contains(sede.SedeId)))
        {
            servicioRepository.AgregarRelacion(new ServicioSede
            {
                ServicioId = servicioId,
                SedeId = seleccion.SedeId,
                PrecioEspecial = seleccion.PrecioEspecial
            });
        }
    }

    private static Servicio CrearEntidad(Guid organizacionId, ServicioSolicitudBase solicitud)
    {
        var servicio = new Servicio { OrganizacionId = organizacionId };
        AplicarCambios(servicio, solicitud);
        servicio.FechaModificacion = null;
        return servicio;
    }

    private static void AplicarCambios(Servicio servicio, ServicioSolicitudBase solicitud)
    {
        servicio.CategoriaServicioId = solicitud.CategoriaServicioId;
        servicio.Nombre = solicitud.Nombre.Trim();
        servicio.Descripcion = LimpiarOpcional(solicitud.Descripcion);
        servicio.DuracionMinutos = solicitud.DuracionMinutos;
        servicio.Precio = solicitud.Precio;
        servicio.MontoAdelanto = solicitud.MontoAdelanto;
        servicio.Modalidad = solicitud.Modalidad;
        servicio.EsGrupal = solicitud.EsGrupal;
        servicio.CapacidadMaxima = solicitud.CapacidadMaxima;
        servicio.RequiereProfesional = solicitud.RequiereProfesional;
        servicio.RequiereRecurso = solicitud.RequiereRecurso;
        servicio.PermiteCancelacion = solicitud.PermiteCancelacion;
        servicio.PermiteReprogramacion = solicitud.PermiteReprogramacion;
        servicio.HorasLimiteCancelacion = solicitud.HorasLimiteCancelacion;
        servicio.TiempoPreparacionMinutos = solicitud.TiempoPreparacionMinutos;
        servicio.TiempoPosteriorMinutos = solicitud.TiempoPosteriorMinutos;
        servicio.FechaModificacion = DateTime.UtcNow;
    }

    private static ResultadoOperacion? ValidarRegistro(Servicio? servicio)
    {
        if (servicio is null)
            return ResultadoOperacion.Fallo("El servicio no existe.", TipoErrorOperacion.NoEncontrado);
        return servicio.EstaEliminado
            ? ResultadoOperacion.Fallo(
                "El servicio fue eliminado y no admite operaciones.", TipoErrorOperacion.NoEncontrado)
            : null;
    }

    private static string? ValidarSolicitud(ServicioSolicitudBase solicitud)
    {
        if (solicitud.CategoriaServicioId == Guid.Empty)
            return "La categoría es obligatoria.";
        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
            return "El nombre del servicio es obligatorio.";
        if (solicitud.Nombre.Trim().Length > 150)
            return "El nombre del servicio no puede superar 150 caracteres.";
        if (solicitud.Descripcion?.Trim().Length > 1000)
            return "La descripción no puede superar 1000 caracteres.";
        if (solicitud.DuracionMinutos <= 0)
            return "La duración debe ser mayor que cero.";
        if (solicitud.Precio < 0)
            return "El precio no puede ser negativo.";
        if (solicitud.MontoAdelanto < 0)
            return "El adelanto no puede ser negativo.";
        if (solicitud.MontoAdelanto > solicitud.Precio)
            return "El adelanto no puede superar el precio del servicio.";
        if (!Enum.IsDefined(solicitud.Modalidad) || solicitud.Modalidad == ModalidadServicio.NoDefinido)
            return "Seleccione una modalidad válida.";
        if (solicitud.CapacidadMaxima <= 0)
            return "La capacidad máxima debe ser mayor que cero.";
        if (!solicitud.EsGrupal && solicitud.CapacidadMaxima != 1)
            return "Un servicio individual debe tener capacidad máxima igual a uno.";
        if (solicitud.HorasLimiteCancelacion < 0)
            return "Las horas límite de cancelación no pueden ser negativas.";
        if (solicitud.TiempoPreparacionMinutos < 0)
            return "El tiempo de preparación no puede ser negativo.";
        if (solicitud.TiempoPosteriorMinutos < 0)
            return "El tiempo posterior no puede ser negativo.";
        return null;
    }

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
