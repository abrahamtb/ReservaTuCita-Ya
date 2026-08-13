using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.DTOs.CategoriasServicio;

public sealed record CategoriaServicioFiltroDto(
    Guid OrganizacionId,
    string? Busqueda = null,
    EstadoFiltro Estado = EstadoFiltro.Todos,
    int Pagina = 1,
    int TamanoPagina = 10);

public sealed record CategoriaServicioListaDto(
    Guid Id,
    Guid OrganizacionId,
    string Organizacion,
    string Nombre,
    string? Descripcion,
    int CantidadServicios,
    bool EstaActivo);

public sealed record CategoriaServicioDetalleDto(
    Guid Id,
    Guid OrganizacionId,
    string Organizacion,
    string Nombre,
    string? Descripcion,
    bool EstaActivo,
    DateTime FechaCreacion,
    DateTime? FechaModificacion,
    Guid? CreadoPorUsuarioId,
    Guid? ModificadoPorUsuarioId,
    int CantidadServicios,
    int CantidadServiciosActivos);

public sealed record CategoriaServicioOpcionDto(Guid Id, string Nombre);

public sealed class CrearCategoriaServicioSolicitud
{
    public Guid OrganizacionId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}

public sealed class ActualizarCategoriaServicioSolicitud
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}
