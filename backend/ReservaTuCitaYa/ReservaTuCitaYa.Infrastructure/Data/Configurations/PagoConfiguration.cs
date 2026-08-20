using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public sealed class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(p => p.Codigo).IsUnique();

            builder.Property(p => p.Monto)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(p => p.FechaPago)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(p => p.NumeroOperacion)
                .HasMaxLength(100);

            builder.Property(p => p.Observacion)
                .HasMaxLength(500);

            builder.Property(p => p.MotivoAnulacion)
                .HasMaxLength(500);

            builder.HasIndex(p => p.ReservaId);
            builder.HasIndex(p => p.MetodoPagoId);
            builder.HasIndex(p => p.FechaPago);
            builder.HasOne(p => p.MetodoPago)
                .WithMany(m => m.Pagos)
                .HasForeignKey(p => p.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Reserva>()
                .WithMany()
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
