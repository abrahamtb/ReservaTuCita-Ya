using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class RecursoConfiguration
    : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("Recursos");


        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.Property(x => x.UbicacionInterna)
            .HasMaxLength(200);

        builder.Property(x => x.Capacidad)
            .IsRequired();

        builder.Property(x => x.EstadoRecurso)
            .IsRequired();

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sede)
            .WithMany()
            .HasForeignKey(x => x.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado &&
            !x.Organizacion.EstaEliminado &&
            !x.Sede.EstaEliminado);
    }
}
