// Application/Abstractions/Persistence/IRecursoRepository.cs
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Domain.Entities;
namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IRecursoRepository
{
    Task<PaginaResultado<RecursoListaDto>> ListarAsync(
        RecursoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<RecursoDetalleDto?> ObtenerDetalleAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<Recurso?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoAsync(
        Guid sedeId, string codigo, Guid? excluirId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Servicio>> ObtenerServiciosParaValidarAsync(
        Guid sedeId, IReadOnlyCollection<Guid> servicioIds,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServicioRecurso>> ObtenerRelacionesServicioAsync(
        Guid recursoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecursoServicioDto>> ListarServiciosAsync(
        Guid recursoId, CancellationToken cancellationToken = default);
    void Agregar(Recurso recurso);
    void AgregarRelacion(ServicioRecurso relacion);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion, CancellationToken cancellationToken = default);
}