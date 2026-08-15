using System.Net;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests
{
    public class SedesIdorTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public SedesIdorTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminA_NoDebePoderListarSedes_DeOrganizacionB()
        {
            var (orgAId, adminA) = await TestDataSeeder.CrearOrganizacionConAdminAsync(_factory.Services, "A");
            var (orgBId, _) = await TestDataSeeder.CrearOrganizacionConAdminAsync(_factory.Services, "B");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, adminA.Id);
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, RoleNames.Administrador);

            var response = await client.GetAsync($"/api/organizaciones/{orgBId}/sedes");

            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Se esperaba 403 o 401, pero se obtuvo {response.StatusCode}");
        }

        [Fact]
        public async Task AdminA_SiDebePoderListarSedes_DeSuPropiaOrganizacion()
        {
            var (orgAId, adminA) = await TestDataSeeder.CrearOrganizacionConAdminAsync(_factory.Services, "C");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, adminA.Id);
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, RoleNames.Administrador);

            var response = await client.GetAsync($"/api/organizaciones/{orgAId}/sedes");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}