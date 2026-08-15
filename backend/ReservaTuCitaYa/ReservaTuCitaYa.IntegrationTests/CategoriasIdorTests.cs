using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests
{
    public class CategoriasIdorTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public CategoriasIdorTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminA_NoDebePoderListarCategorias_DeOrganizacionB()
        {
            var (_, adminA) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "CatA");

            var (orgBId, _) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "CatB");

            var client = _factory.CreateHttpsClient();

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.UserIdHeader,
                adminA.Id);

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.RoleHeader,
                RoleNames.Administrador);

            var response =
                await client.GetAsync(
                    $"/api/organizaciones/{orgBId}/categorias");

            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Se esperaba 403 o 401, pero se obtuvo {response.StatusCode}");
        }

        [Fact]
        public async Task AdminA_NoDebePoderCrearCategoria_EnOrganizacionB()
        {
            var (_, adminA) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "CatC");

            var (orgBId, _) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "CatD");

            var client = _factory.CreateHttpsClient();

            await client.AutorizarComoAsync(
                adminA.Id,
                RoleNames.Administrador);

            var response = await client.PostAsJsonAsync(
                $"/api/organizaciones/{orgBId}/categorias",
                new
                {
                    nombre = "Categoria intrusa",
                    descripcion = "no deberia crearse"
                });

            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Se esperaba 403 o 401, pero se obtuvo {response.StatusCode}");
        }
    }
}