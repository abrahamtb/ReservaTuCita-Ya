// Application/Interfaces/IRecursoService.cs
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IRecursoService
{
    Task<ResultadoOperacion<PaginaResultado<RecursoListaDto>>> ListarAsync(
        RecursoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<RecursoDetalleDto>> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearRecursoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarRecursoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id, bool estaActivo, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<IReadOnlyList<RecursoServicioDto>>> ListarServiciosAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ReemplazarServiciosAsync(
        Guid id, IReadOnlyList<AsignacionServicioRecurso> servicios,
        CancellationToken cancellationToken = default);
}