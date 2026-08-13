using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;

namespace ReservaTuCitaYa.Application.Interfaces
{
    public interface ISedeService
    {
        Task<ResultadoOperacion<IReadOnlyList<SedeListaDto>>> ListarPorOrganizacionAsync(
            SedeFiltroDto filtro,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<SedeDetalleDto>> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<Guid>> CrearAsync(
            CrearSedeSolicitud solicitud,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> ActualizarAsync(
            ActualizarSedeSolicitud solicitud,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> CambiarEstadoAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
