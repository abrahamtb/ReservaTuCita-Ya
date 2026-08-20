using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public sealed class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
    {
        public void Configure(EntityTypeBuilder<MetodoPago> builder)
        {
            builder.ToTable("MetodosPago");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Codigo)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(m => m.Codigo).IsUnique();

            builder.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.RequiereNumeroOperacion)
                .IsRequired();

            builder.Property(m => m.EstaActivo)
                .HasDefaultValue(true);
        }
    }
}
