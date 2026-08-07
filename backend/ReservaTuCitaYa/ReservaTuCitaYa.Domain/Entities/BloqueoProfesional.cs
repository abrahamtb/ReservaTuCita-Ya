using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities;

public class BloqueoProfesional : BaseEntity
{
    public Guid ProfesionalId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public string Motivo { get; set; } = null!;

    public TipoBloqueo TipoBloqueo { get; set; }

   // public Profesional Profesional { get; set; } = null!;
}