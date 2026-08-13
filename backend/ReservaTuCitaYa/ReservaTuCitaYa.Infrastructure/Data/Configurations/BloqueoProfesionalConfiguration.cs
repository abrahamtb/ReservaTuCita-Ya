using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class BloqueoProfesionalConfiguration
    : IEntityTypeConfiguration<BloqueoProfesional>
{
    public void Configure(EntityTypeBuilder<BloqueoProfesional> builder)
    {
        builder.ToTable("BloqueosProfesionales");

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

        builder.HasQueryFilter(x => !x.EstaEliminado);


        // Cuando exista la entidad Profesional:
        //
        // builder.HasOne(x => x.Profesional)
        //     .WithMany(x => x.BloqueosProfesionales)
        //     .HasForeignKey(x => x.ProfesionalId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
