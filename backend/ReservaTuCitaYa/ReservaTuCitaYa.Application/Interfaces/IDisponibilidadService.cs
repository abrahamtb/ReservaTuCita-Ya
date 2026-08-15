using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Disponibilidad;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IDisponibilidadService
{
    Task<ResultadoOperacion<DisponibilidadRespuestaDto>> ConsultarAsync(
        ConsultarDisponibilidadSolicitud solicitud, CancellationToken ct = default);

    Task<ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>> ListarProfesionalesCompatiblesAsync(
        Guid sedeId, Guid servicioId, DateOnly? fecha, CancellationToken ct = default);

    Task<ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>> ListarRecursosCompatiblesAsync(
        Guid sedeId, Guid servicioId, DateOnly? fecha, CancellationToken ct = default);

    Task<bool> ValidarSlotEspecificoAsync(
        Guid sedeId, Guid servicioId, DateOnly fecha, TimeOnly horaInicio,
        Guid? profesionalId, Guid? recursoId, Guid? reservaIdExcluir = null,
        CancellationToken ct = default);

    Task<bool> ProfesionalDisponibleAsync(
        Guid profesionalId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default);

    Task<Guid?> ObtenerProfesionalDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default);

    Task<bool> RecursoDisponibleAsync(
        Guid recursoId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default);

    Task<Guid?> ObtenerRecursoDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default);
}
