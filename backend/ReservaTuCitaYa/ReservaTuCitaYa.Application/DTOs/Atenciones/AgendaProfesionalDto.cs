using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Atenciones;

public sealed record AgendaProfesionalDto(
    Guid ProfesionalId,
    string ProfesionalNombre,
    DateOnly Fecha,
    int TotalReservas,
    IReadOnlyList<AgendaProfesionalItemDto> Reservas);

public sealed record AgendaProfesionalItemDto(
    Guid ReservaId,
    string CodigoReserva,
    Guid ClienteId,
    string ClienteNombre,
    Guid ServicioId,
    string ServicioNombre,
    Guid SedeId,
    string SedeNombre,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string Estado,
    int CantidadParticipantes,
    Guid? AtencionId,
    DateTime? FechaHoraPresencia,
    DateTime? FechaHoraInicioReal,
    DateTime? FechaHoraFinReal);
