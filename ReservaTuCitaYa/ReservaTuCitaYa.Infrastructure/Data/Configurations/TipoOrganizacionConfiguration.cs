using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class TipoOrganizacionConfiguration : IEntityTypeConfiguration<TipoOrganizacion>
    {
        public void Configure(EntityTypeBuilder<TipoOrganizacion> builder)
        {
            builder.ToTable("TiposOrganizacion");
            builder.HasKey(tipo => tipo.Id);

            builder.Property(tipo => tipo.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(tipo => tipo.Descripcion)
                .HasMaxLength(500);

            builder.HasIndex(tipo => tipo.Nombre)
                .IsUnique()
                .HasFilter("[EstaActivo] = 1 AND [EstaEliminado] = 0");

            builder.HasQueryFilter(tipo => !tipo.EstaEliminado);
        }
    }
}
