using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            foreach (var rol in RoleNames.Todos)
            {
                if (await roleManager.RoleExistsAsync(rol))
                {
                    continue;
                }

                var resultado = await roleManager.CreateAsync(new IdentityRole(rol));

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(
                        "; ",
                        resultado.Errors.Select(error => $"{error.Code}: {error.Description}"));

                    throw new InvalidOperationException(
                        $"No se pudo crear el rol '{rol}'. {errores}");
                }

                logger.LogInformation("Rol {Rol} creado correctamente.", rol);
            }
        }
    }
}
