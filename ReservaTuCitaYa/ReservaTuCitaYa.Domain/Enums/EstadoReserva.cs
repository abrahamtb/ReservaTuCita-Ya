using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Enums
{
    public enum EstadoReserva
    {
        NoDefinido = 0,
        Pendiente = 1,
        Confirmada = 2,
        EnCurso = 3,
        Completada = 4,
        Cancelada = 5,
        NoAsistio = 6
    }
}
