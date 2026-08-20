using ReservaTuCitaYa.Domain.Entities;


namespace ReservaTuCitaYa.Application.Interfaces.Repository
{
    public interface IPagoRepository
    {
        Task<IEnumerable<Pago>> ListarPorReservaAsync(Guid reservaId);
        Task<Pago?> ObtenerPorIdAsync(Guid id);
        Task AgregarAsync(Pago pago);
        Task ActualizarAsync(Pago pago);
    }
}
