using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class HorarioProfesionalConfiguration
    : IEntityTypeConfiguration<HorarioProfesional>
{
    public void Configure(EntityTypeBuilder<HorarioProfesional> builder)
    {
        builder.ToTable("HorariosProfesionales", tabla =>
        {
            tabla.HasCheckConstraint(
                "CK_HorariosProfesionales_HoraInicio_HoraFin",
                "[HoraInicio] < [HoraFin]");
        });

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

        builder.HasIndex(x => x.SedeId);

        // Índice preparado para cuando exista la entidad Profesional.
        // Actualmente ProfesionalId es solo un campo de referencia,
        // no existe relación FK porque la entidad Profesional aún no está creada.
        builder.HasIndex(x => new
        {
            x.ProfesionalId,
            x.SedeId,
            x.DiaSemana
        });

        builder.HasOne(x => x.Sede)
            .WithMany()
            .HasForeignKey(x => x.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.EstaEliminado);
    }
}
