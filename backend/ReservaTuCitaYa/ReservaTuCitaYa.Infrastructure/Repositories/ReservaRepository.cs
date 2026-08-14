using System.Data;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ReservaRepository(ApplicationDbContext context) : IReservaRepository
{
    public async Task<PaginaResultado<ReservaListaDto>> ListarAsync(
        ReservaFiltroDto filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = context.Reservas.AsNoTracking()
            .Where(r => r.OrganizacionId == filtro.OrganizacionId);

        if (filtro.SedeId.HasValue) consulta = consulta.Where(r => r.SedeId == filtro.SedeId.Value);
        if (filtro.ClienteId.HasValue) consulta = consulta.Where(r => r.ClienteId == filtro.ClienteId.Value);
        if (filtro.ProfesionalId.HasValue) consulta = consulta.Where(r => r.ProfesionalId == filtro.ProfesionalId.Value);
        if (filtro.ServicioId.HasValue) consulta = consulta.Where(r => r.ServicioId == filtro.ServicioId.Value);
        if (filtro.Estado.HasValue) consulta = consulta.Where(r => r.EstadoReserva == filtro.Estado.Value);
        if (filtro.Desde.HasValue) consulta = consulta.Where(r => r.Fecha >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) consulta = consulta.Where(r => r.Fecha <= filtro.Hasta.Value);

        var total = await consulta.CountAsync(ct);
        var elementos = await consulta
            .OrderByDescending(r => r.Fecha).ThenByDescending(r => r.HoraInicio)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(r => new ReservaListaDto(
                r.Id, r.Codigo,
                r.Cliente.Nombres + " " + r.Cliente.Apellidos,
                r.Servicio.Nombre, r.Sede.Nombre,
                r.Profesional != null ? r.Profesional.Nombres + " " + r.Profesional.Apellidos : null,
                r.Fecha, r.HoraInicio, r.HoraFinServicio,
                r.EstadoReserva.ToString(), r.CantidadParticipantes))
            .ToListAsync(ct);
        return new PaginaResultado<ReservaListaDto>(elementos, pagina, tamano, total);
    }

    public Task<ReservaDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken ct = default) =>
        ProyectarDetalle(context.Reservas.AsNoTracking().Where(r => r.Id == id))
            .SingleOrDefaultAsync(ct);

    public Task<ReservaDetalleDto?> ObtenerDetallePorCodigoAsync(string codigo, CancellationToken ct = default) =>
        ProyectarDetalle(context.Reservas.AsNoTracking().Where(r => r.Codigo == codigo))
            .SingleOrDefaultAsync(ct);

    private static IQueryable<ReservaDetalleDto> ProyectarDetalle(IQueryable<Reserva> query) =>
        query.Select(r => new ReservaDetalleDto(
            r.Id, r.Codigo, r.EstadoReserva.ToString(),
            new EntidadResumenDto(r.ClienteId, r.Cliente.Nombres + " " + r.Cliente.Apellidos),
            new EntidadResumenDto(r.ServicioId, r.Servicio.Nombre),
            new EntidadResumenDto(r.SedeId, r.Sede.Nombre),
            r.Profesional != null
                ? new EntidadResumenDto(r.ProfesionalId!.Value, r.Profesional.Nombres + " " + r.Profesional.Apellidos)
                : null,
            r.Recurso != null ? new EntidadResumenDto(r.RecursoId!.Value, r.Recurso.Nombre) : null,
            r.Fecha, r.HoraInicio, r.HoraFinServicio, r.HoraInicioOcupacion, r.HoraFinOcupacion,
            r.DuracionMinutos, r.TiempoPreparacionMinutos, r.TiempoPosteriorMinutos,
            r.CantidadParticipantes, r.EsGrupal, r.CapacidadMaxima,
            r.PrecioTotal, r.AdelantoRequerido, r.Observaciones,
            r.Participantes.Select(p => new ParticipanteDto(
                p.Id, p.ClienteId, p.NombreCompleto, p.EsTitular, p.Observaciones)).ToList(),
            r.Historial.OrderByDescending(h => h.FechaAccion).Select(h => new HistorialReservaDto(
                h.Id, h.EstadoAnterior, h.EstadoNuevo, h.TipoAccion, h.Motivo, h.Observacion, h.FechaAccion)).ToList()));

    public Task<Reserva?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default) =>
        context.Reservas.IgnoreQueryFilters().SingleOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default) =>
        context.Reservas.IgnoreQueryFilters().AnyAsync(r => r.Codigo == codigo, ct);

    public async Task<IReadOnlyList<Reserva>> ObtenerConflictosAsync(
        Guid? profesionalId, Guid? recursoId, DateOnly fecha,
        Guid? excluirReservaId = null, CancellationToken ct = default)
    {
        var consulta = context.Reservas.Where(r => r.Fecha == fecha);

        if (excluirReservaId.HasValue)
            consulta = consulta.Where(r => r.Id != excluirReservaId.Value);

        consulta = consulta.Where(r =>
            (profesionalId.HasValue && r.ProfesionalId == profesionalId.Value) ||
            (recursoId.HasValue && r.RecursoId == recursoId.Value));

        return await consulta.ToListAsync(ct);
    }

    public async Task<int> ObtenerCapacidadOcupadaAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly horaInicio,
        Guid? profesionalId, Guid? recursoId, Guid? excluirReservaId = null, CancellationToken ct = default)
    {
        var consulta = context.Reservas.Where(r =>
            r.ServicioId == servicioId && r.SedeId == sedeId &&
            r.Fecha == fecha && r.HoraInicio == horaInicio &&
            r.ProfesionalId == profesionalId && r.RecursoId == recursoId &&
            EstadosReserva.OcupanHorario.Contains(r.EstadoReserva));

        if (excluirReservaId.HasValue)
            consulta = consulta.Where(r => r.Id != excluirReservaId.Value);

        return await consulta.SumAsync(r => r.CantidadParticipantes, ct);
    }

    public void Agregar(Reserva reserva) => context.Reservas.Add(reserva);
    public void AgregarParticipante(ReservaParticipante participante) => context.ReservaParticipantes.Add(participante);
    public void AgregarHistorial(HistorialReserva historial) => context.HistorialReservas.Add(historial);

    public Task GuardarAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);
            try
            {
                var resultado = await operacion(ct);
                await tx.CommitAsync(ct);
                return resultado;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
}