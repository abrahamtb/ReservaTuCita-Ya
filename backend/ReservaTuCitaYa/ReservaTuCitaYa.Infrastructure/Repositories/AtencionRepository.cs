using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Atenciones;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class AtencionRepository(
    ApplicationDbContext db) : IAtencionRepository
{
    public Task<Reserva?> ObtenerReservaParaModificarAsync(
        Guid reservaId,
        CancellationToken ct = default) =>
        db.Reservas
            .FirstOrDefaultAsync(
                x => x.Id == reservaId,
                ct);

    public Task<Atencion?> ObtenerPorReservaIdAsync(
        Guid reservaId,
        CancellationToken ct = default) =>
        db.Atenciones
            .FirstOrDefaultAsync(
                x => x.ReservaId == reservaId,
                ct);

    public void Agregar(Atencion atencion) =>
        db.Atenciones.Add(atencion);

    public void AgregarHistorial(HistorialReserva historial) =>
        db.HistorialReservas.Add(historial);

    public Task GuardarAsync(
        CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public async Task<TResult> EjecutarEnTransaccionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacion,
        CancellationToken ct = default)
    {
        // EF InMemory usado por algunos tests no soporta
        // transacciones relacionales reales.
        if (!db.Database.IsRelational())
        {
            return await operacion(ct);
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(ct);

        try
        {
            var resultado = await operacion(ct);

            await transaction.CommitAsync(ct);

            return resultado;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    public async Task<AtencionDetalleDto?> ObtenerDetalleAsync(
    Guid reservaId,
    CancellationToken ct = default)
    {
        var datos = await db.Atenciones
            .AsNoTracking()
            .Where(a => a.ReservaId == reservaId)
            .Select(a => new
            {
                a.Id,
                a.ReservaId,

                a.Reserva.OrganizacionId,
                a.Reserva.Codigo,
                Estado = a.Reserva.EstadoReserva.ToString(),

                ClienteId = a.Reserva.ClienteId,
                ClienteNombre =
                    (a.Reserva.Cliente.Nombres + " " +
                     a.Reserva.Cliente.Apellidos).Trim(),

                ServicioId = a.Reserva.ServicioId,
                ServicioNombre = a.Reserva.Servicio.Nombre,

                SedeId = a.Reserva.SedeId,
                SedeNombre = a.Reserva.Sede.Nombre,

                a.Reserva.ProfesionalId,

                ProfesionalNombre = a.Reserva.Profesional == null
                    ? null
                    : (a.Reserva.Profesional.Nombres + " " +
                       a.Reserva.Profesional.Apellidos).Trim(),

                a.Reserva.Fecha,
                a.Reserva.HoraInicio,
                a.Reserva.HoraFinServicio,

                a.FechaHoraPresencia,
                a.FechaHoraInicioReal,
                a.FechaHoraFinReal,

                a.ResultadoAtencion,
                a.Observaciones,
                a.Recomendaciones,

                a.ProximoServicioId,

                ProximoServicioNombre = a.ProximoServicio == null
                    ? null
                    : a.ProximoServicio.Nombre,

                a.ProximaFechaSugerida
            })
            .SingleOrDefaultAsync(ct);

        if (datos is null)
        {
            return null;
        }

        int? minutosEspera = null;

        if (datos.FechaHoraPresencia.HasValue &&
            datos.FechaHoraInicioReal.HasValue)
        {
            minutosEspera = Math.Max(
                0,
                (int)(datos.FechaHoraInicioReal.Value -
                      datos.FechaHoraPresencia.Value).TotalMinutes);
        }

        int? duracionRealMinutos = null;

        if (datos.FechaHoraInicioReal.HasValue &&
            datos.FechaHoraFinReal.HasValue)
        {
            duracionRealMinutos = Math.Max(
                0,
                (int)(datos.FechaHoraFinReal.Value -
                      datos.FechaHoraInicioReal.Value).TotalMinutes);
        }

        return new AtencionDetalleDto(
            datos.Id,
            datos.ReservaId,
            datos.OrganizacionId,
            datos.Codigo,
            datos.Estado,

            new EntidadResumenDto(
                datos.ClienteId,
                datos.ClienteNombre),

            new EntidadResumenDto(
                datos.ServicioId,
                datos.ServicioNombre),

            new EntidadResumenDto(
                datos.SedeId,
                datos.SedeNombre),

            datos.ProfesionalId.HasValue
                ? new EntidadResumenDto(
                    datos.ProfesionalId.Value,
                    datos.ProfesionalNombre ?? string.Empty)
                : null,

            datos.Fecha,
            datos.HoraInicio,
            datos.HoraFinServicio,

            datos.FechaHoraPresencia,
            datos.FechaHoraInicioReal,
            datos.FechaHoraFinReal,

            minutosEspera,
            duracionRealMinutos,

            datos.ResultadoAtencion,
            datos.Observaciones,
            datos.Recomendaciones,

            datos.ProximoServicioId.HasValue
                ? new EntidadResumenDto(
                    datos.ProximoServicioId.Value,
                    datos.ProximoServicioNombre ?? string.Empty)
                : null,

            datos.ProximaFechaSugerida);
    }
    public async Task<AgendaProfesionalDto?> ObtenerAgendaProfesionalAsync(
    Guid organizacionId,
    Guid profesionalId,
    DateOnly fecha,
    CancellationToken ct = default)
    {
        var profesional = await db.Empleados
            .AsNoTracking()
            .Where(e =>
                e.Id == profesionalId &&
                e.OrganizacionId == organizacionId &&
                e.EsProfesional)
            .Select(e => new
            {
                e.Id,
                Nombre = (e.Nombres + " " + e.Apellidos).Trim()
            })
            .SingleOrDefaultAsync(ct);

        if (profesional is null)
        {
            return null;
        }

        var reservas = await db.Reservas
            .AsNoTracking()
            .Where(r =>
                r.OrganizacionId == organizacionId &&
                r.ProfesionalId == profesionalId &&
                r.Fecha == fecha)
            .OrderBy(r => r.HoraInicio)
            .Select(r => new AgendaProfesionalItemDto(
                r.Id,
                r.Codigo,

                r.ClienteId,
                (r.Cliente.Nombres + " " + r.Cliente.Apellidos).Trim(),

                r.ServicioId,
                r.Servicio.Nombre,

                r.SedeId,
                r.Sede.Nombre,

                r.HoraInicio,
                r.HoraFinServicio,

                r.EstadoReserva.ToString(),
                r.CantidadParticipantes,

                r.Atencion != null
                    ? r.Atencion.Id
                    : null,

                r.Atencion != null
                    ? r.Atencion.FechaHoraPresencia
                    : null,

                r.Atencion != null
                    ? r.Atencion.FechaHoraInicioReal
                    : null,

                r.Atencion != null
                    ? r.Atencion.FechaHoraFinReal
                    : null))
            .ToListAsync(ct);

        return new AgendaProfesionalDto(
            profesional.Id,
            profesional.Nombre,
            fecha,
            reservas.Count,
            reservas);
    }
}