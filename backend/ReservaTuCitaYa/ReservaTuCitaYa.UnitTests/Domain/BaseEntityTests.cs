using ReservaTuCitaYa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.UnitTests.Domain
{
    public class EntidadDePrueba : BaseEntity
    {
    }

    public class BaseEntityTests
    {
        [Fact]
        public void AlCrear_DebeGenerarUnIdAutomaticamente()
        {
            var entidad = new EntidadDePrueba();

            Assert.NotEqual(Guid.Empty, entidad.Id);
        }

        [Fact]
        public void AlCrear_EstaActivoDebeSerTrueYEstaEliminadoDebeSerFalse()
        {
            var entidad = new EntidadDePrueba();

            Assert.True(entidad.EstaActivo);
            Assert.False(entidad.EstaEliminado);
        }

        [Fact]
        public void AlCrear_FechaCreacionDebeAsignarseAutomaticamente()
        {
            var antes = DateTime.UtcNow;
            var entidad = new EntidadDePrueba();
            var despues = DateTime.UtcNow;

            Assert.InRange(entidad.FechaCreacion, antes, despues);
        }

        [Fact]
        public void AlCrear_FechaModificacionDebeSerNula()
        {
            var entidad = new EntidadDePrueba();

            Assert.Null(entidad.FechaModificacion);
        }
    }
}

