// Application/Interfaces/IBloqueoRecursoService.cs
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
namespace ReservaTuCitaYa.Application.Interfaces;

public interface IBloqueoRecursoService
{
    Task<ResultadoOperacion<IReadOnlyList<BloqueoRecursoDto>>> ListarPorRecursoAsync(
        Guid recursoId, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion<Guid>> CrearAsync(
        CrearBloqueoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> ActualizarAsync(
        ActualizarBloqueoSolicitud solicitud, CancellationToken cancellationToken = default);
    Task<ResultadoOperacion> EliminarAsync(
        Guid id, CancellationToken cancellationToken = default);
}