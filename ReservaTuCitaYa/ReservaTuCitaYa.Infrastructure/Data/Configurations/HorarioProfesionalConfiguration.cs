using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class HorarioProfesionalConfiguration
    : IEntityTypeConfiguration<HorarioProfesional>
{
    public void Configure(EntityTypeBuilder<HorarioProfesional> builder)
    {
        builder.ToTable("HorariosProfesionales");


        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiaSemana)
            .IsRequired();

        builder.Property(x => x.HoraInicio)
            .IsRequired();

        builder.Property(x => x.HoraFin)
            .IsRequired();

        builder.Property(x => x.FechaInicioVigencia)
            .IsRequired();

        builder.Property(x => x.FechaFinVigencia)
            .IsRequired(false);

        builder.HasOne(x => x.Sede)
            .WithMany()
            .HasForeignKey(x => x.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado && !x.Sede.EstaEliminado);

        // Cuando exista Profesional:
        //
        // builder.HasOne(x => x.Profesional)
        //     .WithMany(x => x.Horarios)
        //     .HasForeignKey(x => x.ProfesionalId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
