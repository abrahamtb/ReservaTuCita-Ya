using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Abstractions.Persistence;

public interface IEmpleadoRepository
{
    Task<PaginaResultado<EmpleadoListaDto>> ListarAsync(
        EmpleadoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<EmpleadoDetalleDto?> ObtenerDetalleAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<Empleado?> ObtenerParaModificarAsync(
        Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteDocumentoAsync(
        Guid organizacionId, TipoDocumento tipoDocumento, string numeroDocumento,
        Guid? excluirId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Servicio>> ObtenerServiciosParaValidarAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpleadoSede>> ObtenerRelacionesSedeAsync(
        Guid empleadoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfesionalServicio>> ObtenerRelacionesServicioAsync(
        Guid empleadoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpleadoSedeDto>> ListarSedesAsync(
        Guid empleadoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfesionalServicioDto>> ListarServiciosAsync(
        Guid empleadoId, CancellationToken cancellationToken = default);
    Task<bool> TieneServiciosActivosAsync(
        Guid empleadoId, CancellationToken cancellationToken = default);
    void Agregar(Empleado empleado);
    void AgregarRelacion(EmpleadoSede relacion);
    void AgregarRelacion(ProfesionalServicio relacion);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default);
}
