using System.Net.Http.Json;

namespace ReservaTuCitaYa.IntegrationTests.Infrastructure
{
    public static class TestClientExtensions
    {
        public static async Task AutorizarComoAsync(
            this HttpClient client,
            string userId,
            string rol)
        {
            client.DefaultRequestHeaders.Remove(
                TestAuthHandler.UserIdHeader);

            client.DefaultRequestHeaders.Remove(
                TestAuthHandler.RoleHeader);

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.UserIdHeader,
                userId);

            client.DefaultRequestHeaders.Add(
                TestAuthHandler.RoleHeader,
                rol);

            var response =
                await client.GetAsync("/api/antiforgery/token");

            response.EnsureSuccessStatusCode();

            var payload =
                await response.Content
                    .ReadFromJsonAsync<AntiforgeryTokenPayload>();

            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.RequestToken))
            {
                throw new InvalidOperationException(
                    "No se pudo obtener un token antiforgery válido.");
            }

            client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

            client.DefaultRequestHeaders.Add(
                "X-XSRF-TOKEN",
                payload.RequestToken);
        }

        private sealed record AntiforgeryTokenPayload(
            string RequestToken,
            string HeaderName);
    }
}