// Domain/Entities/Reserva.cs
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Domain.Entities;

public sealed class Reserva : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public Guid OrganizacionId { get; set; }
    public Guid SedeId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ServicioId { get; set; }
    public Guid? ProfesionalId { get; set; }
    public Guid? RecursoId { get; set; }

    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFinServicio { get; set; }
    public TimeOnly HoraInicioOcupacion { get; set; }
    public TimeOnly HoraFinOcupacion { get; set; }

    // Snapshot del servicio al momento de reservar
    public int DuracionMinutos { get; set; }
    public int TiempoPreparacionMinutos { get; set; }
    public int TiempoPosteriorMinutos { get; set; }
    public decimal PrecioTotal { get; set; }
    public decimal? AdelantoRequerido { get; set; }
    public bool EsGrupal { get; set; }
    public int CapacidadMaxima { get; set; }

    public int CantidadParticipantes { get; set; }
    public EstadoReserva EstadoReserva { get; set; }
    public string? Observaciones { get; set; }

    public Organizacion Organizacion { get; set; } = null!;
    public Sede Sede { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
    public Empleado? Profesional { get; set; }
    public Recurso? Recurso { get; set; }
    public ICollection<ReservaParticipante> Participantes { get; set; } = new List<ReservaParticipante>();
    public ICollection<HistorialReserva> Historial { get; set; } = new List<HistorialReserva>();
    public Atencion? Atencion { get; set; }
    public Calificacion? Calificacion { get; set; }
}