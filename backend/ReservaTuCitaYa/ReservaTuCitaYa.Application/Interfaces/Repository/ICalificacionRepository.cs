using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Application.Interfaces.Repository;

public interface ICalificacionRepository
{
    Task<Calificacion?> ObtenerPorReservaAsync(Guid reservaId);
    Task<Reserva?> ObtenerReservaParaCalificarAsync(Guid reservaId);
    Task<Empleado?> ObtenerProfesionalAsync(Guid profesionalId);
    Task<IReadOnlyCollection<Calificacion>> ListarPorProfesionalAsync(Guid profesionalId, int pagina, int tamanoPagina, int? puntuacion);
    Task<int> ContarPorProfesionalAsync(Guid profesionalId, int? puntuacion = null);
    Task CrearAsync(Calificacion calificacion);
    Task<bool> ExistePorReservaAsync(Guid reservaId);
    Task<bool> ExistePorAtencionAsync(Guid atencionId);
    Task GuardarCambiosAsync();
}
