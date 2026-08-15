// Application/Abstractions/Persistence/IExcepcionHorarioSedeRepository.cs
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IExcepcionHorarioSedeRepository
{
    Task<PaginaResultado<ExcepcionHorarioDto>> ListarAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ExcepcionHorarioSede?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioSede>> ObtenerActivasEnFechaAsync(
        Guid sedeId, DateOnly fecha, Guid? excluirId = null, CancellationToken ct = default);
    void Agregar(ExcepcionHorarioSede excepcion);
    Task GuardarAsync(CancellationToken ct = default);
}