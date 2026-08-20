using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Horarios;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ExcepcionHorarioProfesionalRepository(ApplicationDbContext context)
    : IExcepcionHorarioProfesionalRepository
{
    public async Task<PaginaResultado<ExcepcionHorarioDto>> ListarAsync(
        ExcepcionHorarioFiltroDto filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.ExcepcionHorarioProfesional.AsNoTracking()
            .Where(e => e.EmpleadoId == filtro.EntidadId);

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

    public Task<ExcepcionHorarioProfesional?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default) =>
        context.ExcepcionHorarioProfesional.IgnoreQueryFilters().SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ExcepcionHorarioProfesional>> ObtenerActivasEnFechaAsync(
        Guid empleadoId, DateOnly fecha, Guid? excluirId = null, CancellationToken ct = default) =>
        await context.ExcepcionHorarioProfesional
            .Where(e => e.EmpleadoId == empleadoId && e.Fecha == fecha &&
                        (!excluirId.HasValue || e.Id != excluirId.Value))
            .ToListAsync(ct);

    public void Agregar(ExcepcionHorarioProfesional excepcion) =>
        context.ExcepcionHorarioProfesional.Add(excepcion);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}