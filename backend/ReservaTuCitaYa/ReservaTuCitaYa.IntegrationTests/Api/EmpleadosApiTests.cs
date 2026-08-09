using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class EmpleadosApiTests(ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task Empleados_SinSesionDevuelve401()
    {
        using var client = ClienteHttp();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/empleados/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Empleados_UsuarioSinRolDevuelve403()
    {
        using var client = ClienteHttp();
        await ApiAuthenticationTests.LoginAsync(client, "sinrol.api@test.local");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.GetAsync($"/api/empleados/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Empleados_EscrituraSinAntiforgeryDevuelve400()
    {
        using var client = ClienteHttp();
        await ApiAuthenticationTests.LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        var response = await client.PostAsJsonAsync(
            $"/api/organizaciones/{Guid.NewGuid()}/empleados", EmpleadoRequest());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Empleados_CompletanCrudRelacionesFiltrosYConflictos()
    {
        using var client = ClienteHttp();
        await ApiAuthenticationTests.LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);
        var a = await PrepararOrganizacionAsync(client, "A");
        var b = await PrepararOrganizacionAsync(client, "B");
        var request = EmpleadoRequest(true, [a.SedeId], [a.ServicioId]);

        var creado = await client.PostAsJsonAsync(
            $"/api/organizaciones/{a.OrganizacionId}/empleados", request);
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var creadoJson = await creado.Content.ReadFromJsonAsync<JsonElement>();
        var empleadoId = creadoJson.GetProperty("id").GetGuid();
        Assert.Equal(a.OrganizacionId, creadoJson.GetProperty("organizacionId").GetGuid());
        Assert.True(creadoJson.GetProperty("esProfesional").GetBoolean());
        Assert.Single(creadoJson.GetProperty("sedes").EnumerateArray());
        Assert.Single(creadoJson.GetProperty("servicios").EnumerateArray());

        var duplicado = await client.PostAsJsonAsync(
            $"/api/organizaciones/{a.OrganizacionId}/empleados", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        var problemaDuplicado = await duplicado.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("employee-document-duplicate",
            problemaDuplicado.GetProperty("type").GetString());

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync(
                $"/api/organizaciones/{Guid.NewGuid()}/empleados", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/empleados/{Guid.NewGuid()}")).StatusCode);

        var listado = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones/{a.OrganizacionId}/empleados?busqueda=Barbero&esProfesional=true&sedeId={a.SedeId}&servicioId={a.ServicioId}&estado=Activos&pagina=1&tamanoPagina=1");
        Assert.Equal(1, listado.GetProperty("totalElementos").GetInt32());
        var profesionales = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones/{a.OrganizacionId}/empleados?esProfesional=true");
        Assert.All(profesionales.GetProperty("elementos").EnumerateArray(),
            e => Assert.True(e.GetProperty("esProfesional").GetBoolean()));

        Assert.Single((await client.GetFromJsonAsync<JsonElement>(
            $"/api/empleados/{empleadoId}/sedes")).EnumerateArray());
        Assert.Single((await client.GetFromJsonAsync<JsonElement>(
            $"/api/empleados/{empleadoId}/servicios")).EnumerateArray());
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync($"/api/empleados/{empleadoId}/sedes",
                new { sedeIds = new[] { a.SedeId } })).StatusCode);
        var sedeAjena = await client.PutAsJsonAsync($"/api/empleados/{empleadoId}/sedes",
            new { sedeIds = new[] { b.SedeId } });
        Assert.Equal(HttpStatusCode.Conflict, sedeAjena.StatusCode);
        Assert.Equal("employee-site-organization-mismatch",
            (await sedeAjena.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("type").GetString());

        var servicioAjeno = await client.PutAsJsonAsync(
            $"/api/empleados/{empleadoId}/servicios",
            new { servicioIds = new[] { b.ServicioId } });
        Assert.Equal(HttpStatusCode.Conflict, servicioAjeno.StatusCode);
        Assert.Equal("professional-service-organization-mismatch",
            (await servicioAjeno.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("type").GetString());

        var noProfesionalCreado = await client.PostAsJsonAsync(
            $"/api/organizaciones/{a.OrganizacionId}/empleados",
            EmpleadoRequest(false, [], [], "87654321"));
        Assert.Equal(HttpStatusCode.Created, noProfesionalCreado.StatusCode);
        var noProfesionalId = (await noProfesionalCreado.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
        var asignacionNoProfesional = await client.PutAsJsonAsync(
            $"/api/empleados/{noProfesionalId}/servicios",
            new { servicioIds = new[] { a.ServicioId } });
        Assert.Equal(HttpStatusCode.Conflict, asignacionNoProfesional.StatusCode);
        Assert.Equal("employee-not-professional",
            (await asignacionNoProfesional.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("type").GetString());

        var quitarProfesional = await client.PutAsJsonAsync(
            $"/api/empleados/{empleadoId}", EmpleadoRequest(false, [], []));
        Assert.Equal(HttpStatusCode.Conflict, quitarProfesional.StatusCode);
        Assert.Equal("employee-has-professional-services",
            (await quitarProfesional.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("type").GetString());

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync($"/api/empleados/{empleadoId}/servicios",
                new { servicioIds = Array.Empty<Guid>() })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync($"/api/empleados/{empleadoId}",
                EmpleadoRequest(false, [], []))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PatchAsJsonAsync($"/api/empleados/{empleadoId}/estado",
                new { estaActivo = false })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/empleados/{empleadoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/empleados/{empleadoId}")).StatusCode);
    }

    private HttpClient ClienteHttp() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static object EmpleadoRequest(
        bool profesional = false,
        IReadOnlyList<Guid>? sedeIds = null,
        IReadOnlyList<Guid>? servicioIds = null,
        string documento = "71234567") => new
    {
        tipoDocumento = "DNI",
        numeroDocumento = documento,
        nombres = "Carlos",
        apellidos = "Ramirez Soto",
        correo = "carlos@test.local",
        telefono = "987654321",
        direccion = "Lima",
        fechaNacimiento = "1995-04-10",
        cargo = "Barbero",
        especialidad = profesional ? "Corte y barba" : null,
        esProfesional = profesional,
        numeroColegiatura = (string?)null,
        observaciones = "Prueba API",
        sedeIds = sedeIds ?? [],
        servicioIds = servicioIds ?? []
    };

    private static async Task<DatosApi> PrepararOrganizacionAsync(HttpClient client, string nombre)
    {
        var tipos = await client.GetFromJsonAsync<JsonElement>("/api/organizaciones/tipos");
        var tipoId = tipos.EnumerateArray().First().GetProperty("id").GetGuid();
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var org = await client.PostAsJsonAsync("/api/organizaciones", new
        {
            tipoOrganizacionId = tipoId,
            nombreComercial = $"Empleados {nombre} {sufijo}",
            numeroDocumento = $"40{sufijo[..6]}"
        });
        org.EnsureSuccessStatusCode();
        var orgId = (await org.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var sede = await client.PostAsJsonAsync($"/api/organizaciones/{orgId}/sedes", new
        {
            nombre = $"Sede {nombre} {sufijo}", direccion = "Lima"
        });
        sede.EnsureSuccessStatusCode();
        var sedeId = (await sede.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var categoria = await client.PostAsJsonAsync($"/api/organizaciones/{orgId}/categorias", new
        {
            nombre = $"Categoría {nombre} {sufijo}"
        });
        categoria.EnsureSuccessStatusCode();
        var categoriaId = (await categoria.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
        var servicio = await client.PostAsJsonAsync($"/api/organizaciones/{orgId}/servicios",
            new
            {
                categoriaServicioId = categoriaId,
                nombre = $"Servicio {nombre} {sufijo}",
                duracionMinutos = 30,
                precio = 50m,
                montoAdelanto = 0m,
                modalidad = "Presencial",
                esGrupal = false,
                capacidadMaxima = 1,
                requiereProfesional = true,
                requiereRecurso = false,
                permiteCancelacion = true,
                permiteReprogramacion = true,
                horasLimiteCancelacion = 24,
                tiempoPreparacionMinutos = 0,
                tiempoPosteriorMinutos = 0,
                sedes = Array.Empty<object>()
            });
        servicio.EnsureSuccessStatusCode();
        var servicioId = (await servicio.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
        return new(orgId, sedeId, servicioId);
    }

    private sealed record DatosApi(Guid OrganizacionId, Guid SedeId, Guid ServicioId);
}
