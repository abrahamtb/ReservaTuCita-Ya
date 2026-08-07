using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class ApiAuthenticationTests(ReservaTuCitaYaApiFactory factory)
    : IClassFixture<ReservaTuCitaYaApiFactory>
{
    [Fact]
    public async Task Organizaciones_sin_sesion_devuelve_401()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/api/organizaciones");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_incorrecto_devuelve_401()
    {
        using var client = CreateClient();
        await AddAntiforgeryAsync(client);
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = ReservaTuCitaYaApiFactory.AdminEmail,
            password = "Incorrecta123",
            recordarme = false
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_me_y_logout_mantienen_el_ciclo_de_cookie()
    {
        using var client = CreateClient();
        await LoginAsync(client, ReservaTuCitaYaApiFactory.AdminEmail);

        var me = await client.GetAsync("/api/auth/me");
        me.EnsureSuccessStatusCode();
        var json = await me.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(ReservaTuCitaYaApiFactory.AdminEmail, json.GetProperty("email").GetString());
        Assert.Contains("Superadministrador", json.GetProperty("roles").EnumerateArray()
            .Select(role => role.GetString()));

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Usuario_autenticado_sin_rol_recibe_403()
    {
        using var client = CreateClient();
        await LoginAsync(client, "sinrol.api@test.local");
        var response = await client.GetAsync("/api/organizaciones");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    internal static async Task LoginAsync(HttpClient client, string email)
    {
        await AddAntiforgeryAsync(client);
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = ReservaTuCitaYaApiFactory.AdminPassword,
            recordarme = false
        });
        response.EnsureSuccessStatusCode();
        await AddAntiforgeryAsync(client);
    }

    internal static async Task AddAntiforgeryAsync(HttpClient client)
    {
        var tokenResponse = await client.GetFromJsonAsync<JsonElement>("/api/antiforgery/token");
        var token = tokenResponse.GetProperty("requestToken").GetString();
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }
}
