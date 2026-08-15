using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IHorarioProfesionalRepository
{
    Task<IReadOnlyList<HorarioProfesional>> ListarPorSedeAsync(
        Guid empleadoId, Guid sedeId, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProfesional>> ListarPorEmpleadoAsync(
        Guid empleadoId, CancellationToken ct = default);
    void Agregar(HorarioProfesional horario);
    Task GuardarAsync(CancellationToken ct = default);
    Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken ct = default);
}