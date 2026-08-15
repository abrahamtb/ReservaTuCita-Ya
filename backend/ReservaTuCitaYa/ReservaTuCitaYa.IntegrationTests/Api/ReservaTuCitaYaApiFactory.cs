using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.IntegrationTests.Api;

public sealed class ReservaTuCitaYaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin.api@test.local";
    public const string AdminPassword = "Admin1234";
    private readonly string _databaseName = $"ReservaTuCitaYa_Api_{Guid.NewGuid():N}";
    private string TestConnectionString =>
    $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["SeedAdmin:Email"] = AdminEmail,
                ["SeedAdmin:Password"] = AdminPassword,
                ["SeedAdmin:Nombres"] = "Administrador",
                ["SeedAdmin:Apellidos"] = "API",
                ["Frontend:Url"] = "http://localhost:5173"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(TestConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        _ = Server;
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await dbContext.TiposOrganizacion.AnyAsync())
        {
            dbContext.TiposOrganizacion.Add(new TipoOrganizacion
            {
                Nombre = "Empresa de prueba",
                Descripcion = "Tipo para pruebas HTTP"
            });
            await dbContext.SaveChangesAsync();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string email = "sinrol.api@test.local";
        if (await userManager.FindByEmailAsync(email) is null)
        {
            var result = await userManager.CreateAsync(new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nombres = "Usuario",
                Apellidos = "Sin rol",
                NumeroDocumento = "99999999",
                Telefono = "999999999",
                EstaActivo = true,
                EmailConfirmed = true
            }, AdminPassword);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}
