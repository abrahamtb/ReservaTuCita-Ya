using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.DTOs.Organizaciones
{
    public sealed record OrganizacionFiltroDto(
        string? Busqueda = null,
        EstadoFiltro Estado = EstadoFiltro.Todos,
        int Pagina = 1,
        int TamanoPagina = 10);

    public sealed record TipoOrganizacionOpcionDto(Guid Id, string Nombre);

    public sealed record OrganizacionListaDto(
        Guid Id,
        string NombreComercial,
        string? RazonSocial,
        string NumeroDocumento,
        string TipoOrganizacion,
        string? Telefono,
        string? Correo,
        bool EstaActivo);

    public sealed record OrganizacionDetalleDto(
        Guid Id,
        Guid TipoOrganizacionId,
        string TipoOrganizacion,
        string NombreComercial,
        string? RazonSocial,
        string NumeroDocumento,
        string? Telefono,
        string? Correo,
        string? DireccionPrincipal,
        string? LogoUrl,
        bool EstaActivo,
        DateTime FechaCreacion,
        DateTime? FechaModificacion,
        int CantidadSedesActivas);

    public sealed class CrearOrganizacionSolicitud
    {
        public Guid TipoOrganizacionId { get; init; }
        public string NombreComercial { get; init; } = string.Empty;
        public string? RazonSocial { get; init; }
        public string NumeroDocumento { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string? Correo { get; init; }
        public string? DireccionPrincipal { get; init; }
        public string? LogoUrl { get; init; }
    }

    public sealed class ActualizarOrganizacionSolicitud
    {
        public Guid Id { get; init; }
        public Guid TipoOrganizacionId { get; init; }
        public string NombreComercial { get; init; } = string.Empty;
        public string? RazonSocial { get; init; }
        public string NumeroDocumento { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string? Correo { get; init; }
        public string? DireccionPrincipal { get; init; }
        public string? LogoUrl { get; init; }
    }
}
