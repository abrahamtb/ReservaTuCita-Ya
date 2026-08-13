using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Empleados;

public sealed record EmpleadoFiltroDto(
    Guid OrganizacionId,
    string? Busqueda = null,
    TipoDocumento? TipoDocumento = null,
    bool? EsProfesional = null,
    EstadoFiltro Estado = EstadoFiltro.Todos,
    Guid? SedeId = null,
    Guid? ServicioId = null,
    int Pagina = 1,
    int TamanoPagina = 10);

public sealed record EmpleadoListaDto(
    Guid Id,
    Guid OrganizacionId,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string? Correo,
    string? Telefono,
    string Cargo,
    string? Especialidad,
    bool EsProfesional,
    int CantidadSedes,
    int CantidadServicios,
    bool EstaActivo);

public sealed record EmpleadoSedeDto(Guid Id, Guid SedeId, string Nombre, bool EstaActivo);
public sealed record ProfesionalServicioDto(
    Guid Id, Guid ServicioId, string Nombre, bool EstaActivo);

public sealed record EmpleadoDetalleDto(
    Guid Id,
    Guid OrganizacionId,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string? Correo,
    string? Telefono,
    string? Direccion,
    DateOnly? FechaNacimiento,
    string Cargo,
    string? Especialidad,
    bool EsProfesional,
    string? NumeroColegiatura,
    string? Observaciones,
    bool EstaActivo,
    DateTime FechaCreacion,
    DateTime? FechaModificacion,
    Guid? CreadoPorUsuarioId,
    Guid? ModificadoPorUsuarioId,
    IReadOnlyList<EmpleadoSedeDto> Sedes,
    IReadOnlyList<ProfesionalServicioDto> Servicios);

public class GuardarEmpleadoSolicitud
{
    public TipoDocumento TipoDocumento { get; init; }
    public string NumeroDocumento { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public DateOnly? FechaNacimiento { get; init; }
    public string Cargo { get; init; } = string.Empty;
    public string? Especialidad { get; init; }
    public bool EsProfesional { get; init; }
    public string? NumeroColegiatura { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearEmpleadoSolicitud : GuardarEmpleadoSolicitud
{
    public Guid OrganizacionId { get; init; }
    public IReadOnlyList<Guid> SedeIds { get; init; } = [];
    public IReadOnlyList<Guid> ServicioIds { get; init; } = [];
}

public sealed class ActualizarEmpleadoSolicitud : GuardarEmpleadoSolicitud
{
    public Guid Id { get; init; }
}
