using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.DTOs.Sedes
{
    public sealed record SedeFiltroDto(
        Guid OrganizacionId,
        string? Busqueda = null,
        EstadoFiltro Estado = EstadoFiltro.Todos);

    public sealed record SedeListaDto(
        Guid Id,
        Guid OrganizacionId,
        string Nombre,
        string Direccion,
        string? Telefono,
        string? Correo,
        string? Referencia,
        bool EstaActivo);

    public sealed record SedeDetalleDto(
        Guid Id,
        Guid OrganizacionId,
        string Organizacion,
        string Nombre,
        string Direccion,
        string? Telefono,
        string? Correo,
        string? Referencia,
        bool EstaActivo,
        DateTime FechaCreacion,
        DateTime? FechaModificacion);

    public sealed class CrearSedeSolicitud
    {
        public Guid OrganizacionId { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Direccion { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string? Correo { get; init; }
        public string? Referencia { get; init; }
    }

    public sealed class ActualizarSedeSolicitud
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Direccion { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string? Correo { get; init; }
        public string? Referencia { get; init; }
    }
}
