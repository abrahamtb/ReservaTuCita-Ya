using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class SedeConfiguration : IEntityTypeConfiguration<Sede>
    {
        public void Configure(EntityTypeBuilder<Sede> builder)
        {
            builder.ToTable("Sedes");
            builder.HasKey(sede => sede.Id);

            builder.Property(sede => sede.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(sede => sede.Direccion)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(sede => sede.Telefono)
                .HasMaxLength(30);

            builder.Property(sede => sede.Correo)
                .HasMaxLength(256);

            builder.Property(sede => sede.Referencia)
                .HasMaxLength(500);

            builder.HasIndex(sede => new { sede.OrganizacionId, sede.Nombre })
                .IsUnique()
                .HasFilter("[EstaActivo] = 1 AND [EstaEliminado] = 0");

            builder.HasOne(sede => sede.Organizacion)
                .WithMany(organizacion => organizacion.Sedes)
                .HasForeignKey(sede => sede.OrganizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(sede => !sede.EstaEliminado);
        }
    }
}
