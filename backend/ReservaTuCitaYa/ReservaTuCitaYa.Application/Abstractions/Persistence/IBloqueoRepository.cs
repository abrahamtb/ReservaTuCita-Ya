// Application/Abstractions/Persistence/IBloqueoRecursoRepository.cs
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IBloqueoRecursoRepository
{
    Task<IReadOnlyList<BloqueoRecursoDto>> ListarPorRecursoAsync(
        Guid recursoId, CancellationToken cancellationToken = default);
    Task<BloqueoRecurso?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteSolapamientoAsync(
        Guid recursoId, DateTime inicio, DateTime fin, Guid? excluirId = null,
        CancellationToken cancellationToken = default);
    void Agregar(BloqueoRecurso bloqueo);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion,
        CancellationToken cancellationToken = default);
}
