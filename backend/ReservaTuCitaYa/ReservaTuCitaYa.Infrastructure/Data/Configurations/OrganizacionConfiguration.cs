using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class OrganizacionConfiguration : IEntityTypeConfiguration<Organizacion>
    {
        public void Configure(EntityTypeBuilder<Organizacion> builder)
        {
            builder.ToTable("Organizaciones");
            builder.HasKey(organizacion => organizacion.Id);

            builder.Property(organizacion => organizacion.NombreComercial)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(organizacion => organizacion.RazonSocial)
                .HasMaxLength(200);

            builder.Property(organizacion => organizacion.NumeroDocumento)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(organizacion => organizacion.Telefono)
                .HasMaxLength(30);

            builder.Property(organizacion => organizacion.Correo)
                .HasMaxLength(256);

            builder.Property(organizacion => organizacion.DireccionPrincipal)
                .HasMaxLength(250);

            builder.Property(organizacion => organizacion.LogoUrl)
                .HasMaxLength(500);

            builder.HasIndex(organizacion => organizacion.NumeroDocumento)
                .IsUnique();

            builder.HasIndex(organizacion => organizacion.TipoOrganizacionId);

            builder.HasOne(organizacion => organizacion.TipoOrganizacion)
                .WithMany(tipo => tipo.Organizaciones)
                .HasForeignKey(organizacion => organizacion.TipoOrganizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(organizacion => !organizacion.EstaEliminado);
        }
    }
}
