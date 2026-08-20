using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities
{
    public sealed class HorarioProfesional : BaseEntity
    {
        public Guid EmpleadoId { get; set; }
        public Guid SedeId { get; set; }
        public DiaSemana DiaSemana { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public Empleado Empleado { get; set; } = null!;
        public Sede Sede { get; set; } = null!;
    }
}