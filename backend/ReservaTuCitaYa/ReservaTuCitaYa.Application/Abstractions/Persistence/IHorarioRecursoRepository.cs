// Application/Abstractions/Persistence/IHorarioRecursoRepository.cs
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IHorarioRecursoRepository
{
    Task<IReadOnlyList<HorarioRecurso>> ListarAsync(Guid recursoId, CancellationToken ct = default);
    void Agregar(HorarioRecurso horario);
    Task GuardarAsync(CancellationToken ct = default);
    Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken ct = default);
}