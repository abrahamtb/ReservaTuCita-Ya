using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class BloqueoRecursoConfiguration : IEntityTypeConfiguration<BloqueoRecurso>
{
    public void Configure(EntityTypeBuilder<BloqueoRecurso> builder)
    {
        builder.ToTable("BloqueosRecursos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Motivo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.TipoBloqueo)
            .IsRequired();

        builder.Property(x => x.FechaHoraInicio)
            .IsRequired();

        builder.Property(x => x.FechaHoraFin)
            .IsRequired();


        builder.HasOne(x => x.Recurso)
            .WithMany()
            .HasForeignKey(x => x.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado &&
            !x.Recurso.EstaEliminado &&
            !x.Recurso.Organizacion.EstaEliminado &&
            !x.Recurso.Sede.EstaEliminado);


        // Cuando exista Profesional dentro de BloqueoProfesional
        // la relación sería:
        //
        // builder.HasOne(x => x.Profesional)
        //     .WithMany(x => x.Bloqueos)
        //     .HasForeignKey(x => x.ProfesionalId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
