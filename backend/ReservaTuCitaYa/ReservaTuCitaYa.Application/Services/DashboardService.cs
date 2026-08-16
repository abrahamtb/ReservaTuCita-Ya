using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Dashboard;
using ReservaTuCitaYa.Application.Interfaces;

namespace ReservaTuCitaYa.Application.Services;

public sealed class DashboardService(
    IDashboardRepository dashboardRepository)
    : IDashboardService
{
    public const string FechaDesdeObligatoria =
        "La fecha desde es obligatoria.";

    public const string FechaHastaObligatoria =
        "La fecha hasta es obligatoria.";

    public const string RangoFechasInvalido =
        "La fecha hasta no puede ser menor que la fecha desde.";

    public const string RangoDemasiadoAmplio =
        "El rango máximo permitido para el dashboard es de 366 días.";

    public async Task<ResultadoOperacion<DashboardResumenDto>> ObtenerAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        CancellationToken ct = default)
    {
        if (organizacionId == Guid.Empty)
        {
            return ResultadoOperacion<DashboardResumenDto>
                .Fallo(
                    "No se pudo determinar la organización del usuario.",
                   TipoErrorOperacion.Validacion);
        }

        if (fechaHasta < fechaDesde)
        {
            return ResultadoOperacion<DashboardResumenDto>
                .Fallo(
                    RangoFechasInvalido,
                    TipoErrorOperacion.Validacion);
        }

        var cantidadDias =
            fechaHasta.DayNumber - fechaDesde.DayNumber + 1;

        if (cantidadDias > 366)
        {
            return ResultadoOperacion<DashboardResumenDto>
                .Fallo(
                    RangoDemasiadoAmplio,
                    TipoErrorOperacion.Validacion);
        }

        var filtro = new DashboardFiltroDto(
            organizacionId,
            fechaDesde,
            fechaHasta,
            sedeId);

        var dashboard =
            await dashboardRepository.ObtenerAsync(
                filtro,
                ct);

        return ResultadoOperacion<DashboardResumenDto>
            .Exito(dashboard);
    }
}