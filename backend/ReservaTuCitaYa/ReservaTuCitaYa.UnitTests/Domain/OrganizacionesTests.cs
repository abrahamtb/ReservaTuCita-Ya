using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.UnitTests.Domain
{
    public class OrganizacionesTests
    {
        [Fact]
        public void Organizacion_PuedeAsociarseAUnTipoDeOrganizacion()
        {
            var tipo = new TipoOrganizacion { Nombre = "Consultorio" };
            var organizacion = new Organizacion
            {
                TipoOrganizacionId = tipo.Id,
                TipoOrganizacion = tipo,
                NombreComercial = "Consultorio Central",
                NumeroDocumento = "20123456789"
            };

            tipo.Organizaciones.Add(organizacion);

            Assert.Same(tipo, organizacion.TipoOrganizacion);
            Assert.Equal(tipo.Id, organizacion.TipoOrganizacionId);
            Assert.Contains(organizacion, tipo.Organizaciones);
        }

        [Fact]
        public void Sede_PuedeAsociarseAUnaOrganizacion()
        {
            var organizacion = CrearOrganizacionValida();
            var sede = new Sede
            {
                OrganizacionId = organizacion.Id,
                Organizacion = organizacion,
                Nombre = "Sede Centro",
                Direccion = "Av. Principal 123"
            };

            organizacion.Sedes.Add(sede);

            Assert.Same(organizacion, sede.Organizacion);
            Assert.Equal(organizacion.Id, sede.OrganizacionId);
            Assert.Contains(sede, organizacion.Sedes);
        }

        [Fact]
        public void EntidadesNuevas_GeneranId()
        {
            Assert.NotEqual(Guid.Empty, new TipoOrganizacion().Id);
            Assert.NotEqual(Guid.Empty, new Organizacion().Id);
            Assert.NotEqual(Guid.Empty, new Sede().Id);
        }

        [Fact]
        public void EntidadesNuevas_EstanActivasYNoEliminadas()
        {
            var entidades = new ReservaTuCitaYa.Domain.Common.BaseEntity[]
            {
                new TipoOrganizacion(),
                new Organizacion(),
                new Sede()
            };

            Assert.All(entidades, entidad =>
            {
                Assert.True(entidad.EstaActivo);
                Assert.False(entidad.EstaEliminado);
            });
        }

        [Fact]
        public void PropiedadesObligatorias_PuedenRepresentarUnaEstructuraValida()
        {
            var organizacion = CrearOrganizacionValida();
            var sede = new Sede
            {
                OrganizacionId = organizacion.Id,
                Organizacion = organizacion,
                Nombre = "Sede Norte",
                Direccion = "Calle Norte 456"
            };

            Assert.False(string.IsNullOrWhiteSpace(organizacion.TipoOrganizacion.Nombre));
            Assert.False(string.IsNullOrWhiteSpace(organizacion.NombreComercial));
            Assert.False(string.IsNullOrWhiteSpace(organizacion.NumeroDocumento));
            Assert.False(string.IsNullOrWhiteSpace(sede.Nombre));
            Assert.False(string.IsNullOrWhiteSpace(sede.Direccion));
        }

        private static Organizacion CrearOrganizacionValida()
        {
            var tipo = new TipoOrganizacion { Nombre = "Centro deportivo" };

            return new Organizacion
            {
                TipoOrganizacionId = tipo.Id,
                TipoOrganizacion = tipo,
                NombreComercial = "Centro Deportivo Norte",
                NumeroDocumento = "20987654321"
            };
        }
    }
}
