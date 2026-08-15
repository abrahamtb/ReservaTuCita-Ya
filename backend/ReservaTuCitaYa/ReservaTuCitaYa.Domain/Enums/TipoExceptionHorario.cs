using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Domain.Enums
{
    public enum TipoExcepcionHorario
    {
        NoDefinida = 0,
        CerradoTodoElDia = 1,
        HorarioEspecial = 2,
        NoDisponibleParcial = 3
    }
}
