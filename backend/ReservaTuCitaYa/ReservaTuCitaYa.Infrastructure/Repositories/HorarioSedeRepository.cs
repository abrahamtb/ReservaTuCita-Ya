using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class HorarioSedeRepository(ApplicationDbContext context) : IHorarioSedeRepository
{
    public async Task<IReadOnlyList<HorarioSede>> ListarAsync(Guid sedeId, CancellationToken ct = default) =>
        await context.HorarioSede.IgnoreQueryFilters()
            .Where(h => h.SedeId == sedeId).ToListAsync(ct);

    public void Agregar(HorarioSede horario) => context.HorarioSede.Add(horario);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion, CancellationToken ct = default)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        try { await operacion(ct); await tx.CommitAsync(ct); }
        catch { await tx.RollbackAsync(ct); throw; }
    }
}