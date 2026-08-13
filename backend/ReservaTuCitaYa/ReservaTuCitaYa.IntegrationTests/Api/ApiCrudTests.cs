using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class ApiCrudTests(ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task Organizacion_y_sede_completan_crud_y_controlan_duplicados()
    {
        using var client = CreateClient();
        await ApiAuthenticationTests.LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);
        var tipoId = await GetFirstIdAsync(client, "/api/organizaciones/tipos");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var organizationRequest = new
        {
            tipoOrganizacionId = tipoId,
            nombreComercial = $"Organización API {suffix}",
            razonSocial = $"Organización API {suffix} SAC",
            numeroDocumento = $"20{suffix[..6]}",
            telefono = "999111222",
            correo = $"org-{suffix}@test.local",
            direccionPrincipal = "Lima",
            logoUrl = (string?)null
        };

        var created = await client.PostAsJsonAsync("/api/organizaciones", organizationRequest);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var organizationId = (await created.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        var duplicate = await client.PostAsJsonAsync("/api/organizaciones", organizationRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var page = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones?busqueda={suffix}&pagina=1&tamanoPagina=5");
        Assert.True(page.GetProperty("totalElementos").GetInt32() >= 1);

        var sedeCreated = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizationId}/sedes",
            new
            {
                organizacionId = Guid.NewGuid(),
                nombre = $"Sede {suffix}",
                direccion = "Av. Prueba 123",
                telefono = "999111333",
                correo = $"sede-{suffix}@test.local",
                referencia = "Frente al parque"
            });
        Assert.Equal(HttpStatusCode.Created, sedeCreated.StatusCode);
        var sedeJson = await sedeCreated.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(organizationId, sedeJson.GetProperty("organizacionId").GetGuid());
        var sedeId = sedeJson.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PatchAsync($"/api/sedes/{sedeId}/estado", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/sedes/{sedeId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/organizaciones/{organizationId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/organizaciones/{organizationId}")).StatusCode);
    }

    [Fact]
    public async Task Categoria_y_servicio_validan_reglas_sedes_y_precio_especial()
    {
        using var client = CreateClient();
        await ApiAuthenticationTests.LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);
        var tipoId = await GetFirstIdAsync(client, "/api/organizaciones/tipos");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var organizationId = await CreateOrganizationAsync(client, tipoId, suffix);
        var sedeId = await CreateSedeAsync(client, organizationId, suffix);

        var categoryCreated = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizationId}/categorias",
            new { organizacionId = Guid.NewGuid(), nombre = $"Categoría {suffix}", descripcion = "Prueba API" });
        Assert.Equal(HttpStatusCode.Created, categoryCreated.StatusCode);
        var categoryId = (await categoryCreated.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        var duplicate = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizationId}/categorias",
            new { nombre = $"Categoría {suffix}", descripcion = "Duplicada" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var invalidService = ServiceRequest(categoryId, sedeId, suffix, 10m, 20m);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync(
                $"/api/organizaciones/{organizationId}/servicios", invalidService)).StatusCode);

        var serviceCreated = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizationId}/servicios",
            ServiceRequest(categoryId, sedeId, suffix, 100m, 20m));
        Assert.Equal(HttpStatusCode.Created, serviceCreated.StatusCode);
        var serviceJson = await serviceCreated.Content.ReadFromJsonAsync<JsonElement>();
        var serviceId = serviceJson.GetProperty("id").GetGuid();
        var assignedSede = Assert.Single(serviceJson.GetProperty("sedes").EnumerateArray());
        Assert.Equal(80m, assignedSede.GetProperty("precioEspecial").GetDecimal());

        var filtered = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones/{organizationId}/servicios?categoriaServicioId={categoryId}&modalidad=Presencial");
        Assert.True(filtered.GetProperty("totalElementos").GetInt32() >= 1);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PatchAsync($"/api/servicios/{serviceId}/estado", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/servicios/{serviceId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/categorias/{categoryId}")).StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task<Guid> GetFirstIdAsync(HttpClient client, string url)
    {
        var items = await client.GetFromJsonAsync<JsonElement>(url);
        return items.EnumerateArray().First().GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateOrganizationAsync(HttpClient client, Guid tipoId, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/organizaciones", new
        {
            tipoOrganizacionId = tipoId,
            nombreComercial = $"Servicios API {suffix}",
            numeroDocumento = $"10{suffix[..6]}"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateSedeAsync(HttpClient client, Guid organizationId, string suffix)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizationId}/sedes",
            new { nombre = $"Sede servicios {suffix}", direccion = "Av. Servicios 456" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static object ServiceRequest(
        Guid categoryId,
        Guid sedeId,
        string suffix,
        decimal price,
        decimal advance) => new
    {
        categoriaServicioId = categoryId,
        nombre = $"Servicio {suffix}",
        descripcion = "Servicio creado desde pruebas HTTP",
        duracionMinutos = 60,
        precio = price,
        montoAdelanto = advance,
        modalidad = "Presencial",
        esGrupal = false,
        capacidadMaxima = 1,
        requiereProfesional = true,
        requiereRecurso = false,
        permiteCancelacion = true,
        permiteReprogramacion = true,
        horasLimiteCancelacion = 24,
        tiempoPreparacionMinutos = 5,
        tiempoPosteriorMinutos = 5,
        sedes = new[] { new { sedeId, precioEspecial = (decimal?)80m } }
    };
}
