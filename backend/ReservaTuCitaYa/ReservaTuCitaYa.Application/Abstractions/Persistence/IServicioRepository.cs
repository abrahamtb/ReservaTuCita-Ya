using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IServicioRepository
{
    Task<PaginaResultado<ServicioListaDto>> ListarAsync(
        ServicioFiltroDto filtro,
        CancellationToken cancellationToken = default);

    Task<ServicioDetalleDto?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Servicio?> ObtenerParaModificarAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreActivoAsync(
        Guid organizacionId,
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SedeAsignacionDto>> ListarSedesParaAsignarAsync(
        Guid organizacionId,
        Guid? servicioId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(
        IReadOnlyCollection<Guid> sedeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicioSede>> ObtenerRelacionesSedeAsync(
        Guid servicioId,
        CancellationToken cancellationToken = default);

    Task<ServicioSede?> ObtenerServicioSedeAsync(Guid servicioId, Guid sedeId, CancellationToken cancellationToken = default);

    void Agregar(Servicio servicio);
    void AgregarRelacion(ServicioSede servicioSede);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default);
}