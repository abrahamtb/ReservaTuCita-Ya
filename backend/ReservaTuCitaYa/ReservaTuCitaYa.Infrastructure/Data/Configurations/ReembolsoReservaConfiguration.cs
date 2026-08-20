using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public sealed class ReembolsoReservaConfiguration : IEntityTypeConfiguration<ReembolsoReserva>
    {
        public void Configure(EntityTypeBuilder<ReembolsoReserva> builder)
        {
            builder.ToTable("ReembolsosReserva");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Codigo)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(r => r.Codigo).IsUnique();

            builder.Property(r => r.Monto)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(r => r.FechaReembolso)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(r => r.NumeroOperacion)
                .HasMaxLength(100);

            builder.Property(r => r.Motivo)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(r => r.Observacion)
                .HasMaxLength(500);

            builder.HasIndex(r => r.ReservaId);
            builder.HasIndex(r => r.FechaReembolso);

            builder.HasOne(r => r.MetodoPago)
                .WithMany(m => m.Reembolsos)
                .HasForeignKey(r => r.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Reserva>()
                .WithMany()
                .HasForeignKey(r => r.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
