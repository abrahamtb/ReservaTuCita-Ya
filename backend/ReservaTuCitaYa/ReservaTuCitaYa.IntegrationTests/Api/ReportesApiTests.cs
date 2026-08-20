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

public sealed class ReportesApiTests(
    ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    // =========================================================
    // 1. REPORTE RESERVAS
    // =========================================================

    [Fact]
    public async Task ReporteReservas_DevuelveIndicadoresCorrectos()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.Confirmada,
            "RepConfirmada");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.Atendida,
            "RepAtendida");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.NoAsistio,
            "RepNoAsistio");

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var indicadores =
            json.GetProperty("indicadores");

        Assert.Equal(
            3,
            indicadores
                .GetProperty("totalReservas")
                .GetInt32());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("confirmadasReprogramadas")
                .GetInt32());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("atendidas")
                .GetInt32());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("noAsistieron")
                .GetInt32());

        Assert.Equal(
            3,
            json.GetProperty("totalElementos")
                .GetInt32());
    }

    // =========================================================
    // 2. INGRESOS: 100 - 20 = 80
    // =========================================================

    [Fact]
    public async Task ReporteIngresos_Pago100_Reembolso20_Neto80()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "RepEconomico");

        await CrearPagoYReembolsoAsync(
            reserva.Id,
            100m,
            20m);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/ingresos" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var indicadores =
            json.GetProperty("indicadores");

        Assert.Equal(
            100m,
            indicadores
                .GetProperty("ingresosBrutos")
                .GetDecimal());

        Assert.Equal(
            20m,
            indicadores
                .GetProperty("reembolsos")
                .GetDecimal());

        Assert.Equal(
            80m,
            indicadores
                .GetProperty("ingresosNetos")
                .GetDecimal());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("cantidadPagos")
                .GetInt32());

        Assert.Equal(
            100m,
            indicadores
                .GetProperty("ticketPromedio")
                .GetDecimal());

        Assert.Equal(
            2,
            json.GetProperty("elementos")
                .GetArrayLength());
    }

    // =========================================================
    // 3. ATENCIONES: 50% ASISTENCIA
    // =========================================================

    [Fact]
    public async Task ReporteAtenciones_UnaAtendidaUnaNoAsistio_Asistencia50()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var atendida =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "RepAtendida50");

        await CrearAtencionCompletadaAsync(
            atendida.Id);

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionId,
            EstadoReserva.NoAsistio,
            "RepNoAsistio50");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/atenciones" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var indicadores =
            json.GetProperty("indicadores");

        Assert.Equal(
            2,
            indicadores
                .GetProperty("reservasProgramadas")
                .GetInt32());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("atendidas")
                .GetInt32());

        Assert.Equal(
            1,
            indicadores
                .GetProperty("noAsistieron")
                .GetInt32());

        Assert.Equal(
            50m,
            indicadores
                .GetProperty("porcentajeAsistencia")
                .GetDecimal());

        Assert.False(
            indicadores
                .GetProperty("sinDatos")
                .GetBoolean());
    }

    // =========================================================
    // 4. RANGO INVERTIDO
    // =========================================================

    [Fact]
    public async Task Reportes_RangoInvertido_DevuelveBadRequest()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde=2026-08-16" +
            $"&fechaHasta=2026-08-01" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // =========================================================
    // 5. MÁS DE 366 DÍAS
    // =========================================================

    [Fact]
    public async Task Reportes_PeriodoMayorA366Dias_DevuelveBadRequest()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde=2025-01-01" +
            $"&fechaHasta=2026-08-16" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // =========================================================
    // 6. TAMAÑO DE PÁGINA > 100
    // =========================================================

    [Fact]
    public async Task Reportes_TamanoPaginaMayorA100_DevuelveBadRequest()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=101" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // =========================================================
    // 7. ADMIN A NO PUEDE FORZAR ORGANIZACIÓN B
    // =========================================================

    [Fact]
    public async Task Reportes_AdminA_NoPuedeForzarOrganizacionB()
    {
        using var superClient = CrearCliente();

        await LoginSuperadminAsync(superClient);

        var organizacionA =
            await CrearOrganizacionAsync(superClient);

        var organizacionB =
            await CrearOrganizacionAsync(superClient);

        var adminA =
            await TestDataSeeder.CrearAdminDeOrganizacionAsync(
                factory.Services,
                organizacionA,
                "ReportesAdminA");

        await TestDataSeeder.CrearReservaParaAtencionAsync(
            factory.Services,
            organizacionB,
            EstadoReserva.Confirmada,
            "ReporteOrgB");

        using var adminClient = CrearCliente();

        await ApiAuthenticationTests.LoginAsync(
            adminClient,
            adminA.Email!);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await adminClient.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=10" +
            $"&organizacionId={organizacionB}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            0,
            json.GetProperty("totalElementos")
                .GetInt32());

        Assert.Equal(
            0,
            json.GetProperty("indicadores")
                .GetProperty("totalReservas")
                .GetInt32());
    }

    // =========================================================
    // 8. SUPERADMIN SIN ORGANIZACIÓN
    // =========================================================

    [Fact]
    public async Task Reportes_SuperadminSinOrganizacion_DevuelveForbidden()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=10");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    // =========================================================
    // 9. PAGINACIÓN
    // =========================================================

    [Fact]
    public async Task ReporteReservas_PaginacionFunciona()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        for (var i = 0; i < 3; i++)
        {
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Confirmada,
                $"Pagina{i}");
        }

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&pagina=1" +
            $"&tamanoPagina=2" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            2,
            json.GetProperty("elementos")
                .GetArrayLength());

        Assert.Equal(
            3,
            json.GetProperty("totalElementos")
                .GetInt32());

        Assert.Equal(
            2,
            json.GetProperty("totalPaginas")
                .GetInt32());
    }

    // =========================================================
    // 10. CSV RESERVAS
    // =========================================================

    [Fact]
    public async Task ExportarReservas_DevuelveCsv()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Confirmada,
                "CsvReserva");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas/exportar" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Contains(
            "text/csv",
            response.Content.Headers.ContentType?
                .ToString() ?? "");

        var csv =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Codigo;Fecha;Hora;Cliente",
            csv);

        Assert.Contains(
            reserva.Codigo,
            csv);
    }

    // =========================================================
    // 11. CSV INGRESOS
    // =========================================================

    [Fact]
    public async Task ExportarIngresos_DevuelvePagoYReembolso()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "CsvIngreso");

        await CrearPagoYReembolsoAsync(
            reserva.Id,
            100m,
            20m);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/ingresos/exportar" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var csv =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Fecha;CodigoMovimiento;CodigoReserva",
            csv);

        Assert.Contains(
            "Pago",
            csv);

        Assert.Contains(
            "Reembolso",
            csv);

        Assert.Contains(
            reserva.Codigo,
            csv);
    }

    // =========================================================
    // 12. CSV ATENCIONES
    // =========================================================

    [Fact]
    public async Task ExportarAtenciones_DevuelveResultadoCompletada()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var reserva =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Atendida,
                "CsvAtencion");

        await CrearAtencionCompletadaAsync(
            reserva.Id);

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/atenciones/exportar" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var csv =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "CodigoReserva;Fecha;HoraProgramada",
            csv);

        Assert.Contains(
            reserva.Codigo,
            csv);

        Assert.Contains(
            "Completada",
            csv);

        Assert.Contains(
            "Atendida",
            csv);
    }

    // =========================================================
    // 13. EXPORTACIÓN RESPETA FILTRO ESTADO
    // =========================================================

    [Fact]
    public async Task ExportarReservas_FiltroEstado_RespetaFiltro()
    {
        using var client = CrearCliente();

        await LoginSuperadminAsync(client);

        var organizacionId =
            await CrearOrganizacionAsync(client);

        var confirmada =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.Confirmada,
                "CsvConfirmada");

        var noAsistio =
            await TestDataSeeder.CrearReservaParaAtencionAsync(
                factory.Services,
                organizacionId,
                EstadoReserva.NoAsistio,
                "CsvNoAsistio");

        var hoy =
            DateOnly.FromDateTime(DateTime.Today);

        var response = await client.GetAsync(
            $"/api/reportes/reservas/exportar" +
            $"?fechaDesde={hoy:yyyy-MM-dd}" +
            $"&fechaHasta={hoy:yyyy-MM-dd}" +
            $"&estado={EstadoReserva.Confirmada}" +
            $"&organizacionId={organizacionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var csv =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            confirmada.Codigo,
            csv);

        Assert.DoesNotContain(
            noAsistio.Codigo,
            csv);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private HttpClient CrearCliente() =>
        factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress =
                    new Uri("https://localhost"),

                AllowAutoRedirect = false,
                HandleCookies = true
            });

    private static Task LoginSuperadminAsync(
        HttpClient client) =>
        ApiAuthenticationTests.LoginAsync(
            client,
            ReservaTuCitaYaApiFactory.AdminEmail);

    private async Task CrearPagoYReembolsoAsync(
        Guid reservaId,
        decimal montoPago,
        decimal montoReembolso)
    {
        using var scope =
            factory.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var metodoPago =
            await db.MetodosPago
                .AsNoTracking()
                .FirstAsync(m => m.EstaActivo);

        db.Pagos.Add(
            new Pago
            {
                Id = Guid.NewGuid(),

                Codigo =
                    $"PAG-R28-{Guid.NewGuid():N}"[..20],

                ReservaId = reservaId,
                MetodoPagoId = metodoPago.Id,

                Monto = montoPago,

                FechaPago =
                    DateOnly.FromDateTime(DateTime.Today),

                EstaAnulado = false,

                Observacion =
                    "Pago para pruebas RG028",

                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

        db.ReembolsosReserva.Add(
            new ReembolsoReserva
            {
                Id = Guid.NewGuid(),

                Codigo =
                    $"REM-R28-{Guid.NewGuid():N}"[..20],

                ReservaId = reservaId,
                MetodoPagoId = metodoPago.Id,

                Monto = montoReembolso,

                FechaReembolso =
                    DateOnly.FromDateTime(DateTime.Today),

                Motivo =
                    "Reembolso para pruebas RG028",

                Observacion =
                    "Prueba de reporte económico",

                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }

    private async Task CrearAtencionCompletadaAsync(
        Guid reservaId)
    {
        using var scope =
            factory.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var inicio =
            DateTime.Today.AddHours(10);

        db.Atenciones.Add(
            new Atencion
            {
                Id = Guid.NewGuid(),
                ReservaId = reservaId,

                FechaHoraPresencia =
                    inicio.AddMinutes(-5),

                FechaHoraInicioReal =
                    inicio,

                FechaHoraFinReal =
                    inicio.AddMinutes(30),

                ResultadoAtencion =
                    ResultadoAtencion.Completada,

                Observaciones =
                    "Atención de prueba RG028",

                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }

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
                        $"Reportes Test {sufijo}",

                    razonSocial =
                        $"Reportes Test {sufijo} SAC",

                    numeroDocumento =
                        $"20{sufijo[..6]}",

                    telefono =
                        "999999999",

                    correo =
                        $"reportes-{sufijo}@test.local",

                    direccionPrincipal =
                        "Direccion de prueba Reportes",

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