using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IExcepcionHorarioProfesionalRepository
{
    Task<PaginaResultado<ExcepcionHorarioDto>> ListarAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ExcepcionHorarioProfesional?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioProfesional>> ObtenerActivasEnFechaAsync(
        Guid empleadoId, DateOnly fecha, Guid? excluirId = null, CancellationToken ct = default);
    void Agregar(ExcepcionHorarioProfesional excepcion);
    Task GuardarAsync(CancellationToken ct = default);
}