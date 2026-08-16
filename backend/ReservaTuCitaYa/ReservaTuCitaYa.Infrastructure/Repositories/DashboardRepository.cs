using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Dashboard;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class DashboardRepository(
    ApplicationDbContext context) : IDashboardRepository
{
    public async Task<DashboardResumenDto> ObtenerAsync(
        DashboardFiltroDto filtro,
        CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var ayer = hoy.AddDays(-1);

        var inicioHoy = hoy.ToDateTime(TimeOnly.MinValue);
        var finHoy = hoy.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var inicioAyer = ayer.ToDateTime(TimeOnly.MinValue);
        var finAyer = hoy.ToDateTime(TimeOnly.MinValue);

        // ========================================
        // PERIODO ACTUAL / PERIODO ANTERIOR
        // ========================================

        var cantidadDias =
            filtro.FechaHasta.DayNumber -
            filtro.FechaDesde.DayNumber + 1;

        var fechaHastaAnterior =
            filtro.FechaDesde.AddDays(-1);

        var fechaDesdeAnterior =
            fechaHastaAnterior.AddDays(-(cantidadDias - 1));

        var inicioPeriodo =
            filtro.FechaDesde.ToDateTime(TimeOnly.MinValue);

        var finPeriodo =
            filtro.FechaHasta
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

        var inicioPeriodoAnterior =
            fechaDesdeAnterior.ToDateTime(TimeOnly.MinValue);

        var finPeriodoAnterior =
            fechaHastaAnterior
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

        // ========================================
        // RESERVAS HOY
        // ========================================

        var reservasHoyActual =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .CountAsync(
                    r => r.Fecha == hoy,
                    ct);

        var reservasHoyAnterior =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .CountAsync(
                    r => r.Fecha == ayer,
                    ct);

        // ========================================
        // POR ATENDER HOY
        // ========================================

        var porAtenderActual =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .CountAsync(
                    r =>
                        r.Fecha == hoy &&
                        (
                            r.EstadoReserva == EstadoReserva.Confirmada ||
                            r.EstadoReserva == EstadoReserva.Reprogramada ||
                            r.EstadoReserva == EstadoReserva.Presente ||
                            r.EstadoReserva == EstadoReserva.EnAtencion
                        ),
                    ct);

        var porAtenderAnterior =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .CountAsync(
                    r =>
                        r.Fecha == ayer &&
                        (
                            r.EstadoReserva == EstadoReserva.Confirmada ||
                            r.EstadoReserva == EstadoReserva.Reprogramada ||
                            r.EstadoReserva == EstadoReserva.Presente ||
                            r.EstadoReserva == EstadoReserva.EnAtencion
                        ),
                    ct);

        // ========================================
        // ATENCIONES COMPLETADAS HOY
        // Se utiliza FechaHoraFinReal.
        // ========================================

        var atencionesHoyQuery =
            context.Atenciones
                .AsNoTracking()
                .Where(a =>
                    a.Reserva.OrganizacionId ==
                        filtro.OrganizacionId &&
                    a.FechaHoraFinReal != null);

        if (filtro.SedeId.HasValue)
        {
            atencionesHoyQuery =
                atencionesHoyQuery.Where(
                    a => a.Reserva.SedeId ==
                         filtro.SedeId.Value);
        }

        var atencionesCompletadasActual =
            await atencionesHoyQuery.CountAsync(
                a =>
                    a.FechaHoraFinReal >= inicioHoy &&
                    a.FechaHoraFinReal < finHoy,
                ct);

        var atencionesCompletadasAnterior =
            await atencionesHoyQuery.CountAsync(
                a =>
                    a.FechaHoraFinReal >= inicioAyer &&
                    a.FechaHoraFinReal < finAyer,
                ct);

        // ========================================
        // CANCELACIONES DEL PERIODO
        // Se utiliza FechaCancelacion.
        // ========================================

        var cancelacionesQuery =
            context.CancelacionesReserva
                .AsNoTracking()
                .Where(c =>
                    c.Reserva.OrganizacionId ==
                    filtro.OrganizacionId);

        if (filtro.SedeId.HasValue)
        {
            cancelacionesQuery =
                cancelacionesQuery.Where(
                    c => c.Reserva.SedeId ==
                         filtro.SedeId.Value);
        }

        var cancelacionesActual =
            await cancelacionesQuery.CountAsync(
                c =>
                    c.FechaCancelacion >= inicioPeriodo &&
                    c.FechaCancelacion < finPeriodo,
                ct);

        var cancelacionesAnterior =
            await cancelacionesQuery.CountAsync(
                c =>
                    c.FechaCancelacion >=
                        inicioPeriodoAnterior &&
                    c.FechaCancelacion <
                        finPeriodoAnterior,
                ct);

        // ========================================
        // CLIENTES NUEVOS
        // Métrica de organización.
        // NO cambia con sede.
        // ========================================

        var clientesQuery =
            context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.OrganizacionId ==
                    filtro.OrganizacionId);

        var clientesNuevosActual =
            await clientesQuery.CountAsync(
                c =>
                    c.FechaCreacion >= inicioPeriodo &&
                    c.FechaCreacion < finPeriodo,
                ct);

        var clientesNuevosAnterior =
            await clientesQuery.CountAsync(
                c =>
                    c.FechaCreacion >=
                        inicioPeriodoAnterior &&
                    c.FechaCreacion <
                        finPeriodoAnterior,
                ct);

        // ========================================
        // INGRESOS BRUTOS
        // Pago no anulado + FechaPago.
        // Pago no tiene navegación Reserva,
        // por eso utilizamos JOIN.
        // ========================================

        var pagosBase =
            from pago in context.Pagos.AsNoTracking()
            join reserva in context.Reservas.AsNoTracking()
                on pago.ReservaId equals reserva.Id
            where
                reserva.OrganizacionId ==
                    filtro.OrganizacionId &&
                !pago.EstaAnulado
            select new
            {
                Pago = pago,
                Reserva = reserva
            };

        if (filtro.SedeId.HasValue)
        {
            pagosBase =
                pagosBase.Where(
                    x => x.Reserva.SedeId ==
                         filtro.SedeId.Value);
        }

        var ingresosBrutosActual =
            await pagosBase
                .Where(x =>
                    x.Pago.FechaPago >= filtro.FechaDesde &&
                    x.Pago.FechaPago <= filtro.FechaHasta)
                .Select(x => (decimal?)x.Pago.Monto)
                .SumAsync(ct) ?? 0m;

        var ingresosBrutosAnterior =
            await pagosBase
                .Where(x =>
                    x.Pago.FechaPago >= fechaDesdeAnterior &&
                    x.Pago.FechaPago <= fechaHastaAnterior)
                .Select(x => (decimal?)x.Pago.Monto)
                .SumAsync(ct) ?? 0m;

        // ========================================
        // REEMBOLSOS
        // ========================================

        var reembolsosBase =
            from reembolso in
                context.ReembolsosReserva.AsNoTracking()
            join reserva in context.Reservas.AsNoTracking()
                on reembolso.ReservaId equals reserva.Id
            where
                reserva.OrganizacionId ==
                    filtro.OrganizacionId
            select new
            {
                Reembolso = reembolso,
                Reserva = reserva
            };

        if (filtro.SedeId.HasValue)
        {
            reembolsosBase =
                reembolsosBase.Where(
                    x => x.Reserva.SedeId ==
                         filtro.SedeId.Value);
        }

        var reembolsosActual =
            await reembolsosBase
                .Where(x =>
                    x.Reembolso.FechaReembolso >=
                        filtro.FechaDesde &&
                    x.Reembolso.FechaReembolso <=
                        filtro.FechaHasta)
                .Select(x =>
                    (decimal?)x.Reembolso.Monto)
                .SumAsync(ct) ?? 0m;

        var reembolsosAnterior =
            await reembolsosBase
                .Where(x =>
                    x.Reembolso.FechaReembolso >=
                        fechaDesdeAnterior &&
                    x.Reembolso.FechaReembolso <=
                        fechaHastaAnterior)
                .Select(x =>
                    (decimal?)x.Reembolso.Monto)
                .SumAsync(ct) ?? 0m;

        var ingresosNetosActual =
            ingresosBrutosActual - reembolsosActual;

        var ingresosNetosAnterior =
            ingresosBrutosAnterior - reembolsosAnterior;

        // ========================================
        // GRÁFICO: RESERVAS POR DÍA
        // ========================================

        var reservasPorDiaDb =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .Where(r =>
                    r.Fecha >= filtro.FechaDesde &&
                    r.Fecha <= filtro.FechaHasta)
                .GroupBy(r => r.Fecha)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync(ct);

        // Incluimos días con cero reservas.
        var reservasPorDia =
            Enumerable
                .Range(0, cantidadDias)
                .Select(indice =>
                {
                    var fecha =
                        filtro.FechaDesde.AddDays(indice);

                    var dato =
                        reservasPorDiaDb.FirstOrDefault(
                            x => x.Fecha == fecha);

                    return new ReservaPorDiaDto(
                        fecha,
                        dato?.Cantidad ?? 0);
                })
                .ToList();

        // ========================================
        // GRÁFICO: RESERVAS POR ESTADO
        // ========================================

        var reservasPorEstado =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .Where(r =>
                    r.Fecha >= filtro.FechaDesde &&
                    r.Fecha <= filtro.FechaHasta)
                .GroupBy(r => r.EstadoReserva)
                .Select(g =>
                    new ReservaPorEstadoDto(
                        g.Key.ToString(),
                        g.Count()))
                .OrderBy(x => x.Estado)
                .ToListAsync(ct);

        // ========================================
        // GRÁFICO: INGRESOS POR DÍA
        // ========================================

        var pagosPorDia =
            await pagosBase
                .Where(x =>
                    x.Pago.FechaPago >= filtro.FechaDesde &&
                    x.Pago.FechaPago <= filtro.FechaHasta)
                .GroupBy(x => x.Pago.FechaPago)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(x => x.Pago.Monto)
                })
                .ToListAsync(ct);

        var reembolsosPorDia =
            await reembolsosBase
                .Where(x =>
                    x.Reembolso.FechaReembolso >=
                        filtro.FechaDesde &&
                    x.Reembolso.FechaReembolso <=
                        filtro.FechaHasta)
                .GroupBy(x => x.Reembolso.FechaReembolso)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(
                        x => x.Reembolso.Monto)
                })
                .ToListAsync(ct);

        var ingresosPorDia =
            Enumerable
                .Range(0, cantidadDias)
                .Select(indice =>
                {
                    var fecha =
                        filtro.FechaDesde.AddDays(indice);

                    var bruto =
                        pagosPorDia
                            .FirstOrDefault(
                                x => x.Fecha == fecha)
                            ?.Total ?? 0m;

                    var reembolso =
                        reembolsosPorDia
                            .FirstOrDefault(
                                x => x.Fecha == fecha)
                            ?.Total ?? 0m;

                    return new IngresoPorDiaDto(
                        fecha,
                        bruto,
                        reembolso,
                        bruto - reembolso);
                })
                .ToList();

        // ========================================
        // TOP 5 SERVICIOS
        // ========================================

        var reservasPeriodo =
            ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .Where(r =>
                    r.Fecha >= filtro.FechaDesde &&
                    r.Fecha <= filtro.FechaHasta);

        var totalReservasPeriodo =
            await reservasPeriodo.CountAsync(ct);

        var topServiciosDb =
            await reservasPeriodo
                .GroupBy(r => new
                {
                    r.ServicioId,
                    r.Servicio.Nombre
                })
                .Select(g => new
                {
                    g.Key.ServicioId,
                    g.Key.Nombre,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.Nombre)
                .Take(5)
                .ToListAsync(ct);

        var topServicios =
            topServiciosDb
                .Select(x =>
                    new TopServicioDto(
                        x.ServicioId,
                        x.Nombre,
                        x.Cantidad,
                        totalReservasPeriodo == 0
                            ? 0m
                            : Math.Round(
                                (decimal)x.Cantidad /
                                totalReservasPeriodo *
                                100m,
                                2)))
                .ToList();

        // ========================================
        // PRÓXIMAS RESERVAS DEL DÍA
        // ========================================

        var horaActual =
            TimeOnly.FromDateTime(DateTime.Now);

        var proximasReservas =
            await ReservasBase(
                    filtro.OrganizacionId,
                    filtro.SedeId)
                .Where(r =>
                    r.Fecha == hoy &&
                    r.HoraInicio >= horaActual &&
                    (
                        r.EstadoReserva ==
                            EstadoReserva.Confirmada ||
                        r.EstadoReserva ==
                            EstadoReserva.Reprogramada ||
                        r.EstadoReserva ==
                            EstadoReserva.Presente ||
                        r.EstadoReserva ==
                            EstadoReserva.EnAtencion
                    ))
                .OrderBy(r => r.HoraInicio)
                .Take(10)
                .Select(r =>
                    new ProximaReservaDto(
                        r.Id,
                        r.Codigo,
                        r.HoraInicio,
                        r.Cliente.Nombres + " " +
                            r.Cliente.Apellidos,
                        r.Servicio.Nombre,
                        r.Profesional != null
                            ? r.Profesional.Nombres + " " +
                              r.Profesional.Apellidos
                            : null,
                        r.EstadoReserva.ToString()))
                .ToListAsync(ct);

        // ========================================
        // RESULTADO
        // ========================================

        return new DashboardResumenDto(
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.SedeId,
            DateTime.UtcNow,

            CrearKpi(
                reservasHoyActual,
                reservasHoyAnterior),

            CrearKpi(
                porAtenderActual,
                porAtenderAnterior),

            CrearKpi(
                atencionesCompletadasActual,
                atencionesCompletadasAnterior),

            CrearKpi(
                cancelacionesActual,
                cancelacionesAnterior),

            CrearKpi(
                clientesNuevosActual,
                clientesNuevosAnterior),

            CrearKpiDecimal(
                ingresosNetosActual,
                ingresosNetosAnterior),

            reservasPorDia,
            reservasPorEstado,
            ingresosPorDia,
            topServicios,
            proximasReservas);
    }

    // ==========================================
    // CONSULTA BASE DE RESERVAS
    // ==========================================

    private IQueryable<ReservaTuCitaYa.Domain.Entities.Reserva>
        ReservasBase(
            Guid organizacionId,
            Guid? sedeId)
    {
        var consulta =
            context.Reservas
                .AsNoTracking()
                .Where(r =>
                    r.OrganizacionId ==
                    organizacionId);

        if (sedeId.HasValue)
        {
            consulta =
                consulta.Where(
                    r => r.SedeId ==
                         sedeId.Value);
        }

        return consulta;
    }

    // ==========================================
    // COMPARACIONES
    // ==========================================

    private static DashboardKpiDto CrearKpi(
        int actual,
        int anterior)
    {
        if (anterior == 0)
        {
            if (actual == 0)
            {
                return new DashboardKpiDto(
                    0,
                    0,
                    0m,
                    false);
            }

            return new DashboardKpiDto(
                actual,
                0,
                null,
                true);
        }

        var variacion =
            ((decimal)(actual - anterior) /
             anterior) * 100m;

        return new DashboardKpiDto(
            actual,
            anterior,
            Math.Round(variacion, 2),
            false);
    }

    private static DashboardKpiDecimalDto CrearKpiDecimal(
        decimal actual,
        decimal anterior)
    {
        if (anterior == 0m)
        {
            if (actual == 0m)
            {
                return new DashboardKpiDecimalDto(
                    0m,
                    0m,
                    0m,
                    false);
            }

            return new DashboardKpiDecimalDto(
                actual,
                0m,
                null,
                true);
        }

        var variacion =
            ((actual - anterior) /
             anterior) * 100m;

        return new DashboardKpiDecimalDto(
            actual,
            anterior,
            Math.Round(variacion, 2),
            false);
    }
}