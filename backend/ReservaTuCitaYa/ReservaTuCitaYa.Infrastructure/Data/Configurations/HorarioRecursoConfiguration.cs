using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class HorarioRecursoConfiguration
    : IEntityTypeConfiguration<HorarioRecurso>
{
    public void Configure(EntityTypeBuilder<HorarioRecurso> builder)
    {
        builder.ToTable("HorariosRecursos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiaSemana)
            .IsRequired();

        builder.Property(x => x.HoraInicio)
            .IsRequired();

        builder.Property(x => x.HoraFin)
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
    }
}
