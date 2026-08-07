using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence
{
    public interface ISedeRepository
    {
        Task<IReadOnlyList<SedeListaDto>> ListarAsync(
            SedeFiltroDto filtro,
            CancellationToken cancellationToken = default);

        Task<SedeDetalleDto?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Sede?> ObtenerParaModificarAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> ExisteNombreActivoAsync(
            Guid organizacionId,
            string nombre,
            Guid? excluirId = null,
            CancellationToken cancellationToken = default);

        void Agregar(Sede sede);

        Task GuardarAsync(CancellationToken cancellationToken = default);
    }
}
