// Application/Services/RecursoService.cs
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Services;

public sealed class RecursoService(
    IRecursoRepository recursoRepository,
    ISedeRepository sedeRepository) : IRecursoService
{
    public const string CodigoDuplicado =
        "Ya existe un recurso con el mismo código en esta sede.";
    public const string SedeInvalida =
        "La sede no existe, fue eliminada o está inactiva.";
    public const string ServicioSedeInvalido =
        "Uno o más servicios no pertenecen a la organización o no se ofrecen en esta sede.";

    public async Task<ResultadoOperacion<PaginaResultado<RecursoListaDto>>> ListarAsync(
        RecursoFiltroDto filtro, CancellationToken cancellationToken = default) =>
        ResultadoOperacion<PaginaResultado<RecursoListaDto>>.Exito(
            await recursoRepository.ListarAsync(filtro, cancellationToken));

    public async Task<ResultadoOperacion<RecursoDetalleDto>> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var detalle = await recursoRepository.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null
            ? ResultadoOperacion<RecursoDetalleDto>.Fallo(
                "El recurso no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<RecursoDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearRecursoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);

        var sede = await sedeRepository.ObtenerParaModificarAsync(solicitud.SedeId, cancellationToken);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo)
            return ResultadoOperacion<Guid>.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        if (TieneServiciosDuplicados(solicitud.Servicios))
            return ResultadoOperacion<Guid>.Fallo("No se pueden repetir servicios en la solicitud.");

        if (!string.IsNullOrWhiteSpace(solicitud.Codigo) &&
            await recursoRepository.ExisteCodigoAsync(solicitud.SedeId, solicitud.Codigo.Trim(), cancellationToken: cancellationToken))
            return ResultadoOperacion<Guid>.Fallo(CodigoDuplicado, TipoErrorOperacion.Conflicto);

        if (solicitud.Servicios.Count > 0)
        {
            var servicioIds = solicitud.Servicios.Select(s => s.ServicioId).ToList();
            var servicios = await recursoRepository.ObtenerServiciosParaValidarAsync(
                solicitud.SedeId, servicioIds, cancellationToken);
            var errorServicios = ValidarServicios(sede.OrganizacionId, servicioIds, servicios);
            if (errorServicios is not null)
                return ResultadoOperacion<Guid>.Fallo(errorServicios, TipoErrorOperacion.Conflicto);
        }

        var recurso = new Recurso { OrganizacionId = sede.OrganizacionId, SedeId = solicitud.SedeId };
        AsignarCampos(recurso, solicitud);
        recursoRepository.Agregar(recurso);

        try
        {
            await recursoRepository.EjecutarEnTransaccionAsync(async ct =>
            {
                foreach (var s in solicitud.Servicios)
                    recursoRepository.AgregarRelacion(new ServicioRecurso
                    {
                        RecursoId = recurso.Id,
                        ServicioId = s.ServicioId,
                        EsObligatorio = s.EsObligatorio,
                        CantidadRequerida = s.CantidadRequerida <= 0 ? 1 : s.CantidadRequerida
                    });
                await recursoRepository.GuardarAsync(ct);
            }, cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<Guid>.Fallo(CodigoDuplicado, TipoErrorOperacion.Conflicto);
        }
        return ResultadoOperacion<Guid>.Exito(recurso.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarRecursoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(solicitud.Id, cancellationToken);
        if (ValidarRegistro(recurso) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);

        if (!string.IsNullOrWhiteSpace(solicitud.Codigo) &&
            await recursoRepository.ExisteCodigoAsync(
                recurso!.SedeId, solicitud.Codigo.Trim(), recurso.Id, cancellationToken))
            return ResultadoOperacion.Fallo(CodigoDuplicado, TipoErrorOperacion.Conflicto);

        AsignarCampos(recurso!, solicitud);
        recurso!.FechaModificacion = DateTime.UtcNow;
        try { await recursoRepository.GuardarAsync(cancellationToken); }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion.Fallo(CodigoDuplicado, TipoErrorOperacion.Conflicto);
        }
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id, bool estaActivo, CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(recurso) is { } estado) return estado;
        recurso!.EstaActivo = estaActivo;
        recurso.FechaModificacion = DateTime.UtcNow;
        await recursoRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(recurso) is { } estado) return estado;
        var relaciones = await recursoRepository.ObtenerRelacionesServicioAsync(id, cancellationToken);
        await recursoRepository.EjecutarEnTransaccionAsync(async ct =>
        {
            recurso!.EstaActivo = false;
            recurso.EstaEliminado = true;
            recurso.FechaModificacion = DateTime.UtcNow;
            foreach (var relacion in relaciones)
            {
                relacion.EstaActivo = false;
                relacion.EstaEliminado = true;
                relacion.FechaModificacion = DateTime.UtcNow;
            }
            await recursoRepository.GuardarAsync(ct);
        }, cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion<IReadOnlyList<RecursoServicioDto>>> ListarServiciosAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(recurso) is { } estado)
            return ResultadoOperacion<IReadOnlyList<RecursoServicioDto>>.Fallo(
                estado.Error!, estado.TipoError);
        return ResultadoOperacion<IReadOnlyList<RecursoServicioDto>>.Exito(
            await recursoRepository.ListarServiciosAsync(id, cancellationToken));
    }

    public async Task<ResultadoOperacion> ReemplazarServiciosAsync(
        Guid id, IReadOnlyList<AsignacionServicioRecurso> servicios,
        CancellationToken cancellationToken = default)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(recurso) is { } estado) return estado;
        if (TieneServiciosDuplicados(servicios))
            return ResultadoOperacion.Fallo("No se pueden repetir servicios en la solicitud.");

        var servicioIds = servicios.Select(s => s.ServicioId).ToList();
        if (servicioIds.Count > 0)
        {
            var entidadesServicio = await recursoRepository.ObtenerServiciosParaValidarAsync(
                recurso!.SedeId, servicioIds, cancellationToken);
            var errorServicios = ValidarServicios(recurso.OrganizacionId, servicioIds, entidadesServicio);
            if (errorServicios is not null)
                return ResultadoOperacion.Fallo(errorServicios, TipoErrorOperacion.Conflicto);
        }

        var relaciones = await recursoRepository.ObtenerRelacionesServicioAsync(id, cancellationToken);
        await recursoRepository.EjecutarEnTransaccionAsync(async ct =>
        {
            foreach (var relacion in relaciones)
            {
                var asignacion = servicios.SingleOrDefault(s => s.ServicioId == relacion.ServicioId);
                if (asignacion is not null)
                {
                    relacion.EstaActivo = true;
                    relacion.EstaEliminado = false;
                    relacion.EsObligatorio = asignacion.EsObligatorio;
                    relacion.CantidadRequerida = asignacion.CantidadRequerida <= 0 ? 1 : asignacion.CantidadRequerida;
                }
                else
                {
                    relacion.EstaActivo = false;
                    relacion.EstaEliminado = true;
                }
                relacion.FechaModificacion = DateTime.UtcNow;
            }
            foreach (var nueva in servicios.Where(s => relaciones.All(r => r.ServicioId != s.ServicioId)))
                recursoRepository.AgregarRelacion(new ServicioRecurso
                {
                    RecursoId = id,
                    ServicioId = nueva.ServicioId,
                    EsObligatorio = nueva.EsObligatorio,
                    CantidadRequerida = nueva.CantidadRequerida <= 0 ? 1 : nueva.CantidadRequerida
                });
            await recursoRepository.GuardarAsync(ct);
        }, cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private static string? ValidarServicios(
        Guid organizacionId, IReadOnlyCollection<Guid> ids, IReadOnlyList<Servicio> servicios)
    {
        if (servicios.Count != ids.Count) return ServicioSedeInvalido;
        return servicios.Any(s => s.OrganizacionId != organizacionId || s.EstaEliminado || !s.EstaActivo)
            ? ServicioSedeInvalido : null;
    }

    private static ResultadoOperacion? ValidarRegistro(Recurso? recurso) =>
        recurso is null || recurso.EstaEliminado
            ? ResultadoOperacion.Fallo("El recurso no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : null;

    private static bool TieneServiciosDuplicados(IReadOnlyList<AsignacionServicioRecurso> servicios) =>
        servicios.Select(s => s.ServicioId).Distinct().Count() != servicios.Count;

    private static string? Validar(GuardarRecursoSolicitud s)
    {
        if (string.IsNullOrWhiteSpace(s.Nombre)) return "El nombre del recurso es obligatorio.";
        if (s.Nombre.Trim().Length > 120) return "El nombre no puede superar 120 caracteres.";
        if (s.Codigo?.Trim().Length > 50) return "El código no puede superar 50 caracteres.";
        if (s.Descripcion?.Trim().Length > 500) return "La descripción no puede superar 500 caracteres.";
        if (string.IsNullOrWhiteSpace(s.TipoRecurso)) return "El tipo de recurso es obligatorio.";
        if (s.TipoRecurso.Trim().Length > 30) return "El tipo de recurso no puede superar 30 caracteres.";
        if (s.Capacidad <= 0) return "La capacidad debe ser mayor que cero.";
        if (s.UbicacionInterna?.Trim().Length > 150) return "La ubicación interna no puede superar 150 caracteres.";
        if (s.Observaciones?.Trim().Length > 500) return "Las observaciones no pueden superar 500 caracteres.";
        return null;
    }

    private static void AsignarCampos(Recurso r, GuardarRecursoSolicitud s)
    {
        r.Nombre = s.Nombre.Trim();
        r.Codigo = string.IsNullOrWhiteSpace(s.Codigo) ? null : s.Codigo.Trim();
        r.Descripcion = string.IsNullOrWhiteSpace(s.Descripcion) ? null : s.Descripcion.Trim();
        r.TipoRecurso = s.TipoRecurso.Trim();
        r.Capacidad = s.Capacidad;
        r.UbicacionInterna = string.IsNullOrWhiteSpace(s.UbicacionInterna) ? null : s.UbicacionInterna.Trim();
        r.Observaciones = string.IsNullOrWhiteSpace(s.Observaciones) ? null : s.Observaciones.Trim();
    }
}