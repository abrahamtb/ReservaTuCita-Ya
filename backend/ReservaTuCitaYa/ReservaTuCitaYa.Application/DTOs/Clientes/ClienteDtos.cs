using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Clientes;

public sealed record ClienteFiltroDto(
    Guid OrganizacionId,
    string? Busqueda = null,
    TipoDocumento? TipoDocumento = null,
    EstadoFiltro Estado = EstadoFiltro.Todos,
    int Pagina = 1,
    int TamanoPagina = 10);

public sealed record ClienteListaDto(
    Guid Id,
    Guid OrganizacionId,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string? Correo,
    string? Telefono,
    bool EstaActivo);

public sealed record ClienteDetalleDto(
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
    string? Observaciones,
    bool EstaActivo,
    DateTime FechaCreacion,
    DateTime? FechaModificacion,
    Guid? CreadoPorUsuarioId,
    Guid? ModificadoPorUsuarioId);

public class GuardarClienteSolicitud
{
    public TipoDocumento TipoDocumento { get; init; }
    public string NumeroDocumento { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public DateOnly? FechaNacimiento { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearClienteSolicitud : GuardarClienteSolicitud
{
    public Guid OrganizacionId { get; init; }
}

public sealed class ActualizarClienteSolicitud : GuardarClienteSolicitud
{
    public Guid Id { get; init; }
}
