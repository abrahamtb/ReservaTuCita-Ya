using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IDisponibilidadRepository
{
    Task<Sede?> ObtenerSedeAsync(Guid sedeId, CancellationToken ct = default);
    Task<Servicio?> ObtenerServicioAsync(Guid servicioId, CancellationToken ct = default);
    Task<ServicioSede?> ObtenerServicioSedeAsync(Guid servicioId, Guid sedeId, CancellationToken ct = default);

    Task<IReadOnlyList<Empleado>> ObtenerProfesionalesCompatiblesAsync(
        Guid servicioId, Guid sedeId, CancellationToken ct = default);
    Task<IReadOnlyList<Recurso>> ObtenerRecursosCompatiblesAsync(
        Guid servicioId, Guid sedeId, CancellationToken ct = default);

    Task<Empleado?> ObtenerProfesionalAsync(Guid empleadoId, CancellationToken ct = default);
    Task<Recurso?> ObtenerRecursoAsync(Guid recursoId, CancellationToken ct = default);

    Task<IReadOnlyList<HorarioSede>> ObtenerHorariosSedeAsync(Guid sedeId, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioSede>> ObtenerExcepcionesSedeAsync(
        Guid sedeId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<IReadOnlyList<HorarioProfesional>> ObtenerHorariosProfesionalesAsync(
        IReadOnlyCollection<Guid> empleadoIds, Guid sedeId, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioProfesional>> ObtenerExcepcionesProfesionalesAsync(
        IReadOnlyCollection<Guid> empleadoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<IReadOnlyList<HorarioRecurso>> ObtenerHorariosRecursosAsync(
        IReadOnlyCollection<Guid> recursoIds, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioRecurso>> ObtenerExcepcionesRecursosAsync(
        IReadOnlyCollection<Guid> recursoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
    Task<IReadOnlyList<BloqueoRecurso>> ObtenerBloqueosAsync(
        IReadOnlyCollection<Guid> recursoIds, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
    Task<IReadOnlyList<Reserva>> ObtenerReservasActivasAsync(
        Guid sedeId, DateOnly desde, DateOnly hasta, Guid? excluirReservaId = null,
        CancellationToken ct = default);
}
