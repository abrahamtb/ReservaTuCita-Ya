using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.IntegrationTests
{
    public class ApplicationDbContextModelTests
    {
        private static readonly Type[] EntidadesDeNegocio =
        {
            typeof(TipoOrganizacion),
            typeof(Organizacion),
            typeof(Sede),
            typeof(CategoriaServicio),
            typeof(Servicio),
            typeof(ServicioSede)
        };

        [Fact]
        public void Modelo_PuedeCrearseYDetectaTodasLasConfiguraciones()
        {
            using var context = CrearContexto();

            foreach (var tipoEntidad in EntidadesDeNegocio)
            {
                var entidad = context.Model.FindEntityType(tipoEntidad);

                Assert.NotNull(entidad);
                Assert.NotNull(entidad.GetTableName());
                Assert.NotNull(entidad.GetQueryFilter());
            }
        }

        [Fact]
        public void RelacionesDeNegocio_RestringenEliminacionEnCascada()
        {
            using var context = CrearContexto();

            var clavesForaneas = EntidadesDeNegocio
                .Select(context.Model.FindEntityType)
                .Where(entidad => entidad is not null)
                .SelectMany(entidad => entidad!.GetForeignKeys())
                .ToList();

            Assert.NotEmpty(clavesForaneas);
            Assert.All(
                clavesForaneas,
                claveForanea => Assert.Equal(
                    DeleteBehavior.Restrict,
                    claveForanea.DeleteBehavior));
        }

        [Theory]
        [InlineData(typeof(Organizacion), "NumeroDocumento", false)]
        [InlineData(typeof(Sede), "OrganizacionId,Nombre", true)]
        [InlineData(typeof(CategoriaServicio), "OrganizacionId,Nombre", true)]
        [InlineData(typeof(Servicio), "OrganizacionId,Nombre", true)]
        [InlineData(typeof(ServicioSede), "ServicioId,SedeId", true)]
        public void IndicesUnicos_EstanConfigurados(
            Type tipoEntidad,
            string nombresPropiedades,
            bool debeSerFiltrado)
        {
            using var context = CrearContexto();
            var entidad = context.Model.FindEntityType(tipoEntidad)!;
            var propiedades = nombresPropiedades.Split(',');

            var indice = entidad.GetIndexes().Single(indice =>
                indice.Properties
                    .Select(propiedad => propiedad.Name)
                    .SequenceEqual(propiedades));

            Assert.True(indice.IsUnique);

            if (debeSerFiltrado)
            {
                Assert.Equal(
                    "[EstaActivo] = 1 AND [EstaEliminado] = 0",
                    indice.GetFilter());
            }
        }

        [Fact]
        public void Servicios_TienenRestriccionesCheckConfiguradas()
        {
            using var context = CrearContexto();
            var modelo = context.GetService<IDesignTimeModel>().Model;

            var restriccionesServicio = modelo
                .FindEntityType(typeof(Servicio))!
                .GetCheckConstraints()
                .Select(restriccion => restriccion.Name)
                .ToList();

            var restriccionesServicioSede = modelo
                .FindEntityType(typeof(ServicioSede))!
                .GetCheckConstraints()
                .Select(restriccion => restriccion.Name)
                .ToList();

            Assert.Contains("CK_Servicios_DuracionMinutos", restriccionesServicio);
            Assert.Contains("CK_Servicios_Precio", restriccionesServicio);
            Assert.Contains("CK_Servicios_MontoAdelanto", restriccionesServicio);
            Assert.Contains("CK_Servicios_CapacidadMaxima", restriccionesServicio);
            Assert.Contains("CK_Servicios_CapacidadIndividual", restriccionesServicio);
            Assert.Contains("CK_Servicios_TiemposNoNegativos", restriccionesServicio);
            Assert.Contains(
                "CK_ServiciosSede_PrecioEspecial",
                restriccionesServicioSede);
        }

        private static ApplicationDbContext CrearContexto()
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=ModeloReservaTuCitaYa;Trusted_Connection=True;")
                .Options;

            return new ApplicationDbContext(opciones);
        }
    }
}
