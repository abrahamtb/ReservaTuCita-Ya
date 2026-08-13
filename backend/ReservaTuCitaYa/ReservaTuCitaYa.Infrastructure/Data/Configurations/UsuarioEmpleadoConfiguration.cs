using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class UsuarioEmpleadoConfiguration : IEntityTypeConfiguration<UsuarioEmpleado>
    {
        public void Configure(EntityTypeBuilder<UsuarioEmpleado> builder)
        {
            builder.HasIndex(ue => new { ue.UsuarioId, ue.EmpleadoId }).IsUnique();

            // FK hacia Empleado se agrega cuando la entidad Empleado exista
        }
    }
}
