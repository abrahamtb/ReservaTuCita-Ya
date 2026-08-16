using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Atenciones;

public sealed record MarcarNoAsistioRespuesta(
    Guid ReservaId,
    string CodigoReserva,
    string Estado,
    DateTime FechaHoraRegistro);
