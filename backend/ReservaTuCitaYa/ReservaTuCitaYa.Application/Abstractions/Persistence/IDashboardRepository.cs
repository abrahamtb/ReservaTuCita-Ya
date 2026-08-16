using ReservaTuCitaYa.Application.DTOs.Dashboard;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IDashboardRepository
{
    Task<DashboardResumenDto> ObtenerAsync(
        DashboardFiltroDto filtro,
        CancellationToken ct = default);
}