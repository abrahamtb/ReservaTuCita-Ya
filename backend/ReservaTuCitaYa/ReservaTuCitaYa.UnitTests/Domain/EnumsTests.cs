using ReservaTuCitaYa.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.UnitTests.Domain
{
    public class EnumsTests
    {
        [Theory]
        [InlineData(typeof(TipoDocumento))]
        [InlineData(typeof(ModalidadServicio))]
        [InlineData(typeof(EstadoReserva))]
        [InlineData(typeof(EstadoPago))]
        [InlineData(typeof(EstadoRecurso))]
        [InlineData(typeof(DiaSemana))]
        [InlineData(typeof(TipoBloqueo))]
        public void TodosLosEnums_DebenTenerValorCeroDefinido(Type tipoEnum)
        {
            var valores = Enum.GetValues(tipoEnum);
            var primerValor = (int)valores.GetValue(0)!;

            Assert.Equal(0, primerValor);
        }
    }
}
