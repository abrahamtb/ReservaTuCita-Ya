using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ExcepcionHorarioRecursoRepository(ApplicationDbContext context)
    : IExcepcionHorarioRecursoRepository
{
    public async Task<PaginaResultado<ExcepcionHorarioDto>> ListarAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.ExcepcionesHorarioRecurso.AsNoTracking()
            .Where(e => e.RecursoId == filtro.EntidadId);

        if (filtro.Desde.HasValue) consulta = consulta.Where(e => e.Fecha >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) consulta = consulta.Where(e => e.Fecha <= filtro.Hasta.Value);
        if (filtro.TipoExcepcion.HasValue) consulta = consulta.Where(e => e.TipoExcepcion == filtro.TipoExcepcion.Value);

        var total = await consulta.CountAsync(ct);
        var elementos = await consulta
            .OrderBy(e => e.Fecha).ThenBy(e => e.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(e => new ExcepcionHorarioDto(
                e.Id, e.Fecha, e.TipoExcepcion, e.HoraInicio, e.HoraFin, e.Motivo, e.Observaciones))
            .ToListAsync(ct);
        return new PaginaResultado<ExcepcionHorarioDto>(elementos, pagina, tamano, total);
    }

    public Task<ExcepcionHorarioRecurso?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default) =>
        context.ExcepcionesHorarioRecurso.IgnoreQueryFilters().SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ExcepcionHorarioRecurso>> ObtenerActivasEnFechaAsync(
        Guid recursoId, DateOnly fecha, Guid? excluirId = null, CancellationToken ct = default) =>
        await context.ExcepcionesHorarioRecurso
            .Where(e => e.RecursoId == recursoId && e.Fecha == fecha &&
                        (!excluirId.HasValue || e.Id != excluirId.Value))
            .ToListAsync(ct);

    public void Agregar(ExcepcionHorarioRecurso excepcion) => context.ExcepcionesHorarioRecurso.Add(excepcion);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}