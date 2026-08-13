using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using ReservaTuCitaYa.Api.Middleware;
using ReservaTuCitaYa.Infrastructure;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddIdentityServices();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ReservaTuCitaYa.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context => WriteAuthenticationProblemAsync(
        context,
        StatusCodes.Status401Unauthorized,
        "No autenticado",
        "Debes iniciar sesión para acceder a este recurso.");

    options.Events.OnRedirectToAccessDenied = context => WriteAuthenticationProblemAsync(
        context,
        StatusCodes.Status403Forbidden,
        "Acceso denegado",
        "No tienes permisos para realizar esta operación.");

    options.Events.OnValidatePrincipal = async context =>
    {
        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal!);

        if (user is null || !user.EstaActivo)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "ReservaTuCitaYa.Xsrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var frontendUrl = builder.Configuration["Frontend:Url"]
    ?? throw new InvalidOperationException("No se configuró Frontend:Url.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddProblemDetails();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reserva tu Cita Ya API",
        Version = "v1"
    });
    options.AddSecurityDefinition("IdentityCookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "ReservaTuCitaYa.Auth",
        Description = "Cookie segura creada por POST /api/auth/login. Para escrituras solicita antes /api/antiforgery/token."
    });
    options.AddSecurityDefinition("XsrfToken", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-XSRF-TOKEN",
        Description = "Token antiforgery obtenido de GET /api/antiforgery/token. Requerido en POST/PUT/PATCH/DELETE."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "XsrfToken"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiAntiforgeryMiddleware>();
app.MapControllers();

await app.Services.InitializeDatabaseAsync(builder.Configuration);

app.Run();

static Task WriteAuthenticationProblemAsync(
    RedirectContext<CookieAuthenticationOptions> context,
    int statusCode,
    string title,
    string detail)
{
    return Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail,
        instance: context.Request.Path)
        .ExecuteAsync(context.HttpContext);
}

public partial class Program;
