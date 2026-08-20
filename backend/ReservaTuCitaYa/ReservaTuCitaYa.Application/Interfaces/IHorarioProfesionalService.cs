using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IHorarioProfesionalService
{
    Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(
        Guid empleadoId, Guid? sedeId, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarAsync(
        Guid empleadoId, Guid sedeId, ActualizarHorarioSemanalSolicitud solicitud,
        CancellationToken ct = default);

    Task<ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepcionesAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionProfesionalSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionProfesionalSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> EliminarExcepcionAsync(Guid id, CancellationToken ct = default);
}