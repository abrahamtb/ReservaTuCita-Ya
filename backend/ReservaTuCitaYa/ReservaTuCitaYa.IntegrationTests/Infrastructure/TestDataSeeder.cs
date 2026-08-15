using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.IntegrationTests.Infrastructure
{
    public static class TestDataSeeder
    {
        public static async Task<(Guid OrgId, ApplicationUser Admin)> CrearOrganizacionConAdminAsync(
            IServiceProvider services, string sufijo)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Asegurar que los roles existan
            foreach (var rol in RoleNames.Todos)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Asegurar que los permisos existan
            await SeedPermissionsAsync(db);

            // 3. Crear TipoOrganizacion
            var tipoOrg = new TipoOrganizacion
            {
                Id = Guid.NewGuid(),
                Nombre = $"Tipo-{sufijo}"
            };
            db.TiposOrganizacion.Add(tipoOrg);
            await db.SaveChangesAsync();

            // 4. Crear Organizacion
            var organizacion = new Organizacion
            {
                Id = Guid.NewGuid(),
                TipoOrganizacionId = tipoOrg.Id,
                NombreComercial = $"Organizacion-{sufijo}",
                RazonSocial = $"Organizacion-{sufijo} SAC",
                NumeroDocumento = $"2000000{sufijo}",
                Telefono = "999999999",
                Correo = $"org{sufijo}@test.com",
                DireccionPrincipal = "Direccion de prueba",
                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            };
            db.Organizaciones.Add(organizacion);
            await db.SaveChangesAsync();

            // 5. Crear usuario admin
            var admin = new ApplicationUser
            {
                UserName = $"admin{sufijo}@test.com",
                Email = $"admin{sufijo}@test.com",
                Nombres = "Admin",
                Apellidos = sufijo,
                NumeroDocumento = $"1000000{sufijo}",
                Telefono = "999999999",
                EstaActivo = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!Seguro");
            await userManager.AddToRoleAsync(admin, RoleNames.Administrador);

            // 6. Crear relación Usuario-Organizacion
            var usuarioOrg = new UsuarioOrganizacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = admin.Id,
                OrganizacionId = organizacion.Id,
                EsPrincipal = true,
                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            };
            db.UsuariosOrganizaciones.Add(usuarioOrg);
            await db.SaveChangesAsync();

            // 7. --- IMPORTANTE: Asignar TODOS los permisos al rol Administrador ---
            await AsignarPermisosAlRolAdminAsync(db, roleManager);

            return (organizacion.Id, admin);
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext db)
        {
            foreach (var permisoNombre in Permissions.Todos)
            {
                if (!await db.Permissions.AnyAsync(p => p.Nombre == permisoNombre))
                {
                    db.Permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Codigo = permisoNombre,
                        Nombre = permisoNombre,
                        Descripcion = $"Permiso para {permisoNombre}"
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        private static async Task AsignarPermisosAlRolAdminAsync(
            ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync(RoleNames.Administrador);
            if (adminRole == null) return;

            var allPermissions = await db.Permissions.ToListAsync();

            foreach (var permission in allPermissions)
            {
                var exists = await db.RolePermissions
                    .AnyAsync(rp => rp.RoleId == adminRole.Id &&
                                    rp.PermissionId == permission.Id);

                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}