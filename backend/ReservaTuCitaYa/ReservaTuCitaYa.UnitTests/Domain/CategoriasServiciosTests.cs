using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Domain
{
    public class CategoriasServiciosTests
    {
        [Fact]
        public void Categoria_PuedeAsociarseAUnaOrganizacion()
        {
            var organizacion = CrearOrganizacion();
            var categoria = CrearCategoria(organizacion);

            organizacion.CategoriasServicio.Add(categoria);

            Assert.Same(organizacion, categoria.Organizacion);
            Assert.Equal(organizacion.Id, categoria.OrganizacionId);
            Assert.Contains(categoria, organizacion.CategoriasServicio);
        }

        [Fact]
        public void Servicio_PuedeAsociarseAUnaCategoria()
        {
            var organizacion = CrearOrganizacion();
            var categoria = CrearCategoria(organizacion);
            var servicio = CrearServicio(organizacion, categoria);

            categoria.Servicios.Add(servicio);

            Assert.Same(categoria, servicio.CategoriaServicio);
            Assert.Equal(categoria.Id, servicio.CategoriaServicioId);
            Assert.Contains(servicio, categoria.Servicios);
        }

        [Fact]
        public void Servicio_PuedeAsociarseAUnaOrganizacion()
        {
            var organizacion = CrearOrganizacion();
            var servicio = CrearServicio(organizacion, CrearCategoria(organizacion));

            organizacion.Servicios.Add(servicio);

            Assert.Same(organizacion, servicio.Organizacion);
            Assert.Equal(organizacion.Id, servicio.OrganizacionId);
            Assert.Contains(servicio, organizacion.Servicios);
        }

        [Fact]
        public void ServicioSede_PuedeAsociarUnServicioYUnaSede()
        {
            var organizacion = CrearOrganizacion();
            var servicio = CrearServicio(organizacion, CrearCategoria(organizacion));
            var sede = new Sede
            {
                OrganizacionId = organizacion.Id,
                Organizacion = organizacion,
                Nombre = "Sede Centro",
                Direccion = "Av. Principal 123"
            };
            var servicioSede = new ServicioSede
            {
                ServicioId = servicio.Id,
                Servicio = servicio,
                SedeId = sede.Id,
                Sede = sede,
                PrecioEspecial = 45m
            };

            servicio.ServiciosSede.Add(servicioSede);
            sede.ServiciosSede.Add(servicioSede);

            Assert.Same(servicio, servicioSede.Servicio);
            Assert.Same(sede, servicioSede.Sede);
            Assert.Contains(servicioSede, servicio.ServiciosSede);
            Assert.Contains(servicioSede, sede.ServiciosSede);
        }

        [Fact]
        public void EntidadesNuevas_GeneranId()
        {
            Assert.NotEqual(Guid.Empty, new CategoriaServicio().Id);
            Assert.NotEqual(Guid.Empty, new Servicio().Id);
            Assert.NotEqual(Guid.Empty, new ServicioSede().Id);
        }

        [Fact]
        public void EntidadesNuevas_EstanActivasYNoEliminadas()
        {
            BaseEntity[] entidades =
            {
                new CategoriaServicio(),
                new Servicio(),
                new ServicioSede()
            };

            Assert.All(entidades, entidad =>
            {
                Assert.True(entidad.EstaActivo);
                Assert.False(entidad.EstaEliminado);
            });
        }

        [Fact]
        public void ColeccionesNuevas_EstanInicializadasYVacias()
        {
            var organizacion = new Organizacion();
            var categoria = new CategoriaServicio();
            var servicio = new Servicio();
            var sede = new Sede();

            Assert.Empty(organizacion.CategoriasServicio);
            Assert.Empty(organizacion.Servicios);
            Assert.Empty(categoria.Servicios);
            Assert.Empty(servicio.ServiciosSede);
            Assert.Empty(sede.ServiciosSede);
        }

        [Fact]
        public void ServicioIndividual_TieneCapacidadUnoPorDefecto()
        {
            var servicio = new Servicio();

            Assert.False(servicio.EsGrupal);
            Assert.Equal(1, servicio.CapacidadMaxima);
        }

        [Fact]
        public void ValoresPrincipales_PuedenAsignarseCorrectamente()
        {
            var organizacion = CrearOrganizacion();
            var categoria = CrearCategoria(organizacion);
            var servicio = CrearServicio(organizacion, categoria);

            Assert.Equal("Corte clásico", servicio.Nombre);
            Assert.Equal(30, servicio.DuracionMinutos);
            Assert.Equal(50m, servicio.Precio);
            Assert.Equal(10m, servicio.MontoAdelanto);
            Assert.Equal(ModalidadServicio.Presencial, servicio.Modalidad);
            Assert.True(servicio.PermiteCancelacion);
            Assert.True(servicio.PermiteReprogramacion);
        }

        private static Organizacion CrearOrganizacion()
        {
            return new Organizacion
            {
                NombreComercial = "Barbería Central",
                NumeroDocumento = "20123456789"
            };
        }

        private static CategoriaServicio CrearCategoria(Organizacion organizacion)
        {
            return new CategoriaServicio
            {
                OrganizacionId = organizacion.Id,
                Organizacion = organizacion,
                Nombre = "Cabello"
            };
        }

        private static Servicio CrearServicio(
            Organizacion organizacion,
            CategoriaServicio categoria)
        {
            return new Servicio
            {
                OrganizacionId = organizacion.Id,
                Organizacion = organizacion,
                CategoriaServicioId = categoria.Id,
                CategoriaServicio = categoria,
                Nombre = "Corte clásico",
                DuracionMinutos = 30,
                Precio = 50m,
                MontoAdelanto = 10m,
                Modalidad = ModalidadServicio.Presencial,
                CapacidadMaxima = 1,
                PermiteCancelacion = true,
                PermiteReprogramacion = true,
                HorasLimiteCancelacion = 2
            };
        }
    }
}
