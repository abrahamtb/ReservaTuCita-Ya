using ReservaTuCitaYa.Application.DTOs.Pagos;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services
{
    public sealed class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IReembolsoRepository _reembolsoRepository;

        public PagoService(IPagoRepository pagoRepository, IReembolsoRepository reembolsoRepository)
        {
            _pagoRepository = pagoRepository;
            _reembolsoRepository = reembolsoRepository;
        }

        public async Task<ResumenPagoReservaDto> ObtenerResumenAsync(Guid reservaId)
        {
            var pagos = await _pagoRepository.ListarPorReservaAsync(reservaId);
            var reembolsos = await _reembolsoRepository.ListarPorReservaAsync(reservaId);

            decimal totalPagadoBruto = pagos.Where(p => !p.EstaAnulado).Sum(p => p.Monto);
            decimal totalReembolsado = reembolsos.Sum(r => r.Monto);
            decimal totalPagadoNeto = totalPagadoBruto - totalReembolsado;
            decimal precioTotal = 0;
            decimal adelantoRequerido = 0;
            decimal saldoPendiente = precioTotal - totalPagadoNeto;

            EstadoPagoReserva estado = EstadoPagoReserva.SinPago;
            if (totalPagadoNeto == 0 && totalReembolsado > 0) estado = EstadoPagoReserva.Reembolsado;
            else if (totalPagadoNeto >= precioTotal) estado = EstadoPagoReserva.Pagado;
            else if (totalPagadoNeto > 0 && totalPagadoNeto < precioTotal) estado = EstadoPagoReserva.Parcial;

            return new ResumenPagoReservaDto
            {
                ReservaId = reservaId,
                CodigoReserva = string.Empty,
                PrecioTotal = precioTotal,
                AdelantoRequerido = adelantoRequerido,
                TotalPagadoBruto = totalPagadoBruto,
                TotalReembolsado = totalReembolsado,
                TotalPagadoNeto = totalPagadoNeto,
                SaldoPendiente = saldoPendiente,
                EstadoPago = estado,
                Pagos = pagos.Select(p => new PagoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    ReservaId = p.ReservaId,
                    MetodoPago = p.MetodoPago.Nombre,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    NumeroOperacion = p.NumeroOperacion,
                    Observacion = p.Observacion,
                    EstaAnulado = p.EstaAnulado
                }),
                Reembolsos = reembolsos.Select(r => new ReembolsoDto
                {
                    Id = r.Id,
                    Codigo = r.Codigo,
                    ReservaId = r.ReservaId,
                    MetodoPago = r.MetodoPago?.Nombre,
                    Monto = r.Monto,
                    FechaReembolso = r.FechaReembolso,
                    NumeroOperacion = r.NumeroOperacion,
                    Motivo = r.Motivo,
                    Observacion = r.Observacion
                })
            };
        }

        public async Task<PagoDto> RegistrarPagoAsync(Guid reservaId, CrearPagoRequest request)
        {
            var pago = new Pago
            {
                Id = Guid.NewGuid(),
                Codigo = $"PAG-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6)}",
                ReservaId = reservaId,
                MetodoPagoId = request.MetodoPagoId,
                Monto = request.Monto,
                FechaPago = request.FechaPago,
                NumeroOperacion = request.NumeroOperacion,
                Observacion = request.Observacion
            };

            await _pagoRepository.AgregarAsync(pago);

            return new PagoDto
            {
                Id = pago.Id,
                Codigo = pago.Codigo,
                ReservaId = pago.ReservaId,
                MetodoPago = pago.MetodoPago?.Nombre ?? string.Empty,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                NumeroOperacion = pago.NumeroOperacion,
                Observacion = pago.Observacion,
                EstaAnulado = pago.EstaAnulado
            };
        }

        public async Task<PagoDto> AnularPagoAsync(Guid pagoId, AnularPagoRequest request)
        {
            var pago = await _pagoRepository.ObtenerPorIdAsync(pagoId);
            if (pago is null) throw new Exception("Pago no encontrado.");

            pago.EstaAnulado = true;
            pago.FechaAnulacion = DateTime.UtcNow;
            pago.MotivoAnulacion = request.Motivo;

            await _pagoRepository.ActualizarAsync(pago);

            return new PagoDto
            {
                Id = pago.Id,
                Codigo = pago.Codigo,
                ReservaId = pago.ReservaId,
                MetodoPago = pago.MetodoPago?.Nombre ?? string.Empty,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                NumeroOperacion = pago.NumeroOperacion,
                Observacion = pago.Observacion,
                EstaAnulado = pago.EstaAnulado
            };
        }

        public async Task<ReembolsoDto> RegistrarReembolsoAsync(Guid reservaId, RegistrarReembolsoRequest request)
        {
            var reembolso = new ReembolsoReserva
            {
                Id = Guid.NewGuid(),
                Codigo = $"REM-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6)}",
                ReservaId = reservaId,
                MetodoPagoId = request.MetodoPagoId,
                Monto = request.Monto,
                FechaReembolso = request.FechaReembolso,
                NumeroOperacion = request.NumeroOperacion,
                Motivo = request.Motivo,
                Observacion = request.Observacion
            };

            await _reembolsoRepository.AgregarAsync(reembolso);

            return new ReembolsoDto
            {
                Id = reembolso.Id,
                Codigo = reembolso.Codigo,
                ReservaId = reembolso.ReservaId,
                MetodoPago = reembolso.MetodoPago?.Nombre,
                Monto = reembolso.Monto,
                FechaReembolso = reembolso.FechaReembolso,
                NumeroOperacion = reembolso.NumeroOperacion,
                Motivo = reembolso.Motivo,
                Observacion = reembolso.Observacion
            };
        }

        public async Task<IEnumerable<PagoDto>> ListarPagosAsync(Guid reservaId)
        {
            var pagos = await _pagoRepository.ListarPorReservaAsync(reservaId);

            return pagos.Select(p => new PagoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                ReservaId = p.ReservaId,
                MetodoPago = p.MetodoPago?.Nombre ?? string.Empty,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                NumeroOperacion = p.NumeroOperacion,
                Observacion = p.Observacion,
                EstaAnulado = p.EstaAnulado
            });
        }

        public async Task<IEnumerable<ReembolsoDto>> ListarReembolsosAsync(Guid reservaId)
        {
            var reembolsos = await _reembolsoRepository.ListarPorReservaAsync(reservaId);

            return reembolsos.Select(r => new ReembolsoDto
            {
                Id = r.Id,
                Codigo = r.Codigo,
                ReservaId = r.ReservaId,
                MetodoPago = r.MetodoPago?.Nombre,
                Monto = r.Monto,
                FechaReembolso = r.FechaReembolso,
                NumeroOperacion = r.NumeroOperacion,
                Motivo = r.Motivo,
                Observacion = r.Observacion
            });
        }
    }
}
