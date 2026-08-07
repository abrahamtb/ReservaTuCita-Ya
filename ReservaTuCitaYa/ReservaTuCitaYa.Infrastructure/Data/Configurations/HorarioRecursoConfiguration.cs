using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class HorarioRecursoConfiguration : IEntityTypeConfiguration<HorarioRecurso>
    {
        public void Configure(EntityTypeBuilder<HorarioRecurso> builder)
        {
            builder.ToTable("HorariosRecurso", tabla =>
            {
                tabla.HasCheckConstraint(
                    "CK_HorariosRecurso_HoraInicio_HoraFin",
                    "[HoraInicio] < [HoraFin]");
            });

            builder.HasKey(horario => horario.Id);

            builder.Property(horario => horario.DiaSemana)
                .IsRequired();

            builder.Property(horario => horario.HoraInicio)
                .IsRequired();

            builder.Property(horario => horario.HoraFin)
                .IsRequired();

            builder.HasIndex(horario => horario.RecursoId);

            builder.HasIndex(horario => new
            {
                horario.RecursoId,
                horario.DiaSemana,
                horario.HoraInicio,
                horario.HoraFin
            });

            builder.HasOne(horario => horario.Recurso)
                .WithMany(recurso => recurso.Horarios)
                .HasForeignKey(horario => horario.RecursoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(horario => !horario.EstaEliminado);
        }
    }
}
