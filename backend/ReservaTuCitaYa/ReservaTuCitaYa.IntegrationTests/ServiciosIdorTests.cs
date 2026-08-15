using System.Net;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests
{
    public class ServiciosIdorTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public ServiciosIdorTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminA_NoDebePoderListarServicios_DeOrganizacionB()
        {
            var (_, adminA) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "SrvA");

            var (orgBId, _) =
                await TestDataSeeder.CrearOrganizacionConAdminAsync(
                    _factory.Services,
                    "SrvB");

            var client = _factory.CreateHttpsClient();

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.UserIdHeader,
                adminA.Id);

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.RoleHeader,
                RoleNames.Administrador);

            var response =
                await client.GetAsync(
                    $"/api/organizaciones/{orgBId}/servicios");

            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Se esperaba 403 o 401, pero se obtuvo {response.StatusCode}");
        }
    }
}