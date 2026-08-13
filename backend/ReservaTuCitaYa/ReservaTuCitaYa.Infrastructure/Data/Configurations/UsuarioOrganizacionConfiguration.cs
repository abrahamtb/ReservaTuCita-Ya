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
    public class UsuarioOrganizacionConfiguration : IEntityTypeConfiguration<UsuarioOrganizacion>
    {
        public void Configure(EntityTypeBuilder<UsuarioOrganizacion> builder)
        {
            builder.HasIndex(uo => new { uo.UsuarioId, uo.OrganizacionId }).IsUnique();

            builder.HasOne(uo => uo.Organizacion)
                .WithMany()
                .HasForeignKey(uo => uo.OrganizacionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
