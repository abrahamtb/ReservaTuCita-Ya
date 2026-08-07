using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Data.Seed;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Infrastructure.Repositories;

namespace ReservaTuCitaYa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrganizacionService, OrganizacionService>();
        services.AddScoped<ISedeService, SedeService>();
        services.AddScoped<ICategoriaServicioService, CategoriaServicioService>();
        services.AddScoped<IServicioService, ServicioService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se configuró ConnectionStrings:DefaultConnection mediante User Secrets o variables de entorno.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOrganizacionRepository, OrganizacionRepository>();
        services.AddScoped<ISedeRepository, SedeRepository>();
        services.AddScoped<ICategoriaServicioRepository, CategoriaServicioRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitialization");

        await dbContext.Database.MigrateAsync(cancellationToken);
        await IdentitySeeder.SeedRolesAsync(roleManager, logger);
        await SeedSuperAdmin.SeedSuperAdminAsync(
            userManager,
            roleManager,
            configuration,
            logger);
    }
}
