using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.IntegrationTests.Api;
using ReservaTuCitaYa.IntegrationTests.Infrastructure;
using Xunit;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class AtencionesFlujoTests(
    ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task FlujoCompleto_Confirmada_Presente_EnAtencion_Atendida()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgId = await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgId,
                EstadoReserva.Confirmada,
                "Flujo");

        // Confirmada -> Presente
        var presencia = await client.PostAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/presencia",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            presencia.StatusCode);

        // Presente -> EnAtencion
        var iniciar = await client.PostAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/iniciar",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            iniciar.StatusCode);

        // EnAtencion -> Atendida
        var finalizar = await client.PostAsJsonAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/finalizar",
            new
            {
                resultado = "Completada",
                observaciones = "Prueba automatizada RG025",
                recomendaciones = "Control posterior",
                proximoServicioId = (Guid?)null,
                proximaFechaSugerida = (DateOnly?)null
            });

        Assert.Equal(
            HttpStatusCode.OK,
            finalizar.StatusCode);

        // ==============================
        // Verificar directamente en BD
        // ==============================

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var reservaDb = await db.Reservas
            .IgnoreQueryFilters()
            .SingleAsync(r => r.Id == reserva.Id);

        Assert.Equal(
            EstadoReserva.Atendida,
            reservaDb.EstadoReserva);

        var atencion = await db.Atenciones
            .IgnoreQueryFilters()
            .SingleAsync(a => a.ReservaId == reserva.Id);

        Assert.NotNull(atencion.FechaHoraPresencia);
        Assert.NotNull(atencion.FechaHoraInicioReal);
        Assert.NotNull(atencion.FechaHoraFinReal);

        Assert.Equal(
            ResultadoAtencion.Completada,
            atencion.ResultadoAtencion);

        var historial = await db.HistorialReservas
            .IgnoreQueryFilters()
            .Where(h => h.ReservaId == reserva.Id)
            .ToListAsync();

        Assert.Contains(
            historial,
            h => h.TipoAccion ==
                 TipoAccionReserva.MarcadaPresente);

        Assert.Contains(
            historial,
            h => h.TipoAccion ==
                 TipoAccionReserva.AtencionIniciada);

        Assert.Contains(
            historial,
            h => h.TipoAccion ==
                 TipoAccionReserva.AtencionFinalizada);
    }

    [Fact]
    public async Task Confirmada_PuedeMarcarseComoNoAsistio_SinCrearAtencion()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgId = await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgId,
                EstadoReserva.Confirmada,
                "NoAsistio");

        var response = await client.PostAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/no-asistio",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var reservaDb = await db.Reservas
            .IgnoreQueryFilters()
            .SingleAsync(r => r.Id == reserva.Id);

        Assert.Equal(
            EstadoReserva.NoAsistio,
            reservaDb.EstadoReserva);

        var existeAtencion = await db.Atenciones
            .IgnoreQueryFilters()
            .AnyAsync(a => a.ReservaId == reserva.Id);

        // NoAsistio NO debe crear una atención.
        Assert.False(existeAtencion);

        var historial = await db.HistorialReservas
            .IgnoreQueryFilters()
            .SingleAsync(h =>
                h.ReservaId == reserva.Id &&
                h.TipoAccion == TipoAccionReserva.NoAsistio);

        Assert.Equal(
            EstadoReserva.Confirmada,
            historial.EstadoAnterior);

        Assert.Equal(
            EstadoReserva.NoAsistio,
            historial.EstadoNuevo);
    }

    [Fact]
    public async Task NoPuedeIniciarAtencion_SiReservaNoEstaPresente()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgId = await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgId,
                EstadoReserva.Confirmada,
                "EstadoInvalido");

        var response = await client.PostAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/iniciar",
            null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task NoPuedeFinalizarAtencion_SiReservaNoEstaEnAtencion()
    {
        using var client = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgId = await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgId,
                EstadoReserva.Confirmada,
                "FinalizarInvalido");

        var response = await client.PostAsJsonAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/finalizar",
            new
            {
                resultado = "Completada",
                observaciones = "No debería finalizar",
                recomendaciones = (string?)null,
                proximoServicioId = (Guid?)null,
                proximaFechaSugerida = (DateOnly?)null
            });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task ProfesionalConPermiso_PuedeMarcarPresenteEnSuOrganizacion()
    {
        using var superAdminClient = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            superAdminClient,
            ReservaTuCitaYaApiFactory.AdminEmail);

        var orgId = await CrearOrganizacionAsync(superAdminClient);
        var profesional =
            await TestDataSeeder.CrearUsuarioDeOrganizacionAsync(
                factory.Services,
                orgId,
                "PermisoAtencion",
                RoleNames.Profesional);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                orgId,
                EstadoReserva.Confirmada,
                "Profesional");

        using var profesionalClient = CrearClienteHttp();

        await ApiAuthenticationTests.LoginAsync(
            profesionalClient,
            profesional.Email!);

        var response = await profesionalClient.PostAsync(
            $"/api/organizaciones/{orgId}/reservas/{reserva.Id}/atencion/presencia",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        HttpClient client)
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
                        $"Atenciones RG025 {sufijo}",
                    razonSocial =
                        $"Atenciones RG025 {sufijo} SAC",
                    numeroDocumento =
                        $"20{sufijo[..6]}",
                    telefono = "999999999",
                    correo =
                        $"rg025-{sufijo}@test.local",
                    direccionPrincipal =
                        "Direccion prueba RG025",
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
}
