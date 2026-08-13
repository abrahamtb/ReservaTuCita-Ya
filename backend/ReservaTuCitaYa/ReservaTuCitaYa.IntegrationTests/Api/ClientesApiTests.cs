using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class ClientesApiTests(ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task Clientes_SinSesionDevuelve401()
    {
        using var client = CrearClienteHttp();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/clientes/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Clientes_CompletanCrudBusquedaFiltrosPaginacionYProblemDetails()
    {
        using var client = CrearClienteHttp();
        await ApiAuthenticationTests.LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);
        var organizacionId = await CrearOrganizacionAsync(client);
        var request = new
        {
            tipoDocumento = "DNI",
            numeroDocumento = "76543210",
            nombres = "Ana",
            apellidos = "López",
            correo = "ana@test.local",
            telefono = "999123456",
            direccion = "Lima",
            fechaNacimiento = "1998-06-10",
            observaciones = "Cliente frecuente"
        };

        var creado = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizacionId}/clientes", request);
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var creadoJson = await creado.Content.ReadFromJsonAsync<JsonElement>();
        var clienteId = creadoJson.GetProperty("id").GetGuid();
        Assert.Equal(organizacionId, creadoJson.GetProperty("organizacionId").GetGuid());

        var duplicado = await client.PostAsJsonAsync(
            $"/api/organizaciones/{organizacionId}/clientes", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        var problema = await duplicado.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("client-document-duplicate", problema.GetProperty("type").GetString());
        Assert.Equal("Cliente duplicado", problema.GetProperty("title").GetString());

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync(
                $"/api/organizaciones/{Guid.NewGuid()}/clientes", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/clientes/{Guid.NewGuid()}")).StatusCode);

        var detalle = await client.GetFromJsonAsync<JsonElement>($"/api/clientes/{clienteId}");
        Assert.Equal("Ana López", detalle.GetProperty("nombreCompleto").GetString());

        var lista = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones/{organizacionId}/clientes?busqueda=999123&tipoDocumento=DNI&estado=Activos&pagina=1&tamanoPagina=1");
        Assert.Equal(1, lista.GetProperty("totalElementos").GetInt32());
        Assert.Equal(1, lista.GetProperty("tamanoPagina").GetInt32());

        var actualizado = await client.PutAsJsonAsync($"/api/clientes/{clienteId}", new
        {
            tipoDocumento = "Pasaporte",
            numeroDocumento = "P-123456",
            nombres = "Ana María",
            apellidos = "López",
            correo = (string?)null,
            telefono = "999123456",
            direccion = "Arequipa",
            fechaNacimiento = "1998-06-10",
            observaciones = "Actualizada"
        });
        Assert.Equal(HttpStatusCode.NoContent, actualizado.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PatchAsJsonAsync($"/api/clientes/{clienteId}/estado",
                new { estaActivo = false })).StatusCode);
        var inactivos = await client.GetFromJsonAsync<JsonElement>(
            $"/api/organizaciones/{organizacionId}/clientes?estado=Inactivos");
        Assert.Equal(1, inactivos.GetProperty("totalElementos").GetInt32());

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/clientes/{clienteId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/clientes/{clienteId}")).StatusCode);
    }

    private HttpClient CrearClienteHttp() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task<Guid> CrearOrganizacionAsync(HttpClient client)
    {
        var tipos = await client.GetFromJsonAsync<JsonElement>("/api/organizaciones/tipos");
        var tipoId = tipos.EnumerateArray().First().GetProperty("id").GetGuid();
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync("/api/organizaciones", new
        {
            tipoOrganizacionId = tipoId,
            nombreComercial = $"Clientes API {sufijo}",
            numeroDocumento = $"30{sufijo[..6]}"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }
}
