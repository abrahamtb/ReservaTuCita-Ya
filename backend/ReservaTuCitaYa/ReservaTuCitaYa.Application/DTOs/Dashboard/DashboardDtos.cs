using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Dashboard;

public sealed record DashboardFiltroDto(
    Guid OrganizacionId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    Guid? SedeId);

public sealed record DashboardKpiDto(
    int ValorActual,
    int ValorAnterior,
    decimal? VariacionPorcentaje,
    bool SinBaseComparacion);

public sealed record DashboardKpiDecimalDto(
    decimal ValorActual,
    decimal ValorAnterior,
    decimal? VariacionPorcentaje,
    bool SinBaseComparacion);

public sealed record ReservaPorDiaDto(
    DateOnly Fecha,
    int Cantidad);

public sealed record ReservaPorEstadoDto(
    string Estado,
    int Cantidad);

public sealed record IngresoPorDiaDto(
    DateOnly Fecha,
    decimal IngresosBrutos,
    decimal Reembolsos,
    decimal IngresosNetos);

public sealed record TopServicioDto(
    Guid ServicioId,
    string Nombre,
    int CantidadReservas,
    decimal PorcentajeSobreTotal);

public sealed record ProximaReservaDto(
    Guid ReservaId,
    string Codigo,
    TimeOnly HoraInicio,
    string Cliente,
    string Servicio,
    string? Profesional,
    string Estado);

public sealed record DashboardResumenDto(
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    Guid? SedeId,
    DateTime FechaHoraConsulta,

    DashboardKpiDto ReservasHoy,
    DashboardKpiDto PorAtenderHoy,
    DashboardKpiDto AtencionesCompletadas,
    DashboardKpiDto Cancelaciones,
    DashboardKpiDto ClientesNuevos,
    DashboardKpiDecimalDto IngresosNetos,

    IReadOnlyList<ReservaPorDiaDto> ReservasPorDia,
    IReadOnlyList<ReservaPorEstadoDto> ReservasPorEstado,
    IReadOnlyList<IngresoPorDiaDto> IngresosPorDia,
    IReadOnlyList<TopServicioDto> TopServicios,
    IReadOnlyList<ProximaReservaDto> ProximasReservas);
