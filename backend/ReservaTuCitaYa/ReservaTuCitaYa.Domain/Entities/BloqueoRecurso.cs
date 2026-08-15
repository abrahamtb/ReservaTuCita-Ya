using System;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities;

public class BloqueoRecurso : BaseEntity
{
    public Guid RecursoId { get; set; }

    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }

    public string Motivo { get; set; } = null!;
    public string? Observaciones { get; set; }

    public TipoBloqueo TipoBloqueo { get; set; }
    public Recurso Recurso { get; set; } = null!;
}