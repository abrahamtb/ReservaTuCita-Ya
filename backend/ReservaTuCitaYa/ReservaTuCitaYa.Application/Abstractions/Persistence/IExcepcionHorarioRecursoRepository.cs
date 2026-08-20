using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IExcepcionHorarioRecursoRepository
{
    Task<PaginaResultado<ExcepcionHorarioDto>> ListarAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default);
    Task<ExcepcionHorarioRecurso?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExcepcionHorarioRecurso>> ObtenerActivasEnFechaAsync(
        Guid recursoId, DateOnly fecha, Guid? excluirId = null, CancellationToken ct = default);
    void Agregar(ExcepcionHorarioRecurso excepcion);
    Task GuardarAsync(CancellationToken ct = default);
}