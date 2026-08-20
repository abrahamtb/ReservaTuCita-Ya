using Microsoft.EntityFrameworkCore;
using System.Data;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class BloqueoRecursoRepository(ApplicationDbContext context) : IBloqueoRecursoRepository
{
    public async Task<IReadOnlyList<BloqueoRecursoDto>> ListarPorRecursoAsync(
        Guid recursoId, CancellationToken cancellationToken = default) =>
        await context.BloqueoRecurso.AsNoTracking()
            .Where(b => b.RecursoId == recursoId)
            .OrderBy(b => b.FechaHoraInicio)
            .Select(b => new BloqueoRecursoDto(
                b.Id, b.RecursoId, b.FechaHoraInicio, b.FechaHoraFin,
                b.TipoBloqueo, b.Motivo, b.Observaciones))
            .ToListAsync(cancellationToken);

    public Task<BloqueoRecurso?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        context.BloqueoRecurso.IgnoreQueryFilters()
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<bool> ExisteSolapamientoAsync(
        Guid recursoId, DateTime inicio, DateTime fin, Guid? excluirId = null,
        CancellationToken cancellationToken = default) =>
        context.BloqueoRecurso.AnyAsync(b =>
            b.RecursoId == recursoId &&
            b.FechaHoraInicio < fin && b.FechaHoraFin > inicio &&
            (!excluirId.HasValue || b.Id != excluirId.Value), cancellationToken);

    public void Agregar(BloqueoRecurso bloqueo) => context.BloqueoRecurso.Add(bloqueo);

    public Task GuardarAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            try
            {
                var resultado = await operacion(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return resultado;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
