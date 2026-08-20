using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.DTOs.Reportes;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.Infrastructure.Repositories;

public sealed class ReporteRepository(
    ApplicationDbContext context) : IReporteRepository
{
    public async Task<ReporteReservasRespuestaDto> ObtenerReservasAsync(
        ReporteReservasFiltroDto filtro,
        CancellationToken ct = default)
    {
        var consulta = context.Reservas
            .AsNoTracking()
            .Where(r =>
                r.OrganizacionId == filtro.OrganizacionId &&
                r.Fecha >= filtro.FechaDesde &&
                r.Fecha <= filtro.FechaHasta);

        if (filtro.SedeId.HasValue)
            consulta = consulta.Where(r =>
                r.SedeId == filtro.SedeId.Value);

        if (filtro.ProfesionalId.HasValue)
            consulta = consulta.Where(r =>
                r.ProfesionalId == filtro.ProfesionalId.Value);

        if (filtro.ServicioId.HasValue)
            consulta = consulta.Where(r =>
                r.ServicioId == filtro.ServicioId.Value);

        if (filtro.Estado.HasValue)
            consulta = consulta.Where(r =>
                r.EstadoReserva == filtro.Estado.Value);

        if (filtro.ClienteId.HasValue)
            consulta = consulta.Where(r =>
                r.ClienteId == filtro.ClienteId.Value);

        // =========================
        // INDICADORES
        // =========================

        var totalReservas =
            await consulta.CountAsync(ct);

        var confirmadasReprogramadas =
            await consulta.CountAsync(
                r =>
                    r.EstadoReserva == EstadoReserva.Confirmada ||
                    r.EstadoReserva == EstadoReserva.Reprogramada,
                ct);

        var atendidas =
            await consulta.CountAsync(
                r => r.EstadoReserva == EstadoReserva.Atendida,
                ct);

        var canceladas =
            await consulta.CountAsync(
                r => r.EstadoReserva == EstadoReserva.Cancelada,
                ct);

        var noAsistieron =
            await consulta.CountAsync(
                r => r.EstadoReserva == EstadoReserva.NoAsistio,
                ct);

        var indicadores =
            new ReporteReservasIndicadoresDto(
                totalReservas,
                confirmadasReprogramadas,
                atendidas,
                canceladas,
                noAsistieron);

        // =========================
        // RESERVAS POR ESTADO
        // =========================

        var estadosDb =
            await consulta
                .GroupBy(r => r.EstadoReserva)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync(ct);

        var reservasPorEstado =
            estadosDb
                .Select(x =>
                    new ReporteReservaEstadoDto(
                        x.Estado.ToString(),
                        x.Cantidad))
                .OrderBy(x => x.Estado)
                .ToList();

        // =========================
        // PAGINACIÓN
        // =========================

        var totalPaginas =
            totalReservas == 0
                ? 0
                : (int)Math.Ceiling(
                    totalReservas /
                    (double)filtro.TamanoPagina);

        var elementos =
            await consulta
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.HoraInicio)
                .ThenBy(r => r.Codigo)
                .Skip(
                    (filtro.Pagina - 1) *
                    filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .Select(r =>
                    new ReporteReservaFilaDto(
                        r.Id,
                        r.Codigo,
                        r.Fecha,
                        r.HoraInicio,
                        r.Cliente.Nombres + " " +
                            r.Cliente.Apellidos,
                        r.Servicio.Nombre,
                        r.Sede.Nombre,
                        r.Profesional != null
                            ? r.Profesional.Nombres + " " +
                              r.Profesional.Apellidos
                            : null,
                        r.EstadoReserva.ToString(),
                        r.CantidadParticipantes,
                        r.PrecioTotal))
                .ToListAsync(ct);

        return new ReporteReservasRespuestaDto(
            filtro.FechaDesde,
            filtro.FechaHasta,
            indicadores,
            reservasPorEstado,
            elementos,
            filtro.Pagina,
            filtro.TamanoPagina,
            totalReservas,
            totalPaginas);
    }

    public async Task<ReporteIngresosRespuestaDto> ObtenerIngresosAsync(
    ReporteIngresosFiltroDto filtro,
    CancellationToken ct = default)
    {
        // =========================
        // PAGOS
        // =========================

        var pagos = context.Pagos
            .AsNoTracking()
            .Where(p =>
                !p.EstaAnulado &&
                !p.EstaEliminado &&
                p.FechaPago >= filtro.FechaDesde &&
                p.FechaPago <= filtro.FechaHasta &&
                context.Reservas.Any(r =>
                    r.Id == p.ReservaId &&
                    r.OrganizacionId == filtro.OrganizacionId));

        if (filtro.SedeId.HasValue)
        {
            pagos = pagos.Where(p =>
                context.Reservas.Any(r =>
                    r.Id == p.ReservaId &&
                    r.OrganizacionId == filtro.OrganizacionId &&
                    r.SedeId == filtro.SedeId.Value));
        }

        if (filtro.MetodoPagoId.HasValue)
        {
            pagos = pagos.Where(p =>
                p.MetodoPagoId == filtro.MetodoPagoId.Value);
        }

        // =========================
        // REEMBOLSOS
        // =========================

        var reembolsos = context.ReembolsosReserva
            .AsNoTracking()
            .Where(r =>
                !r.EstaEliminado &&
                r.FechaReembolso >= filtro.FechaDesde &&
                r.FechaReembolso <= filtro.FechaHasta &&
                context.Reservas.Any(reserva =>
                    reserva.Id == r.ReservaId &&
                    reserva.OrganizacionId == filtro.OrganizacionId));

        if (filtro.SedeId.HasValue)
        {
            reembolsos = reembolsos.Where(r =>
                context.Reservas.Any(reserva =>
                    reserva.Id == r.ReservaId &&
                    reserva.OrganizacionId == filtro.OrganizacionId &&
                    reserva.SedeId == filtro.SedeId.Value));
        }

        if (filtro.MetodoPagoId.HasValue)
        {
            reembolsos = reembolsos.Where(r =>
                r.MetodoPagoId == filtro.MetodoPagoId.Value);
        }

        // =========================
        // INDICADORES
        // =========================

        var ingresosBrutos =
            await pagos.SumAsync(
                p => (decimal?)p.Monto,
                ct) ?? 0m;

        var totalReembolsos =
            await reembolsos.SumAsync(
                r => (decimal?)r.Monto,
                ct) ?? 0m;

        var cantidadPagos =
            await pagos.CountAsync(ct);

        var ingresosNetos =
            ingresosBrutos - totalReembolsos;

        decimal? ticketPromedio =
            cantidadPagos == 0
                ? null
                : ingresosBrutos / cantidadPagos;

        var indicadores =
            new ReporteIngresosIndicadoresDto(
                ingresosBrutos,
                totalReembolsos,
                ingresosNetos,
                cantidadPagos,
                ticketPromedio);

        // =========================
        // TABLA DE MOVIMIENTOS
        // =========================

        var movimientosPagos =
            pagos.Select(p =>
                new
                {
                    Fecha = p.FechaPago,
                    CodigoMovimiento = p.Codigo,
                    ReservaId = p.ReservaId,
                    Tipo = "Pago",
                    Metodo =
                        (string?)p.MetodoPago.Nombre,
                    p.NumeroOperacion,
                    p.Monto
                });

        var movimientosReembolsos =
            reembolsos.Select(r =>
                new
                {
                    Fecha = r.FechaReembolso,
                    CodigoMovimiento = r.Codigo,
                    ReservaId = r.ReservaId,
                    Tipo = "Reembolso",
                    Metodo =
                        r.MetodoPago != null
                            ? r.MetodoPago.Nombre
                            : null,
                    r.NumeroOperacion,
                    r.Monto
                });

        var movimientos =
            movimientosPagos.Concat(
                movimientosReembolsos);

        var totalElementos =
            await movimientos.CountAsync(ct);

        var totalPaginas =
            totalElementos == 0
                ? 0
                : (int)Math.Ceiling(
                    totalElementos /
                    (double)filtro.TamanoPagina);

        var movimientosPagina =
            await movimientos
                .OrderByDescending(m => m.Fecha)
                .ThenBy(m => m.CodigoMovimiento)
                .Skip(
                    (filtro.Pagina - 1) *
                    filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .ToListAsync(ct);

        /*
         * Obtenemos únicamente las reservas correspondientes
         * a los movimientos de la página actual.
         * Evitamos cargar todas las reservas.
         */
        var reservaIds =
            movimientosPagina
                .Select(m => m.ReservaId)
                .Distinct()
                .ToList();

        var reservas =
            await context.Reservas
                .AsNoTracking()
                .Where(r =>
                    reservaIds.Contains(r.Id) &&
                    r.OrganizacionId == filtro.OrganizacionId)
                .Select(r => new
                {
                    r.Id,
                    r.Codigo,
                    Cliente =
                        r.Cliente.Nombres + " " +
                        r.Cliente.Apellidos,
                    Sede = r.Sede.Nombre
                })
                .ToDictionaryAsync(
                    r => r.Id,
                    ct);

        var elementos =
            movimientosPagina
                .Where(m =>
                    reservas.ContainsKey(m.ReservaId))
                .Select(m =>
                {
                    var reserva =
                        reservas[m.ReservaId];

                    return new ReporteMovimientoFilaDto(
                        m.Fecha,
                        m.CodigoMovimiento,
                        reserva.Codigo,
                        reserva.Cliente,
                        reserva.Sede,
                        m.Tipo,
                        m.Metodo,
                        m.NumeroOperacion,
                        m.Monto);
                })
                .ToList();

        return new ReporteIngresosRespuestaDto(
            filtro.FechaDesde,
            filtro.FechaHasta,
            indicadores,
            elementos,
            filtro.Pagina,
            filtro.TamanoPagina,
            totalElementos,
            totalPaginas);
    }

    public async Task<ReporteAtencionesRespuestaDto> ObtenerAtencionesAsync(
    ReporteAtencionesFiltroDto filtro,
    CancellationToken ct = default)
    {
        // =========================
        // RESERVAS DEL PERIODO
        // =========================

        var consulta = context.Reservas
            .AsNoTracking()
            .Where(r =>
                r.OrganizacionId == filtro.OrganizacionId &&
                r.Fecha >= filtro.FechaDesde &&
                r.Fecha <= filtro.FechaHasta);

        if (filtro.SedeId.HasValue)
        {
            consulta = consulta.Where(r =>
                r.SedeId == filtro.SedeId.Value);
        }

        if (filtro.ProfesionalId.HasValue)
        {
            consulta = consulta.Where(r =>
                r.ProfesionalId == filtro.ProfesionalId.Value);
        }

        if (filtro.ServicioId.HasValue)
        {
            consulta = consulta.Where(r =>
                r.ServicioId == filtro.ServicioId.Value);
        }

        if (filtro.Estado.HasValue)
        {
            consulta = consulta.Where(r =>
                r.EstadoReserva == filtro.Estado.Value);
        }

        if (filtro.Resultado.HasValue)
        {
            consulta = consulta.Where(r =>
                r.Atencion != null &&
                r.Atencion.ResultadoAtencion == filtro.Resultado.Value);
        }

        // =========================
        // INDICADORES
        // =========================

        var reservasProgramadas =
            await consulta.CountAsync(ct);

        var atendidas =
            await consulta.CountAsync(
                r => r.EstadoReserva == EstadoReserva.Atendida,
                ct);

        var noAsistieron =
            await consulta.CountAsync(
                r => r.EstadoReserva == EstadoReserva.NoAsistio,
                ct);

        /*
         * Para atención parcial/interrumpida usamos el resultado
         * real de Atencion cuando exista.
         *
         * Si tu enum ResultadoAtencion utiliza un nombre diferente
         * a Parcial/Interrumpida, este bloque lo ajustaremos según
         * los valores reales del enum.
         */
        var atencionParcialInterrumpida =
            await consulta.CountAsync(
                r =>
                    r.Atencion != null &&
                    r.Atencion.ResultadoAtencion.HasValue &&
                    r.Atencion.ResultadoAtencion != ResultadoAtencion.Completada,
                ct);

        // =========================
        // PORCENTAJE ASISTENCIA
        // =========================

        var baseAsistencia =
            atendidas + noAsistieron;

        var sinDatos =
            baseAsistencia == 0;

        decimal? porcentajeAsistencia =
            sinDatos
                ? null
                : Math.Round(
                    atendidas * 100m / baseAsistencia,
                    2);

        var indicadores =
            new ReporteAtencionesIndicadoresDto(
                reservasProgramadas,
                atendidas,
                noAsistieron,
                atencionParcialInterrumpida,
                porcentajeAsistencia,
                sinDatos);

        // =========================
        // PAGINACIÓN
        // =========================

        var totalElementos =
            reservasProgramadas;

        var totalPaginas =
            totalElementos == 0
                ? 0
                : (int)Math.Ceiling(
                    totalElementos /
                    (double)filtro.TamanoPagina);

        // =========================
        // TABLA
        // =========================

        var datos =
            await consulta
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.HoraInicio)
                .ThenBy(r => r.Codigo)
                .Skip(
                    (filtro.Pagina - 1) *
                    filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .Select(r => new
                {
                    r.Id,
                    r.Codigo,
                    r.Fecha,
                    r.HoraInicio,

                    Cliente =
                        r.Cliente.Nombres + " " +
                        r.Cliente.Apellidos,

                    Servicio =
                        r.Servicio.Nombre,

                    Profesional =
                        r.Profesional != null
                            ? r.Profesional.Nombres + " " +
                              r.Profesional.Apellidos
                            : null,

                    HoraLlegada =
                        r.Atencion != null
                            ? r.Atencion.FechaHoraPresencia
                            : null,

                    HoraInicioReal =
                        r.Atencion != null
                            ? r.Atencion.FechaHoraInicioReal
                            : null,

                    HoraFinReal =
                        r.Atencion != null
                            ? r.Atencion.FechaHoraFinReal
                            : null,

                    Resultado =
                        r.Atencion != null
                            ? r.Atencion.ResultadoAtencion
                            : null,

                    Estado =
                        r.EstadoReserva
                })
                .ToListAsync(ct);

        var elementos =
            datos.Select(x =>
            {
                int? duracionRealMinutos = null;

                if (x.HoraInicioReal.HasValue &&
                    x.HoraFinReal.HasValue)
                {
                    duracionRealMinutos =
                        (int)Math.Round(
                            (x.HoraFinReal.Value -
                             x.HoraInicioReal.Value)
                            .TotalMinutes);
                }

                return new ReporteAtencionFilaDto(
                    x.Id,
                    x.Codigo,
                    x.Fecha,
                    x.HoraInicio,
                    x.Cliente,
                    x.Servicio,
                    x.Profesional,
                    x.HoraLlegada,
                    x.HoraInicioReal,
                    x.HoraFinReal,
                    duracionRealMinutos,
                    x.Resultado?.ToString(),
                    x.Estado.ToString());
            })
            .ToList();

        return new ReporteAtencionesRespuestaDto(
            filtro.FechaDesde,
            filtro.FechaHasta,
            indicadores,
            elementos,
            filtro.Pagina,
            filtro.TamanoPagina,
            totalElementos,
            totalPaginas);
    }
    public async Task<IReadOnlyList<ReporteReservaFilaDto>> ExportarReservasAsync(
    ReporteReservasFiltroDto filtro,
    CancellationToken ct = default)
    {
        var consulta = context.Reservas
            .AsNoTracking()
            .Where(r =>
                r.OrganizacionId == filtro.OrganizacionId &&
                r.Fecha >= filtro.FechaDesde &&
                r.Fecha <= filtro.FechaHasta);

        if (filtro.SedeId.HasValue)
            consulta = consulta.Where(r =>
                r.SedeId == filtro.SedeId.Value);

        if (filtro.ProfesionalId.HasValue)
            consulta = consulta.Where(r =>
                r.ProfesionalId == filtro.ProfesionalId.Value);

        if (filtro.ServicioId.HasValue)
            consulta = consulta.Where(r =>
                r.ServicioId == filtro.ServicioId.Value);

        if (filtro.Estado.HasValue)
            consulta = consulta.Where(r =>
                r.EstadoReserva == filtro.Estado.Value);

        if (filtro.ClienteId.HasValue)
            consulta = consulta.Where(r =>
                r.ClienteId == filtro.ClienteId.Value);

        var datos = await consulta
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.HoraInicio)
            .ThenBy(r => r.Codigo)
            .Select(r => new
            {
                r.Id,
                r.Codigo,
                r.Fecha,
                r.HoraInicio,
                Cliente =
                    r.Cliente.Nombres + " " +
                    r.Cliente.Apellidos,
                Servicio = r.Servicio.Nombre,
                Sede = r.Sede.Nombre,
                Profesional =
                    r.Profesional != null
                        ? r.Profesional.Nombres + " " +
                          r.Profesional.Apellidos
                        : null,
                Estado = r.EstadoReserva,
                r.CantidadParticipantes,
                r.PrecioTotal
            })
            .ToListAsync(ct);

        return datos
            .Select(r => new ReporteReservaFilaDto(
                r.Id,
                r.Codigo,
                r.Fecha,
                r.HoraInicio,
                r.Cliente,
                r.Servicio,
                r.Sede,
                r.Profesional,
                r.Estado.ToString(),
                r.CantidadParticipantes,
                r.PrecioTotal))
            .ToList();
    }

    public async Task<IReadOnlyList<ReporteMovimientoFilaDto>> ExportarIngresosAsync(
        ReporteIngresosFiltroDto filtro,
        CancellationToken ct = default)
    {
        var pagos =
            from p in context.Pagos.AsNoTracking()
            join r in context.Reservas.AsNoTracking()
                on p.ReservaId equals r.Id
            where
                r.OrganizacionId == filtro.OrganizacionId &&
                !p.EstaAnulado &&
                !p.EstaEliminado &&
                p.FechaPago >= filtro.FechaDesde &&
                p.FechaPago <= filtro.FechaHasta
            select new
            {
                Fecha = p.FechaPago,
                CodigoMovimiento = p.Codigo,
                CodigoReserva = r.Codigo,
                Cliente =
                    r.Cliente.Nombres + " " +
                    r.Cliente.Apellidos,
                SedeId = r.SedeId,
                Sede = r.Sede.Nombre,
                Tipo = "Pago",
                MetodoPagoId = (Guid?)p.MetodoPagoId,
                Metodo = (string?)p.MetodoPago.Nombre,
                p.NumeroOperacion,
                p.Monto
            };

        var reembolsos =
            from re in context.ReembolsosReserva.AsNoTracking()
            join r in context.Reservas.AsNoTracking()
                on re.ReservaId equals r.Id
            where
                r.OrganizacionId == filtro.OrganizacionId &&
                !re.EstaEliminado &&
                re.FechaReembolso >= filtro.FechaDesde &&
                re.FechaReembolso <= filtro.FechaHasta
            select new
            {
                Fecha = re.FechaReembolso,
                CodigoMovimiento = re.Codigo,
                CodigoReserva = r.Codigo,
                Cliente =
                    r.Cliente.Nombres + " " +
                    r.Cliente.Apellidos,
                SedeId = r.SedeId,
                Sede = r.Sede.Nombre,
                Tipo = "Reembolso",
                MetodoPagoId = re.MetodoPagoId,
                Metodo =
                    re.MetodoPago != null
                        ? re.MetodoPago.Nombre
                        : null,
                re.NumeroOperacion,
                re.Monto
            };

        if (filtro.SedeId.HasValue)
        {
            pagos = pagos.Where(x =>
                x.SedeId == filtro.SedeId.Value);

            reembolsos = reembolsos.Where(x =>
                x.SedeId == filtro.SedeId.Value);
        }

        if (filtro.MetodoPagoId.HasValue)
        {
            pagos = pagos.Where(x =>
                x.MetodoPagoId == filtro.MetodoPagoId.Value);

            reembolsos = reembolsos.Where(x =>
                x.MetodoPagoId == filtro.MetodoPagoId.Value);
        }

        var movimientos = await pagos
            .Concat(reembolsos)
            .OrderByDescending(x => x.Fecha)
            .ThenBy(x => x.CodigoMovimiento)
            .ToListAsync(ct);

        return movimientos
            .Select(x => new ReporteMovimientoFilaDto(
                x.Fecha,
                x.CodigoMovimiento,
                x.CodigoReserva,
                x.Cliente,
                x.Sede,
                x.Tipo,
                x.Metodo,
                x.NumeroOperacion,
                x.Monto))
            .ToList();
    }

    public async Task<IReadOnlyList<ReporteAtencionFilaDto>> ExportarAtencionesAsync(
        ReporteAtencionesFiltroDto filtro,
        CancellationToken ct = default)
    {
        var consulta = context.Reservas
            .AsNoTracking()
            .Where(r =>
                r.OrganizacionId == filtro.OrganizacionId &&
                r.Fecha >= filtro.FechaDesde &&
                r.Fecha <= filtro.FechaHasta);

        if (filtro.SedeId.HasValue)
            consulta = consulta.Where(r =>
                r.SedeId == filtro.SedeId.Value);

        if (filtro.ProfesionalId.HasValue)
            consulta = consulta.Where(r =>
                r.ProfesionalId == filtro.ProfesionalId.Value);

        if (filtro.ServicioId.HasValue)
            consulta = consulta.Where(r =>
                r.ServicioId == filtro.ServicioId.Value);

        if (filtro.Estado.HasValue)
            consulta = consulta.Where(r =>
                r.EstadoReserva == filtro.Estado.Value);

        if (filtro.Resultado.HasValue)
        {
            consulta = consulta.Where(r =>
                r.Atencion != null &&
                r.Atencion.ResultadoAtencion ==
                    filtro.Resultado.Value);
        }

        var datos = await consulta
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.HoraInicio)
            .ThenBy(r => r.Codigo)
            .Select(r => new
            {
                r.Id,
                r.Codigo,
                r.Fecha,
                r.HoraInicio,

                Cliente =
                    r.Cliente.Nombres + " " +
                    r.Cliente.Apellidos,

                Servicio = r.Servicio.Nombre,

                Profesional =
                    r.Profesional != null
                        ? r.Profesional.Nombres + " " +
                          r.Profesional.Apellidos
                        : null,

                HoraLlegada =
                    r.Atencion != null
                        ? r.Atencion.FechaHoraPresencia
                        : null,

                HoraInicioReal =
                    r.Atencion != null
                        ? r.Atencion.FechaHoraInicioReal
                        : null,

                HoraFinReal =
                    r.Atencion != null
                        ? r.Atencion.FechaHoraFinReal
                        : null,

                Resultado =
                    r.Atencion != null
                        ? r.Atencion.ResultadoAtencion
                        : null,

                Estado = r.EstadoReserva
            })
            .ToListAsync(ct);

        return datos
            .Select(x =>
            {
                int? duracionRealMinutos = null;

                if (x.HoraInicioReal.HasValue &&
                    x.HoraFinReal.HasValue)
                {
                    duracionRealMinutos =
                        (int)Math.Round(
                            (x.HoraFinReal.Value -
                             x.HoraInicioReal.Value)
                            .TotalMinutes);
                }

                return new ReporteAtencionFilaDto(
                    x.Id,
                    x.Codigo,
                    x.Fecha,
                    x.HoraInicio,
                    x.Cliente,
                    x.Servicio,
                    x.Profesional,
                    x.HoraLlegada,
                    x.HoraInicioReal,
                    x.HoraFinReal,
                    duracionRealMinutos,
                    x.Resultado?.ToString(),
                    x.Estado.ToString());
            })
            .ToList();
    }
}