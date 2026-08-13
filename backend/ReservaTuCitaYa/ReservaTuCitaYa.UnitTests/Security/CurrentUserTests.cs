using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Abstractions;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Security;
using System.Security.Claims;
using Xunit;

namespace ReservaTuCitaYa.UnitTests.Security
{
    public class CurrentUserTests
    {
        private static ApplicationDbContext CrearDbEnMemoria()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private static IHttpContextAccessor CrearAccessorAutenticado(string userId, string rol)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Role, rol)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext { User = principal };
            return new HttpContextAccessor { HttpContext = context };
        }

        [Fact]
        public void CurrentUser_NoAutenticado_NoDebeCargarPermisos()
        {
            var db = CrearDbEnMemoria();
            var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

            var currentUser = new CurrentUser(accessor, db);

            Assert.False(currentUser.IsAuthenticated);
            Assert.Empty(currentUser.Permissions);
        }

        [Fact]
        public void CurrentUser_ConPermisoAsignado_HasPermission_DebeSerTrue()
        {
            var userId = Guid.NewGuid().ToString();
            const string roleId = "role-admin";
            const string permisoCodigo = "clientes.ver";

            var db = CrearDbEnMemoria();
            var permission = new Permission { Codigo = permisoCodigo, Nombre = permisoCodigo };
            db.Permissions.Add(permission);
            db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permission.Id });
            db.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().Add(
                new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
                {
                    UserId = userId,
                    RoleId = roleId
                });
            db.SaveChanges();

            var accessor = CrearAccessorAutenticado(userId, "Administrador");
            var currentUser = new CurrentUser(accessor, db);

            Assert.True(currentUser.HasPermission(permisoCodigo));
        }

        [Fact]
        public void CurrentUser_SinPermisoAsignado_HasPermission_DebeSerFalse()
        {
            var userId = Guid.NewGuid().ToString();
            var db = CrearDbEnMemoria();

            var accessor = CrearAccessorAutenticado(userId, "Cliente");
            var currentUser = new CurrentUser(accessor, db);

            Assert.False(currentUser.HasPermission("clientes.eliminar"));
        }

        [Fact]
        public void CurrentUser_IsInRole_DebeReflejarClaimsDeRol()
        {
            var userId = Guid.NewGuid().ToString();
            var db = CrearDbEnMemoria();

            var accessor = CrearAccessorAutenticado(userId, "Profesional");
            var currentUser = new CurrentUser(accessor, db);

            Assert.True(currentUser.IsInRole("Profesional"));
            Assert.False(currentUser.IsInRole("Administrador"));
        }
    }
}