using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAsync(ApplicationDbContext db, ILogger logger)
        {
            var existentes = await db.Permissions
                .Select(p => p.Codigo)
                .ToListAsync();

            var faltantes = Permissions.Todos
                .Except(existentes)
                .Select(codigo => new Permission
                {
                    Codigo = codigo,
                    Nombre = codigo
                })
                .ToList();

            if (faltantes.Count > 0)
            {
                db.Permissions.AddRange(faltantes);
                await db.SaveChangesAsync();
                logger.LogInformation("Se sembraron {Cantidad} permisos nuevos.", faltantes.Count);
            }
            else
            {
                logger.LogInformation("Los permisos ya estaban sembrados, no se agregó nada.");
            }
        }
    }
}
