using ReservaTuCitaYa.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.DTOs.Pagos
{
    public sealed class CrearPagoRequest
    {
        public Guid MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
        public DateOnly FechaPago { get; set; }
        public string? NumeroOperacion { get; set; }
        public string? Observacion { get; set; }
    }

    public sealed class AnularPagoRequest
    {
        public string Motivo { get; set; } = string.Empty;
    }

    public sealed class RegistrarReembolsoRequest
    {
        public Guid MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
        public DateOnly FechaReembolso { get; set; }
        public string? NumeroOperacion { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    public sealed class PagoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public Guid ReservaId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateOnly FechaPago { get; set; }
        public string? NumeroOperacion { get; set; }
        public string? Observacion { get; set; }
        public bool EstaAnulado { get; set; }
    }

    public sealed class ReembolsoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public Guid ReservaId { get; set; }
        public string? MetodoPago { get; set; }
        public decimal Monto { get; set; }
        public DateOnly FechaReembolso { get; set; }
        public string? NumeroOperacion { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    public sealed class ResumenPagoReservaDto
    {
        public Guid ReservaId { get; set; }
        public string CodigoReserva { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public decimal AdelantoRequerido { get; set; }
        public decimal TotalPagadoBruto { get; set; }
        public decimal TotalReembolsado { get; set; }
        public decimal TotalPagadoNeto { get; set; }
        public decimal SaldoPendiente { get; set; }
        public EstadoPagoReserva EstadoPago { get; set; }
        public IEnumerable<PagoDto> Pagos { get; set; } = new List<PagoDto>();
        public IEnumerable<ReembolsoDto> Reembolsos { get; set; } = new List<ReembolsoDto>();
    }


}
