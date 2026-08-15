using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.DTOs.Horarios;

public sealed record IntervaloHorarioRequest(DiaSemana DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin);
public sealed record IntervaloHorarioDto(Guid Id, DiaSemana DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin);

public sealed record ActualizarHorarioSemanalSolicitud(IReadOnlyList<IntervaloHorarioRequest> Intervalos);
public sealed record HorarioSemanalDto(IReadOnlyList<IntervaloHorarioDto> Intervalos);

public sealed record ExcepcionHorarioDto(
    Guid Id, DateOnly Fecha, TipoExcepcionHorario TipoExcepcion,
    TimeOnly? HoraInicio, TimeOnly? HoraFin, string Motivo, string? Observaciones);

public abstract class GuardarExcepcionHorarioSolicitud
{
    public DateOnly Fecha { get; init; }
    public TipoExcepcionHorario TipoExcepcion { get; init; }
    public TimeOnly? HoraInicio { get; init; }
    public TimeOnly? HoraFin { get; init; }
    public string Motivo { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
}

public sealed class CrearExcepcionSedeSolicitud : GuardarExcepcionHorarioSolicitud { public Guid SedeId { get; init; } }
public sealed class ActualizarExcepcionSedeSolicitud : GuardarExcepcionHorarioSolicitud { public Guid Id { get; init; } }

public sealed class CrearExcepcionProfesionalSolicitud : GuardarExcepcionHorarioSolicitud
{ public Guid EmpleadoId { get; init; } public Guid SedeId { get; init; } }
public sealed class ActualizarExcepcionProfesionalSolicitud : GuardarExcepcionHorarioSolicitud { public Guid Id { get; init; } }

public sealed class CrearExcepcionRecursoSolicitud : GuardarExcepcionHorarioSolicitud { public Guid RecursoId { get; init; } }
public sealed class ActualizarExcepcionRecursoSolicitud : GuardarExcepcionHorarioSolicitud { public Guid Id { get; init; } }

public sealed record ExcepcionHorarioFiltroDto(
    Guid EntidadId, DateOnly? Desde, DateOnly? Hasta,
    TipoExcepcionHorario? TipoExcepcion, int Pagina, int TamanoPagina);