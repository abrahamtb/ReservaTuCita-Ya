using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IHorarioRecursoService
{
    Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(Guid recursoId, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarAsync(
        Guid recursoId, ActualizarHorarioSemanalSolicitud solicitud, CancellationToken ct = default);

    Task<ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepcionesAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionRecursoSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionRecursoSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> EliminarExcepcionAsync(Guid id, CancellationToken ct = default);
}