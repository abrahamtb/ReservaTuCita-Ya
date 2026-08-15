using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.IntegrationTests.Api;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class AtencionesIdorTests(
    ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task AdminA_NoDebePoderConsultarDetalleAtencion_DeOrganizacionB()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgAId = await CrearOrganizacionAsync(client, "IDA");
        var orgBId = await CrearOrganizacionAsync(client, "IDB");

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgBId,
                EstadoReserva.Confirmada,
                "IdorDetalle");

        // Creamos atención en Org B
        var presencia = await client.PostAsync(
            $"/api/organizaciones/{orgBId}/reservas/{reservaB.Id}/atencion/presencia",
            null);

        Assert.Equal(HttpStatusCode.OK, presencia.StatusCode);

        // Intentamos leerla usando Org A en la URL
        var response = await client.GetAsync(
            $"/api/organizaciones/{orgAId}/reservas/{reservaB.Id}/atencion");

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Se esperaba 404 o 403, pero se obtuvo {response.StatusCode}");
    }

    [Fact]
    public async Task AdminA_NoDebePoderMarcarPresente_ReservaDeOrganizacionB()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgAId = await CrearOrganizacionAsync(client, "IPA");
        var orgBId = await CrearOrganizacionAsync(client, "IPB");

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgBId,
                EstadoReserva.Confirmada,
                "IdorPresencia");

        var response = await client.PostAsync(
            $"/api/organizaciones/{orgAId}/reservas/{reservaB.Id}/atencion/presencia",
            null);

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Se esperaba 404 o 403, pero se obtuvo {response.StatusCode}");
    }

    [Fact]
    public async Task AdminA_NoDebePoderIniciarAtencion_DeOrganizacionB()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgAId = await CrearOrganizacionAsync(client, "IIA");
        var orgBId = await CrearOrganizacionAsync(client, "IIB");

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgBId,
                EstadoReserva.Presente,
                "IdorIniciar");

        var response = await client.PostAsync(
            $"/api/organizaciones/{orgAId}/reservas/{reservaB.Id}/atencion/iniciar",
            null);

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Se esperaba 404 o 403, pero se obtuvo {response.StatusCode}");
    }

    [Fact]
    public async Task AdminA_NoDebePoderMarcarNoAsistio_ReservaDeOrganizacionB()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgAId = await CrearOrganizacionAsync(client, "INA");
        var orgBId = await CrearOrganizacionAsync(client, "INB");

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgBId,
                EstadoReserva.Confirmada,
                "IdorNoAsistio");

        var response = await client.PostAsync(
            $"/api/organizaciones/{orgAId}/reservas/{reservaB.Id}/atencion/no-asistio",
            null);

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Se esperaba 404 o 403, pero se obtuvo {response.StatusCode}");
    }

    private HttpClient CrearClienteHttp() =>
        factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
                HandleCookies = true
            });

    private static async Task<Guid> CrearOrganizacionAsync(
        HttpClient client,
        string prefijo)
    {
        var tipos =
            await client.GetFromJsonAsync<JsonElement>(
                "/api/organizaciones/tipos");

        var tipoId = tipos
            .EnumerateArray()
            .First()
            .GetProperty("id")
            .GetGuid();

        var sufijo =
            Guid.NewGuid().ToString("N")[..8];

        var response =
            await client.PostAsJsonAsync(
                "/api/organizaciones",
                new
                {
                    tipoOrganizacionId = tipoId,
                    nombreComercial =
                        $"Org {prefijo} {sufijo}",
                    razonSocial =
                        $"Org {prefijo} {sufijo} SAC",
                    numeroDocumento =
                        $"20{sufijo[..6]}",
                    telefono = "999999999",
                    correo =
                        $"{prefijo.ToLower()}-{sufijo}@test.local",
                    direccionPrincipal =
                        "Direccion prueba IDOR",
                    logoUrl = (string?)null
                });

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        return json
            .GetProperty("id")
            .GetGuid();
    }
    [Fact]
    public async Task AdministradorDeOrgA_NoPuedeMarcarPresente_ReservaDeOrgB()
    {
        // ==========================================
        // 1. Superadmin prepara las organizaciones
        // ==========================================

        using var superAdminClient = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            superAdminClient,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgAId =
            await CrearOrganizacionAsync(
                superAdminClient,
                "REAL-A");

        var orgBId =
            await CrearOrganizacionAsync(
                superAdminClient,
                "REAL-B");

        // ==========================================
        // 2. Creamos Administrador exclusivo de A
        // ==========================================

        var adminA =
            await TestDataSeeder.CrearAdminDeOrganizacionAsync(
                factory.Services,
                orgAId,
                "OrgA");

        // ==========================================
        // 3. Reserva pertenece a B
        // ==========================================

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgBId,
                EstadoReserva.Confirmada,
                "ReservaOrgB");

        // ==========================================
        // 4. Nuevo cliente HTTP: iniciar sesión
        //    realmente como Admin A
        // ==========================================

        using var adminAClient = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            adminAClient,
            adminA.Email!);

        // ==========================================
        // 5. Admin A intenta operar sobre Org B
        // ==========================================

        var response = await adminAClient.PostAsync(
            $"/api/organizaciones/{orgBId}/reservas/{reservaB.Id}/atencion/presencia",
            null);

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Se esperaba 404 o 403, pero se obtuvo {response.StatusCode}");
    }
}