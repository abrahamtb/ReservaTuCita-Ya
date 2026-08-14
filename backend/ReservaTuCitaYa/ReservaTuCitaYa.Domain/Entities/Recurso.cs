using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class Recurso : BaseEntity
{
    public Guid OrganizacionId { get; set; }
    public Guid SedeId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public EstadoRecurso EstadoRecurso { get; set; }
    public string? UbicacionInterna { get; set; }
    public string? Observaciones { get; set; }

    public Organizacion Organizacion { get; set; } = null!;
    public Sede Sede { get; set; } = null!;
    public ICollection<ServicioRecurso> Servicios { get; set; } = new List<ServicioRecurso>();
    public ICollection<BloqueoRecurso> Bloqueos { get; set; } = new List<BloqueoRecurso>();
}