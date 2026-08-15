using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class HorarioRecursoRepository(ApplicationDbContext context) : IHorarioRecursoRepository
{
    public async Task<IReadOnlyList<HorarioRecurso>> ListarAsync(Guid recursoId, CancellationToken ct = default) =>
        await context.HorarioRecurso.IgnoreQueryFilters()
            .Where(h => h.RecursoId == recursoId).ToListAsync(ct);

    public void Agregar(HorarioRecurso horario) => context.HorarioRecurso.Add(horario);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion, CancellationToken ct = default)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        try { await operacion(ct); await tx.CommitAsync(ct); }
        catch { await tx.RollbackAsync(ct); throw; }
    }
}