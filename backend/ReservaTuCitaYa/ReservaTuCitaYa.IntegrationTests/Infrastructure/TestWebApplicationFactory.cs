using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservaTuCitaYa.Infrastructure.Data;

namespace ReservaTuCitaYa.IntegrationTests.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        // IMPORTANTE:
        // Se genera UNA sola vez para toda esta instancia de la Factory.
        private readonly string _databaseName =
            $"IntegrationTests_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthHandler.SchemeName;

                    options.DefaultChallengeScheme =
                        TestAuthHandler.SchemeName;

                    options.DefaultScheme =
                        TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    options => { });
            });
            builder.UseEnvironment("Development");
        }
        public HttpClient CreateHttpsClient()
        {
            return CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });
        }
    }
}