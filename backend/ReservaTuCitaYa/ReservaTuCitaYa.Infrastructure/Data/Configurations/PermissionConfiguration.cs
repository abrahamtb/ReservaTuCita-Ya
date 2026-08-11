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
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasIndex(p => p.Codigo).IsUnique();
            builder.Property(p => p.Codigo).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Descripcion).HasMaxLength(300);
        }
    }
}
