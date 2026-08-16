using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ReservaTuCitaYa.IntegrationTests.Api;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests.Api
{


    public sealed class DashboardApiTests(
        ReservaTuCitaYaApiFactory factory)
        : IClassFixture<ReservaTuCitaYaApiFactory>
    {
        [Fact]
        public async Task Dashboard_SinOrganizacion_Superadmin_DevuelveBadRequest()
        {
            using var client = CrearCliente();

            await ApiAuthenticationTests.LoginAsync(
                client,
                ReservaTuCitaYaApiFactory.AdminEmail);

            var response = await client.GetAsync(
                "/api/dashboard?fechaDesde=2026-08-01&fechaHasta=2026-08-16");

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Dashboard_RangoInvalido_DevuelveBadRequest()
        {
            using var client = CrearCliente();

            await ApiAuthenticationTests.LoginAsync(
                client,
                ReservaTuCitaYaApiFactory.AdminEmail);

            var organizacionId =
                await CrearOrganizacionAsync(client);

            var response = await client.GetAsync(
                $"/api/dashboard" +
                $"?fechaDesde=2026-08-16" +
                $"&fechaHasta=2026-08-01" +
                $"&organizacionId={organizacionId}");

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Dashboard_OrganizacionSinDatos_DevuelveCeros()
        {
            using var client = CrearCliente();

            await ApiAuthenticationTests.LoginAsync(
                client,
                ReservaTuCitaYaApiFactory.AdminEmail);

            var organizacionId =
                await CrearOrganizacionAsync(client);

            var response = await client.GetAsync(
                $"/api/dashboard" +
                $"?fechaDesde=2026-08-01" +
                $"&fechaHasta=2026-08-16" +
                $"&organizacionId={organizacionId}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var json =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>();

            Assert.Equal(
                0,
                json.GetProperty("reservasHoy")
                    .GetProperty("valorActual")
                    .GetInt32());

            Assert.Equal(
                0,
                json.GetProperty("porAtenderHoy")
                    .GetProperty("valorActual")
                    .GetInt32());

            Assert.Equal(
                0,
                json.GetProperty("atencionesCompletadas")
                    .GetProperty("valorActual")
                    .GetInt32());

            Assert.Equal(
                0,
                json.GetProperty("cancelaciones")
                    .GetProperty("valorActual")
                    .GetInt32());

            Assert.Equal(
                0m,
                json.GetProperty("ingresosNetos")
                    .GetProperty("valorActual")
                    .GetDecimal());
        }

        [Fact]
        public async Task Dashboard_IncluyeTodosLosDiasDelPeriodo()
        {
            using var client = CrearCliente();

            await ApiAuthenticationTests.LoginAsync(
                client,
                ReservaTuCitaYaApiFactory.AdminEmail);

            var organizacionId =
                await CrearOrganizacionAsync(client);

            var response = await client.GetAsync(
                $"/api/dashboard" +
                $"?fechaDesde=2026-08-01" +
                $"&fechaHasta=2026-08-07" +
                $"&organizacionId={organizacionId}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var json =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>();

            var reservasPorDia =
                json.GetProperty("reservasPorDia");

            var ingresosPorDia =
                json.GetProperty("ingresosPorDia");

            Assert.Equal(
                7,
                reservasPorDia.GetArrayLength());

            Assert.Equal(
                7,
                ingresosPorDia.GetArrayLength());
        }

        [Fact]
        public async Task Dashboard_PeriodoMayorA366Dias_DevuelveBadRequest()
        {
            using var client = CrearCliente();

            await ApiAuthenticationTests.LoginAsync(
                client,
                ReservaTuCitaYaApiFactory.AdminEmail);

            var organizacionId =
                await CrearOrganizacionAsync(client);

            var response = await client.GetAsync(
                $"/api/dashboard" +
                $"?fechaDesde=2025-01-01" +
                $"&fechaHasta=2026-08-16" +
                $"&organizacionId={organizacionId}");

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        private HttpClient CrearCliente() =>
            factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress =
                        new Uri("https://localhost"),

                    AllowAutoRedirect = false,
                    HandleCookies = true
                });

        private static async Task<Guid> CrearOrganizacionAsync(
            HttpClient client)
        {
            var tipos =
                await client.GetFromJsonAsync<JsonElement>(
                    "/api/organizaciones/tipos");

            var tipoId =
                tipos.EnumerateArray()
                    .First()
                    .GetProperty("id")
                    .GetGuid();

            var sufijo =
                Guid.NewGuid()
                    .ToString("N")[..8];

            var response =
                await client.PostAsJsonAsync(
                    "/api/organizaciones",
                    new
                    {
                        tipoOrganizacionId = tipoId,
                        nombreComercial =
                            $"Dashboard Test {sufijo}",
                        razonSocial =
                            $"Dashboard Test {sufijo} SAC",
                        numeroDocumento =
                            $"20{sufijo[..6]}",
                        telefono =
                            "999999999",
                        correo =
                            $"dashboard-{sufijo}@test.local",
                        direccionPrincipal =
                            "Direccion de prueba Dashboard",
                        logoUrl =
                            (string?)null
                    });

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>();

            return json
                .GetProperty("id")
                .GetGuid();
        }
    }
}    

