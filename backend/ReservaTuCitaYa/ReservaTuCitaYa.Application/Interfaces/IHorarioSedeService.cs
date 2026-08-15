// Application/Interfaces/IHorarioSedeService.cs
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IHorarioSedeService
{
    Task<ResultadoOperacion<HorarioSemanalDto>> ListarAsync(Guid sedeId, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarAsync(
        Guid sedeId, ActualizarHorarioSemanalSolicitud solicitud, CancellationToken ct = default);

    Task<ResultadoOperacion<PaginaResultado<ExcepcionHorarioDto>>> ListarExcepcionesAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ResultadoOperacion<Guid>> CrearExcepcionAsync(
        CrearExcepcionSedeSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> ActualizarExcepcionAsync(
        ActualizarExcepcionSedeSolicitud solicitud, CancellationToken ct = default);
    Task<ResultadoOperacion> EliminarExcepcionAsync(Guid id, CancellationToken ct = default);
}