using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class AtencionConfiguration : IEntityTypeConfiguration<Atencion>
{
    public void Configure(EntityTypeBuilder<Atencion> builder)
    {
        builder.ToTable("Atenciones");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ReservaId)
            .IsUnique();

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.Recomendaciones)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Reserva)
            .WithOne(x => x.Atencion)
            .HasForeignKey<Atencion>(x => x.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProximoServicio)
            .WithMany()
            .HasForeignKey(x => x.ProximoServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado &&
            !x.Reserva.EstaEliminado &&
            !x.Reserva.Organizacion.EstaEliminado &&
            !x.Reserva.Sede.EstaEliminado &&
            !x.Reserva.Cliente.EstaEliminado &&
            !x.Reserva.Servicio.EstaEliminado);
    }
}