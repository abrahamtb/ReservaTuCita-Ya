using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IEmpleadoService
{
    Task<ResultadoOperacion<PaginaResultado<EmpleadoListaDto>>> ListarAsync(
        EmpleadoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<EmpleadoDetalleDto>> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearEmpleadoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarEmpleadoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> CambiarEstadoAsync(
        Guid id, bool estaActivo, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<IReadOnlyList<EmpleadoSedeDto>>> ListarSedesAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ReemplazarSedesAsync(
        Guid id, IReadOnlyList<Guid> sedeIds, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<IReadOnlyList<ProfesionalServicioDto>>> ListarServiciosAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ReemplazarServiciosAsync(
        Guid id, IReadOnlyList<Guid> servicioIds, CancellationToken cancellationToken = default);
}
