using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Dashboard;

namespace ReservaTuCitaYa.Application.Interfaces;

public interface IDashboardService
{
    Task<ResultadoOperacion<DashboardResumenDto>> ObtenerAsync(
        Guid organizacionId,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        Guid? sedeId,
        CancellationToken ct = default);
}