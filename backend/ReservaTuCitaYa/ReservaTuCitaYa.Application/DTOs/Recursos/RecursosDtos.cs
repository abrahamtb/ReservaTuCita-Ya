// Application/DTOs/Recursos/RecursoDtos.cs
using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Application.DTOs.Recursos;

public sealed record RecursoFiltroDto(
    Guid SedeId,
    string? Busqueda,
    string? TipoRecurso,
    EstadoFiltro Estado,
    Guid? ServicioId,
    int Pagina,
    int TamanoPagina);

public sealed record RecursoListaDto(
    Guid Id, Guid SedeId, string Nombre, string? Codigo, string TipoRecurso,
    int Capacidad, string? UbicacionInterna, int ServiciosCount, bool EstaActivo);

public sealed record RecursoDetalleDto(
    Guid Id, Guid OrganizacionId, Guid SedeId, string SedeNombre,
    string Nombre, string? Codigo, string? Descripcion, string TipoRecurso,
    int Capacidad, string? UbicacionInterna, string? Observaciones,
    bool EstaActivo, DateTime FechaCreacion, DateTime? FechaModificacion,
    IReadOnlyList<RecursoServicioDto> Servicios);

public sealed record RecursoServicioDto(
    Guid Id, Guid ServicioId, string ServicioNombre, bool EsObligatorio,
    int CantidadRequerida, bool EstaActivo);

public sealed record AsignacionServicioRecurso(
    Guid ServicioId, bool EsObligatorio, int CantidadRequerida);

public abstract class GuardarRecursoSolicitud
{
    public string Nombre { get; init; } = string.Empty;
    public string? Codigo { get; init; }
    public string? Descripcion { get; init; }
    public string TipoRecurso { get; init; } = string.Empty;
    public int Capacidad { get; init; }
    public string? UbicacionInterna { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearRecursoSolicitud : GuardarRecursoSolicitud
{
    public Guid SedeId { get; init; }
    public IReadOnlyList<AsignacionServicioRecurso> Servicios { get; init; } = [];
}

public sealed class ActualizarRecursoSolicitud : GuardarRecursoSolicitud
{
    public Guid Id { get; init; }
}