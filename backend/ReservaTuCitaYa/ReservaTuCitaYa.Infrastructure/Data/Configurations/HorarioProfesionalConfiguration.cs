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

        builder.HasOne(x => x.Empleado)
            .WithMany()
            .HasForeignKey(x => x.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sede)
            .WithMany()
            .HasForeignKey(x => x.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado && !x.Sede.EstaEliminado);
    }
}
