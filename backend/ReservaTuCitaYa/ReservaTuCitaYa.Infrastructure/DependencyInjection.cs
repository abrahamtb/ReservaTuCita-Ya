using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common.Disponibilidad;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Data.Seed;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Infrastructure.Repositories;
using ReservaTuCitaYa.Infrastructure.Security;

namespace ReservaTuCitaYa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrganizacionService, OrganizacionService>();
        services.AddScoped<ISedeService, SedeService>();
        services.AddScoped<ICategoriaServicioService, CategoriaServicioService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IRecursoService, RecursoService>();
        services.AddScoped<IBloqueoRecursoService, BloqueoRecursoService>();
        services.AddScoped<IHorarioSedeService, HorarioSedeService>();
        services.AddScoped<IHorarioProfesionalService, HorarioProfesionalService>();
        services.AddScoped<IHorarioRecursoService, HorarioRecursoService>();
        services.AddScoped<IDisponibilidadService, DisponibilidadService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IAtencionService, AtencionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReporteService, ReporteService>();

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

        services.Configure<DisponibilidadOptions>(configuration.GetSection(DisponibilidadOptions.Seccion));

        services.AddScoped<IOrganizacionRepository, OrganizacionRepository>();
        services.AddScoped<ISedeRepository, SedeRepository>();
        services.AddScoped<ICategoriaServicioRepository, CategoriaServicioRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IRecursoRepository, RecursoRepository>();
        services.AddScoped<IBloqueoRecursoRepository, BloqueoRecursoRepository>();
        services.AddScoped<IHorarioSedeRepository, HorarioSedeRepository>();
        services.AddScoped<IExcepcionHorarioSedeRepository, ExcepcionHorarioSedeRepository>();
        services.AddScoped<IHorarioProfesionalRepository, HorarioProfesionalRepository>();
        services.AddScoped<IExcepcionHorarioProfesionalRepository, ExcepcionHorarioProfesionalRepository>();
        services.AddScoped<IHorarioRecursoRepository, HorarioRecursoRepository>();
        services.AddScoped<IExcepcionHorarioRecursoRepository, ExcepcionHorarioRecursoRepository>();
        services.AddScoped<IDisponibilidadRepository, DisponibilidadRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IAtencionRepository, AtencionRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<IReembolsoRepository, ReembolsoRepository>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<ICalificacionService, CalificacionService>();
        services.AddScoped<ICalificacionRepository, CalificacionRepository>();

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
        await PermissionSeeder.SeedPermissionsAsync(dbContext, logger);
        await RolePermissionSeeder.SeedRolePermissionsAsync(dbContext, roleManager, logger);
        await SeedSuperAdmin.SeedSuperAdminAsync(
            userManager,
            roleManager,
            configuration,
            logger);
        await MetodoPagoSeeder.SeedAsync(dbContext);
    }
}