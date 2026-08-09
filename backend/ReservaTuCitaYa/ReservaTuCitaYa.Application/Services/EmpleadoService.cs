using System.Net.Mail;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services;

public sealed class EmpleadoService(
    IEmpleadoRepository empleadoRepository,
    IOrganizacionRepository organizacionRepository) : IEmpleadoService
{
    public const string DocumentoDuplicado =
        "Ya existe un empleado con el mismo tipo y número de documento en esta organización.";
    public const string SedeOrganizacionInvalida =
        "Una o más sedes no pertenecen a la organización del empleado.";
    public const string ServicioOrganizacionInvalida =
        "Uno o más servicios no pertenecen a la organización del empleado.";
    public const string EmpleadoNoProfesional =
        "El empleado debe estar marcado como profesional para asignarle servicios.";
    public const string ProfesionalConServicios =
        "No se puede quitar la condición de profesional mientras tenga servicios asignados.";

    public async Task<ResultadoOperacion<PaginaResultado<EmpleadoListaDto>>> ListarAsync(
        EmpleadoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        if (await ValidarOrganizacionAsync(filtro.OrganizacionId, cancellationToken) is { } errorOrg)
            return ResultadoOperacion<PaginaResultado<EmpleadoListaDto>>.Fallo(
                errorOrg, TipoErrorOperacion.NoEncontrado);
        if (filtro.TipoDocumento.HasValue && !TipoDocumentoValido(filtro.TipoDocumento.Value))
            return ResultadoOperacion<PaginaResultado<EmpleadoListaDto>>.Fallo(
                "El tipo de documento no es válido.");
        return ResultadoOperacion<PaginaResultado<EmpleadoListaDto>>.Exito(
            await empleadoRepository.ListarAsync(filtro, cancellationToken));
    }

    public async Task<ResultadoOperacion<EmpleadoDetalleDto>> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var detalle = await empleadoRepository.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null
            ? ResultadoOperacion<EmpleadoDetalleDto>.Fallo(
                "El empleado no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<EmpleadoDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearEmpleadoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion<Guid>.Fallo(error);
        if (await ValidarOrganizacionAsync(solicitud.OrganizacionId, cancellationToken) is { } errorOrg)
            return ResultadoOperacion<Guid>.Fallo(errorOrg, TipoErrorOperacion.NoEncontrado);
        if (TieneDuplicados(solicitud.SedeIds))
            return ResultadoOperacion<Guid>.Fallo("No se pueden repetir sedes en la solicitud.");
        if (TieneDuplicados(solicitud.ServicioIds))
            return ResultadoOperacion<Guid>.Fallo("No se pueden repetir servicios en la solicitud.");
        if (!solicitud.EsProfesional && solicitud.ServicioIds.Count > 0)
            return ResultadoOperacion<Guid>.Fallo(
                EmpleadoNoProfesional, TipoErrorOperacion.Conflicto);

        var sedes = await empleadoRepository.ObtenerSedesParaValidarAsync(
            solicitud.SedeIds, cancellationToken);
        var errorSedes = ValidarSedes(solicitud.OrganizacionId, solicitud.SedeIds, sedes);
        if (errorSedes is not null)
            return ResultadoOperacion<Guid>.Fallo(errorSedes,
                errorSedes == SedeOrganizacionInvalida
                    ? TipoErrorOperacion.Conflicto : TipoErrorOperacion.Validacion);

        var servicios = await empleadoRepository.ObtenerServiciosParaValidarAsync(
            solicitud.ServicioIds, cancellationToken);
        var errorServicios = ValidarServicios(
            solicitud.OrganizacionId, solicitud.ServicioIds, servicios);
        if (errorServicios is not null)
            return ResultadoOperacion<Guid>.Fallo(errorServicios,
                errorServicios == ServicioOrganizacionInvalida
                    ? TipoErrorOperacion.Conflicto : TipoErrorOperacion.Validacion);

        var documento = solicitud.NumeroDocumento.Trim();
        if (await empleadoRepository.ExisteDocumentoAsync(
                solicitud.OrganizacionId, solicitud.TipoDocumento, documento,
                cancellationToken: cancellationToken))
            return ResultadoOperacion<Guid>.Fallo(
                DocumentoDuplicado, TipoErrorOperacion.Conflicto);

        var empleado = new Empleado { OrganizacionId = solicitud.OrganizacionId };
        AsignarCampos(empleado, solicitud);
        empleadoRepository.Agregar(empleado);

        try
        {
            await empleadoRepository.EjecutarEnTransaccionAsync(async ct =>
            {
                foreach (var sedeId in solicitud.SedeIds)
                    empleadoRepository.AgregarRelacion(new EmpleadoSede
                        { EmpleadoId = empleado.Id, SedeId = sedeId });
                foreach (var servicioId in solicitud.ServicioIds)
                    empleadoRepository.AgregarRelacion(new ProfesionalServicio
                        { EmpleadoId = empleado.Id, ServicioId = servicioId });
                await empleadoRepository.GuardarAsync(ct);
            }, cancellationToken);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<Guid>.Fallo(
                DocumentoDuplicado, TipoErrorOperacion.Conflicto);
        }
        return ResultadoOperacion<Guid>.Exito(empleado.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        ActualizarEmpleadoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(
            solicitud.Id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado) return estado;
        var error = Validar(solicitud);
        if (error is not null) return ResultadoOperacion.Fallo(error);
        if (await ValidarOrganizacionAsync(empleado!.OrganizacionId, cancellationToken) is { } errorOrg)
            return ResultadoOperacion.Fallo(errorOrg, TipoErrorOperacion.NoEncontrado);
        if (empleado.EsProfesional && !solicitud.EsProfesional &&
            await empleadoRepository.TieneServiciosActivosAsync(empleado.Id, cancellationToken))
            return ResultadoOperacion.Fallo(
                ProfesionalConServicios, TipoErrorOperacion.Conflicto);

        var documento = solicitud.NumeroDocumento.Trim();
        if (await empleadoRepository.ExisteDocumentoAsync(
                empleado.OrganizacionId, solicitud.TipoDocumento, documento,
                empleado.Id, cancellationToken))
            return ResultadoOperacion.Fallo(
                DocumentoDuplicado, TipoErrorOperacion.Conflicto);

        AsignarCampos(empleado, solicitud);
        empleado.FechaModificacion = DateTime.UtcNow;
        try { await empleadoRepository.GuardarAsync(cancellationToken); }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion.Fallo(
                DocumentoDuplicado, TipoErrorOperacion.Conflicto);
        }
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id, bool estaActivo, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado) return estado;
        empleado!.EstaActivo = estaActivo;
        empleado.FechaModificacion = DateTime.UtcNow;
        await empleadoRepository.GuardarAsync(cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado) return estado;
        var sedes = await empleadoRepository.ObtenerRelacionesSedeAsync(id, cancellationToken);
        var servicios = await empleadoRepository.ObtenerRelacionesServicioAsync(id, cancellationToken);
        await empleadoRepository.EjecutarEnTransaccionAsync(async ct =>
        {
            empleado!.EstaActivo = false;
            empleado.EstaEliminado = true;
            empleado.FechaModificacion = DateTime.UtcNow;
            foreach (var relacion in sedes) EliminarRelacion(relacion);
            foreach (var relacion in servicios) EliminarRelacion(relacion);
            await empleadoRepository.GuardarAsync(ct);
        }, cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion<IReadOnlyList<EmpleadoSedeDto>>> ListarSedesAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado)
            return ResultadoOperacion<IReadOnlyList<EmpleadoSedeDto>>.Fallo(
                estado.Error!, estado.TipoError);
        return ResultadoOperacion<IReadOnlyList<EmpleadoSedeDto>>.Exito(
            await empleadoRepository.ListarSedesAsync(id, cancellationToken));
    }

    public async Task<ResultadoOperacion> ReemplazarSedesAsync(
        Guid id, IReadOnlyList<Guid> sedeIds, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado) return estado;
        if (TieneDuplicados(sedeIds))
            return ResultadoOperacion.Fallo("No se pueden repetir sedes en la solicitud.");
        var sedes = await empleadoRepository.ObtenerSedesParaValidarAsync(sedeIds, cancellationToken);
        var error = ValidarSedes(empleado!.OrganizacionId, sedeIds, sedes);
        if (error is not null)
            return ResultadoOperacion.Fallo(error,
                error == SedeOrganizacionInvalida
                    ? TipoErrorOperacion.Conflicto : TipoErrorOperacion.Validacion);
        var relaciones = await empleadoRepository.ObtenerRelacionesSedeAsync(id, cancellationToken);
        await empleadoRepository.EjecutarEnTransaccionAsync(async ct =>
        {
            SincronizarSedes(id, sedeIds, relaciones);
            await empleadoRepository.GuardarAsync(ct);
        }, cancellationToken);
        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion<IReadOnlyList<ProfesionalServicioDto>>> ListarServiciosAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado)
            return ResultadoOperacion<IReadOnlyList<ProfesionalServicioDto>>.Fallo(
                estado.Error!, estado.TipoError);
        if (!empleado!.EsProfesional)
            return ResultadoOperacion<IReadOnlyList<ProfesionalServicioDto>>.Fallo(
                EmpleadoNoProfesional, TipoErrorOperacion.Conflicto);
        return ResultadoOperacion<IReadOnlyList<ProfesionalServicioDto>>.Exito(
            await empleadoRepository.ListarServiciosAsync(id, cancellationToken));
    }

    public async Task<ResultadoOperacion> ReemplazarServiciosAsync(
        Guid id, IReadOnlyList<Guid> servicioIds, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(id, cancellationToken);
        if (ValidarRegistro(empleado) is { } estado) return estado;
        if (!empleado!.EsProfesional)
            return ResultadoOperacion.Fallo(EmpleadoNoProfesional, TipoErrorOperacion.Conflicto);
        if (TieneDuplicados(servicioIds))
            return ResultadoOperacion.Fallo("No se pueden repetir servicios en la solicitud.");
        var servicios = await empleadoRepository.ObtenerServiciosParaValidarAsync(
            servicioIds, cancellationToken);
        var error = ValidarServicios(empleado.OrganizacionId, servicioIds, servicios);
        if (error is not null)
            return ResultadoOperacion.Fallo(error,
                error == ServicioOrganizacionInvalida
                    ? TipoErrorOperacion.Conflicto : TipoErrorOperacion.Validacion);
        var relaciones = await empleadoRepository.ObtenerRelacionesServicioAsync(id, cancellationToken);
        await empleadoRepository.EjecutarEnTransaccionAsync(async ct =>
        {
            SincronizarServicios(id, servicioIds, relaciones);
            await empleadoRepository.GuardarAsync(ct);
        }, cancellationToken);
        return ResultadoOperacion.Exito();
    }

    private void SincronizarSedes(
        Guid empleadoId, IReadOnlyList<Guid> ids, IReadOnlyList<EmpleadoSede> relaciones)
    {
        foreach (var relacion in relaciones)
        {
            if (ids.Contains(relacion.SedeId)) RestaurarRelacion(relacion);
            else EliminarRelacion(relacion);
        }
        foreach (var id in ids.Where(id => relaciones.All(r => r.SedeId != id)))
            empleadoRepository.AgregarRelacion(new EmpleadoSede
                { EmpleadoId = empleadoId, SedeId = id });
    }

    private void SincronizarServicios(
        Guid empleadoId, IReadOnlyList<Guid> ids, IReadOnlyList<ProfesionalServicio> relaciones)
    {
        foreach (var relacion in relaciones)
        {
            if (ids.Contains(relacion.ServicioId)) RestaurarRelacion(relacion);
            else EliminarRelacion(relacion);
        }
        foreach (var id in ids.Where(id => relaciones.All(r => r.ServicioId != id)))
            empleadoRepository.AgregarRelacion(new ProfesionalServicio
                { EmpleadoId = empleadoId, ServicioId = id });
    }

    private static void RestaurarRelacion(Domain.Common.BaseEntity relacion)
    {
        relacion.EstaActivo = true;
        relacion.EstaEliminado = false;
        relacion.FechaModificacion = DateTime.UtcNow;
    }

    private static void EliminarRelacion(Domain.Common.BaseEntity relacion)
    {
        relacion.EstaActivo = false;
        relacion.EstaEliminado = true;
        relacion.FechaModificacion = DateTime.UtcNow;
    }

    private async Task<string?> ValidarOrganizacionAsync(Guid id, CancellationToken ct)
    {
        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(id, ct);
        return organizacion is null || organizacion.EstaEliminado
            ? "La organización no existe o fue eliminada." : null;
    }

    private static string? ValidarSedes(
        Guid organizacionId, IReadOnlyCollection<Guid> ids, IReadOnlyList<Sede> sedes)
    {
        if (sedes.Any(s => s.OrganizacionId != organizacionId)) return SedeOrganizacionInvalida;
        return sedes.Count != ids.Count || sedes.Any(s => s.EstaEliminado || !s.EstaActivo)
            ? "Una o más sedes no existen, fueron eliminadas o están inactivas." : null;
    }

    private static string? ValidarServicios(
        Guid organizacionId, IReadOnlyCollection<Guid> ids, IReadOnlyList<Servicio> servicios)
    {
        if (servicios.Any(s => s.OrganizacionId != organizacionId))
            return ServicioOrganizacionInvalida;
        return servicios.Count != ids.Count || servicios.Any(s => s.EstaEliminado || !s.EstaActivo)
            ? "Uno o más servicios no existen, fueron eliminados o están inactivos." : null;
    }

    private static ResultadoOperacion? ValidarRegistro(Empleado? empleado) =>
        empleado is null || empleado.EstaEliminado
            ? ResultadoOperacion.Fallo(
                "El empleado no existe o fue eliminado.", TipoErrorOperacion.NoEncontrado)
            : null;

    private static string? Validar(GuardarEmpleadoSolicitud s)
    {
        if (!TipoDocumentoValido(s.TipoDocumento)) return "El tipo de documento es obligatorio y debe ser válido.";
        if (string.IsNullOrWhiteSpace(s.NumeroDocumento)) return "El número de documento es obligatorio.";
        if (s.NumeroDocumento.Trim().Length > 20) return "El número de documento no puede superar 20 caracteres.";
        if (string.IsNullOrWhiteSpace(s.Nombres)) return "Los nombres son obligatorios.";
        if (s.Nombres.Trim().Length > 100) return "Los nombres no pueden superar 100 caracteres.";
        if (string.IsNullOrWhiteSpace(s.Apellidos)) return "Los apellidos son obligatorios.";
        if (s.Apellidos.Trim().Length > 100) return "Los apellidos no pueden superar 100 caracteres.";
        if (string.IsNullOrWhiteSpace(s.Cargo)) return "El cargo es obligatorio.";
        if (s.Cargo.Trim().Length > 100) return "El cargo no puede superar 100 caracteres.";
        if (s.Correo?.Trim().Length > 150) return "El correo no puede superar 150 caracteres.";
        if (!string.IsNullOrWhiteSpace(s.Correo) && !MailAddress.TryCreate(s.Correo.Trim(), out _)) return "El correo no tiene un formato válido.";
        if (s.Telefono?.Trim().Length > 30) return "El teléfono no puede superar 30 caracteres.";
        if (s.Direccion?.Trim().Length > 250) return "La dirección no puede superar 250 caracteres.";
        if (s.FechaNacimiento > DateOnly.FromDateTime(DateTime.UtcNow)) return "La fecha de nacimiento no puede ser futura.";
        if (s.Especialidad?.Trim().Length > 150) return "La especialidad no puede superar 150 caracteres.";
        if (s.NumeroColegiatura?.Trim().Length > 50) return "El número de colegiatura no puede superar 50 caracteres.";
        if (s.Observaciones?.Trim().Length > 500) return "Las observaciones no pueden superar 500 caracteres.";
        return null;
    }

    private static void AsignarCampos(Empleado e, GuardarEmpleadoSolicitud s)
    {
        e.TipoDocumento = s.TipoDocumento;
        e.NumeroDocumento = s.NumeroDocumento.Trim();
        e.Nombres = s.Nombres.Trim();
        e.Apellidos = s.Apellidos.Trim();
        e.Correo = LimpiarOpcional(s.Correo);
        e.Telefono = LimpiarOpcional(s.Telefono);
        e.Direccion = LimpiarOpcional(s.Direccion);
        e.FechaNacimiento = s.FechaNacimiento;
        e.Cargo = s.Cargo.Trim();
        e.Especialidad = LimpiarOpcional(s.Especialidad);
        e.EsProfesional = s.EsProfesional;
        e.NumeroColegiatura = LimpiarOpcional(s.NumeroColegiatura);
        e.Observaciones = LimpiarOpcional(s.Observaciones);
    }

    private static bool TieneDuplicados(IReadOnlyCollection<Guid> ids) =>
        ids.Count != ids.Distinct().Count();
    private static bool TipoDocumentoValido(TipoDocumento tipo) =>
        tipo != TipoDocumento.NoDefinido && Enum.IsDefined(tipo);
    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
