using Microsoft.AspNetCore.Identity;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class SeedSuperAdmin
    {
        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            const string email = "superadmin@reservatucitaya.com";
            const string password = "SuperAdmin123!"; // cámbiala luego, esto es temporal para desarrollo

            var existente = await userManager.FindByEmailAsync(email);
            if (existente != null)
                return;

            var superAdmin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nombres = "Super",
                Apellidos = "Administrador",
                NumeroDocumento = "00000000",
                Telefono = "000000000",
                EstaActivo = true,
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(superAdmin, password);

            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "Superadministrador");
            }
        }
    }
}