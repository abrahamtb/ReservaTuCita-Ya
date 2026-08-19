using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Identity;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class RolePermissionSeeder
    {
        private static readonly Dictionary<string, string[]> MatrizPorRol = new()
        {
            [RoleNames.Superadministrador] = Permissions.Todos.ToArray(),
            [RoleNames.Administrador] = Permissions.Todos.ToArray(),
            [RoleNames.Recepcionista] = new[]
            {
                Permissions.Sedes.Ver,
                Permissions.Clientes.Ver, Permissions.Clientes.Crear, Permissions.Clientes.Editar,
                Permissions.Servicios.Ver,
                Permissions.Empleados.Ver,
                Permissions.Recursos.Ver,
                Permissions.Horarios.Ver,
                Permissions.Reservas.Ver, Permissions.Reservas.Crear,
                Permissions.Reservas.Reprogramar, Permissions.Reservas.Cancelar,
                Permissions.Atenciones.Ver,
                Permissions.Pagos.Ver, Permissions.Pagos.Registrar,
            },
            [RoleNames.Profesional] = new[]
            {
                Permissions.Reservas.Ver,
                Permissions.Atenciones.Ver, Permissions.Atenciones.MarcarPresente,
                Permissions.Atenciones.Iniciar, Permissions.Atenciones.Finalizar,
            },
            [RoleNames.Cliente] = new[]
            {
                Permissions.Sedes.Ver, Permissions.Servicios.Ver,
                Permissions.Reservas.Ver, Permissions.Reservas.Crear,
                Permissions.Reservas.Reprogramar, Permissions.Reservas.Cancelar,
                Permissions.Pagos.Ver,
                Permissions.Calificaciones.Ver, Permissions.Calificaciones.Crear,
            },
        };

        public static async Task SeedRolePermissionsAsync(
            ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            var permisosPorCodigo = await db.Permissions.ToDictionaryAsync(p => p.Codigo, p => p.Id);
            var totalNuevos = 0;

            foreach (var (nombreRol, codigosPermiso) in MatrizPorRol)
            {
                var rol = await roleManager.FindByNameAsync(nombreRol);
                if (rol is null)
                {
                    logger.LogWarning("El rol {Rol} no existe todavía, se omite su matriz de permisos.", nombreRol);
                    continue;
                }

                var existentes = await db.RolePermissions
                    .Where(rp => rp.RoleId == rol.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var nuevos = codigosPermiso
                    .Where(codigo => permisosPorCodigo.ContainsKey(codigo))
                    .Select(codigo => permisosPorCodigo[codigo])
                    .Where(permissionId => !existentes.Contains(permissionId))
                    .Select(permissionId => new RolePermission { RoleId = rol.Id, PermissionId = permissionId })
                    .ToList();

                if (nuevos.Count > 0)
                {
                    db.RolePermissions.AddRange(nuevos);
                    totalNuevos += nuevos.Count;
                    logger.LogInformation("Se asignaron {Cantidad} permisos nuevos al rol {Rol}.", nuevos.Count, nombreRol);
                }
            }

            if (totalNuevos > 0) await db.SaveChangesAsync();
            else logger.LogInformation("La matriz de permisos por rol ya estaba completa, no se agregó nada.");
        }
    }
}
