using ReservaTuCitaYa.Application.DTOs.Pagos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Interfaces
{
    public interface IPagoService
    {
        Task<ResumenPagoReservaDto> ObtenerResumenAsync(Guid reservaId);

        Task<PagoDto> RegistrarPagoAsync(Guid reservaId, CrearPagoRequest request);

        Task<PagoDto> AnularPagoAsync(Guid pagoId, AnularPagoRequest request);

        Task<ReembolsoDto> RegistrarReembolsoAsync(Guid reservaId, RegistrarReembolsoRequest request);

        Task<IEnumerable<PagoDto>> ListarPagosAsync(Guid reservaId);

        Task<IEnumerable<ReembolsoDto>> ListarReembolsosAsync(Guid reservaId);
    }
}
