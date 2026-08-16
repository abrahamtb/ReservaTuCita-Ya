using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class DashboardMetricasTests(
    ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task Dashboard_Pago100_Reembolso20_IngresoNeto80()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "Economico");

        // =========================
        // Insertar pago y reembolso
        // =========================
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var metodoPago =
                await db.MetodosPago
                    .AsNoTracking()
                    .FirstAsync(m => m.EstaActivo);

            db.Pagos.Add(new Pago
            {
                Id = Guid.NewGuid(),
                Codigo = $"PAG-TEST-{Guid.NewGuid():N}"[..20],
                ReservaId = reserva.Id,
                MetodoPagoId = metodoPago.Id,
                Monto = 100m,
                FechaPago =
                    DateOnly.FromDateTime(DateTime.Today),
                EstaAnulado = false,
                Observacion = "Pago Dashboard RG027",
                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

            db.ReembolsosReserva.Add(
                new ReembolsoReserva
                {
                    Id = Guid.NewGuid(),
                    Codigo =
                        $"REM-TEST-{Guid.NewGuid():N}"[..20],
                    ReservaId = reserva.Id,
                    MetodoPagoId = metodoPago.Id,
                    Monto = 20m,
                    FechaReembolso =
                        DateOnly.FromDateTime(DateTime.Today),
                    Motivo = "Reembolso de prueba RG027",
                    Observacion =
                        "Validación Dashboard",
                    EstaActivo = true,
                    EstaEliminado = false,
                    FechaCreacion = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        }

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var ingresosNetos =
            json.GetProperty("ingresosNetos");

        Assert.Equal(
            80m,
            ingresosNetos
                .GetProperty("valorActual")
                .GetDecimal());

        var ingresosPorDia =
            json.GetProperty("ingresosPorDia");

        Assert.Equal(
            1,
            ingresosPorDia.GetArrayLength());

        var dia =
            ingresosPorDia[0];

        Assert.Equal(
            100m,
            dia.GetProperty("ingresosBrutos")
                .GetDecimal());

        Assert.Equal(
            20m,
            dia.GetProperty("reembolsos")
                .GetDecimal());

        Assert.Equal(
            80m,
            dia.GetProperty("ingresosNetos")
                .GetDecimal());
    }

    [Fact]
    public async Task Dashboard_PagoAnulado_NoCuentaComoIngreso()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "Anulado");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var metodoPago =
                await db.MetodosPago
                    .AsNoTracking()
                    .FirstAsync(m => m.EstaActivo);

            db.Pagos.Add(new Pago
            {
                Id = Guid.NewGuid(),
                Codigo = $"PAG-ANU-{Guid.NewGuid():N}"[..20],
                ReservaId = reserva.Id,
                MetodoPagoId = metodoPago.Id,
                Monto = 500m,
                FechaPago =
                    DateOnly.FromDateTime(DateTime.Today),

                EstaAnulado = true,
                FechaAnulacion = DateTime.UtcNow,
                MotivoAnulacion =
                    "Pago anulado para prueba RG027",

                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            0m,
            json.GetProperty("ingresosNetos")
                .GetProperty("valorActual")
                .GetDecimal());
    }

    [Fact]
    public async Task Dashboard_ReservaConfirmada_AparecePorAtenderYPorEstado()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.Confirmada,
            "Confirmada");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            json.GetProperty("reservasHoy")
                .GetProperty("valorActual")
                .GetInt32());

        Assert.Equal(
            1,
            json.GetProperty("porAtenderHoy")
                .GetProperty("valorActual")
                .GetInt32());

        var estados =
            json.GetProperty("reservasPorEstado")
                .EnumerateArray()
                .ToList();

        var confirmada =
            estados.Single(
                x => x.GetProperty("estado")
                    .GetString() == "Confirmada");

        Assert.Equal(
            1,
            confirmada
                .GetProperty("cantidad")
                .GetInt32());
    }

    [Fact]
    public async Task Dashboard_TopServicios_ContieneServicioDeReserva()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Confirmada,
                "TopServicio");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var top =
            json.GetProperty("topServicios");

        Assert.Equal(
            1,
            top.GetArrayLength());

        Assert.Equal(
            reserva.ServicioId,
            top[0]
                .GetProperty("servicioId")
                .GetGuid());

        Assert.Equal(
            1,
            top[0]
                .GetProperty("cantidadReservas")
                .GetInt32());

        Assert.Equal(
            100m,
            top[0]
                .GetProperty("porcentajeSobreTotal")
                .GetDecimal());
    }

    [Fact]
    public async Task Dashboard_NoMezclaDatosEntreOrganizaciones()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionA =
            await CrearOrganizacionAsync(client);

        var organizacionB =
            await CrearOrganizacionAsync(client);

        // La única reserva pertenece a B.
        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionB,
            EstadoReserva.Confirmada,
            "OrgB");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        // Consultamos A.
        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionA}");

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
            json.GetProperty("topServicios")
                .GetArrayLength());
    }

    [Fact]
    public async Task Dashboard_FiltroSede_SoloIncluyeLaSedeSeleccionada()
    {
        using var client = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reservaSedeA =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Confirmada,
                "SedeA");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.Confirmada,
            "SedeB");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&sedeId={reservaSedeA.SedeId}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            json.GetProperty("reservasHoy")
                .GetProperty("valorActual")
                .GetInt32());

        Assert.Equal(
            reservaSedeA.SedeId,
            json.GetProperty("sedeId")
                .GetGuid());
    }

    [Fact]
    public async Task Dashboard_AdminDeOrganizacion_PuedeConsultarSuDashboard()
    {
        using var superClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            superClient,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionId =
            await CrearOrganizacionAsync(superClient);

        var admin =
            await TestDataSeeder.CrearAdminDeOrganizacionAsync(
                factory.Services,
                organizacionId,
                "DashboardAdmin");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.Confirmada,
            "AdminOrg");

        using var adminClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            adminClient,
            admin.Email!);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await adminClient.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            json.GetProperty("reservasHoy")
                .GetProperty("valorActual")
                .GetInt32());

        Assert.Equal(
            1,
            json.GetProperty("porAtenderHoy")
                .GetProperty("valorActual")
                .GetInt32());
    }

    [Fact]
    public async Task Dashboard_AdminA_NoPuedeForzarOrganizacionB()
    {
        using var superClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            superClient,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionA =
            await CrearOrganizacionAsync(superClient);

        var organizacionB =
            await CrearOrganizacionAsync(superClient);

        var adminA =
            await TestDataSeeder.CrearAdminDeOrganizacionAsync(
                factory.Services,
                organizacionA,
                "DashboardAdminA");

        // Datos solamente de B
        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionB,
            EstadoReserva.Confirmada,
            "DashboardOrgB");

        using var adminClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            adminClient,
            adminA.Email!);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await adminClient.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionB}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        // Debe seguir usando la organización A del usuario.
        Assert.Equal(
            0,
            json.GetProperty("reservasHoy")
                .GetProperty("valorActual")
                .GetInt32());

        Assert.Equal(
            0,
            json.GetProperty("topServicios")
                .GetArrayLength());
    }

    [Fact]
    public async Task Dashboard_AdminA_NoPuedeUsarSedeDeOrganizacionB()
    {
        using var superClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            superClient,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var organizacionA =
            await CrearOrganizacionAsync(superClient);

        var organizacionB =
            await CrearOrganizacionAsync(superClient);

        var adminA =
            await TestDataSeeder.CrearAdminDeOrganizacionAsync(
                factory.Services,
                organizacionA,
                "DashboardSedeA");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionA,
            EstadoReserva.Confirmada,
            "ReservaA");

        var reservaB =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionB,
                EstadoReserva.Confirmada,
                "ReservaB");

        using var adminClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            adminClient,
            adminA.Email!);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await adminClient.GetAsync(
            $"/api/dashboard" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&sedeId={reservaB.SedeId}");

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
            json.GetProperty("topServicios")
                .GetArrayLength());
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
                        $"Dashboard Metricas {sufijo}",

                    razonSocial =
                        $"Dashboard Metricas {sufijo} SAC",

                    numeroDocumento =
                        $"20{sufijo[..6]}",

                    telefono =
                        "999999999",

                    correo =
                        $"metricas-{sufijo}@test.local",

                    direccionPrincipal =
                        "Direccion Dashboard RG027",

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