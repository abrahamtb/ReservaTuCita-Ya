using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed
{
    public static class MetodoPagoSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.MetodosPago.AnyAsync())
            {
                var metodos = new List<MetodoPago>
            {
                new MetodoPago { Codigo = "EFECTIVO", Nombre = "Efectivo", RequiereNumeroOperacion = false, EstaActivo = true },
                new MetodoPago { Codigo = "YAPE_PLIN", Nombre = "Yape / Plin", RequiereNumeroOperacion = true, EstaActivo = true },
                new MetodoPago { Codigo = "TRANSFERENCIA", Nombre = "Transferencia bancaria", RequiereNumeroOperacion = true, EstaActivo = true },
                new MetodoPago { Codigo = "TARJETA", Nombre = "Tarjeta", RequiereNumeroOperacion = true, EstaActivo = true },
                new MetodoPago { Codigo = "OTRO", Nombre = "Otro", RequiereNumeroOperacion = false, EstaActivo = true }
            };

                await context.MetodosPago.AddRangeAsync(metodos);
                await context.SaveChangesAsync();
            }
        }
    }
}
