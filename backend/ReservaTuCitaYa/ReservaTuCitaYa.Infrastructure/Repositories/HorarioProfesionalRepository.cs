// Infrastructure/Repositories/HorarioProfesionalRepository.cs
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class HorarioProfesionalRepository(ApplicationDbContext context) : IHorarioProfesionalRepository
{
    public async Task<IReadOnlyList<HorarioProfesional>> ListarPorSedeAsync(
        Guid empleadoId, Guid sedeId, CancellationToken ct = default) =>
        await context.HorarioProfesional.IgnoreQueryFilters()
            .Where(h => h.EmpleadoId == empleadoId && h.SedeId == sedeId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HorarioProfesional>> ListarPorEmpleadoAsync(
        Guid empleadoId, CancellationToken ct = default) =>
        await context.HorarioProfesional.IgnoreQueryFilters()
            .Where(h => h.EmpleadoId == empleadoId)
            .ToListAsync(ct);

    public void Agregar(HorarioProfesional horario) => context.HorarioProfesional.Add(horario);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion, CancellationToken ct = default)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        try { await operacion(ct); await tx.CommitAsync(ct); }
        catch { await tx.RollbackAsync(ct); throw; }
    }
}