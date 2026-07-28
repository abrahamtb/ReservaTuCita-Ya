using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class SeedSuperAdmin
    {
        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger logger)
        {
            var email = ObtenerConfiguracionRequerida(configuration, "SeedAdmin:Email");
            var password = ObtenerConfiguracionRequerida(configuration, "SeedAdmin:Password");
            var nombres = ObtenerConfiguracionRequerida(configuration, "SeedAdmin:Nombres");
            var apellidos = ObtenerConfiguracionRequerida(configuration, "SeedAdmin:Apellidos");

            if (!await roleManager.RoleExistsAsync(RoleNames.Superadministrador))
            {
                throw new InvalidOperationException(
                    $"El rol '{RoleNames.Superadministrador}' debe existir antes de crear el superadministrador.");
            }

            var superAdmin = await userManager.FindByEmailAsync(email);

            if (superAdmin is null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    NumeroDocumento = "00000000",
                    Telefono = "000000000",
                    EstaActivo = true,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(superAdmin, password);

                ValidarResultado(resultado, "crear el usuario superadministrador");
                logger.LogInformation(
                    "Usuario superadministrador {Email} creado correctamente.",
                    email);
            }

            if (!await userManager.IsInRoleAsync(superAdmin, RoleNames.Superadministrador))
            {
                var resultadoRol = await userManager.AddToRoleAsync(
                    superAdmin,
                    RoleNames.Superadministrador);

                ValidarResultado(
                    resultadoRol,
                    $"asignar el rol '{RoleNames.Superadministrador}'");
            }
        }

        private static string ObtenerConfiguracionRequerida(
            IConfiguration configuration,
            string clave)
        {
            var valor = configuration[clave];

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException(
                    $"Falta la configuración segura '{clave}'. Configúrala mediante User Secrets o variables de entorno.");
            }

            return valor;
        }

        private static void ValidarResultado(IdentityResult resultado, string operacion)
        {
            if (resultado.Succeeded)
            {
                return;
            }

            var errores = string.Join(
                "; ",
                resultado.Errors.Select(error => $"{error.Code}: {error.Description}"));

            throw new InvalidOperationException(
                $"No se pudo {operacion}. {errores}");
        }
    }
}
