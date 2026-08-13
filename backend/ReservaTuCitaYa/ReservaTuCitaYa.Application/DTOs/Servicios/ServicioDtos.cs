using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.DTOs.Servicios;

public sealed record ServicioFiltroDto(
    Guid OrganizacionId,
    string? Busqueda = null,
    Guid? CategoriaServicioId = null,
    ModalidadServicio? Modalidad = null,
    EstadoFiltro Estado = EstadoFiltro.Todos,
    int Pagina = 1,
    int TamanoPagina = 10);

public sealed record ServicioListaDto(
    Guid Id,
    Guid OrganizacionId,
    string Nombre,
    string Categoria,
    ModalidadServicio Modalidad,
    int DuracionMinutos,
    decimal Precio,
    decimal MontoAdelanto,
    bool EsGrupal,
    int CapacidadMaxima,
    int CantidadSedes,
    bool EstaActivo);

public sealed record ServicioSedeDetalleDto(
    Guid SedeId,
    string Sede,
    bool SedeActiva,
    decimal? PrecioEspecial,
    decimal PrecioAplicable);

public sealed record ServicioDetalleDto(
    Guid Id,
    Guid OrganizacionId,
    string Organizacion,
    Guid CategoriaServicioId,
    string Categoria,
    string Nombre,
    string? Descripcion,
    int DuracionMinutos,
    decimal Precio,
    decimal MontoAdelanto,
    ModalidadServicio Modalidad,
    bool EsGrupal,
    int CapacidadMaxima,
    bool RequiereProfesional,
    bool RequiereRecurso,
    bool PermiteCancelacion,
    bool PermiteReprogramacion,
    int HorasLimiteCancelacion,
    int TiempoPreparacionMinutos,
    int TiempoPosteriorMinutos,
    bool EstaActivo,
    DateTime FechaCreacion,
    DateTime? FechaModificacion,
    Guid? CreadoPorUsuarioId,
    Guid? ModificadoPorUsuarioId,
    IReadOnlyList<ServicioSedeDetalleDto> Sedes);

public sealed record SedeAsignacionDto(
    Guid SedeId,
    string Nombre,
    bool EstaActivo,
    bool EstaAsignada,
    decimal? PrecioEspecial);

public sealed class SedeAsignacionSolicitud
{
    public Guid SedeId { get; init; }
    public decimal? PrecioEspecial { get; init; }
}

public abstract class ServicioSolicitudBase
{
    public Guid CategoriaServicioId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public int DuracionMinutos { get; init; }
    public decimal Precio { get; init; }
    public decimal MontoAdelanto { get; init; }
    public ModalidadServicio Modalidad { get; init; }
    public bool EsGrupal { get; init; }
    public int CapacidadMaxima { get; init; } = 1;
    public bool RequiereProfesional { get; init; }
    public bool RequiereRecurso { get; init; }
    public bool PermiteCancelacion { get; init; }
    public bool PermiteReprogramacion { get; init; }
    public int HorasLimiteCancelacion { get; init; }
    public int TiempoPreparacionMinutos { get; init; }
    public int TiempoPosteriorMinutos { get; init; }
    public IReadOnlyList<SedeAsignacionSolicitud> Sedes { get; init; } = [];
}

public sealed class CrearServicioSolicitud : ServicioSolicitudBase
{
    public Guid OrganizacionId { get; init; }
}

public sealed class ActualizarServicioSolicitud : ServicioSolicitudBase
{
    public Guid Id { get; init; }
}
