using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Atenciones;

public sealed record FinalizarAtencionRespuesta(
    Guid ReservaId,
    Guid AtencionId,
    string CodigoReserva,
    string Estado,
    string Resultado,
    DateTime FechaHoraFinReal);
