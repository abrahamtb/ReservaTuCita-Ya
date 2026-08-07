using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public class HorarioSedeConfiguration
    : IEntityTypeConfiguration<HorarioSede>
{
    public void Configure(EntityTypeBuilder<HorarioSede> builder)
    {
        builder.ToTable("HorariosSede");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiaSemana)
            .IsRequired();

        builder.Property(x => x.HoraInicio)
            .IsRequired();

        builder.Property(x => x.HoraFin)
            .IsRequired();

        builder.HasOne(x => x.Sede)
            .WithMany()
            .HasForeignKey(x => x.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x =>
            !x.EstaEliminado && !x.Sede.EstaEliminado);
    }
}
