using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class ProfesionalServicioConfiguration : IEntityTypeConfiguration<ProfesionalServicio>
{
    public void Configure(EntityTypeBuilder<ProfesionalServicio> builder)
    {
        builder.ToTable("ProfesionalesServicio");
        builder.HasKey(relacion => relacion.Id);
        builder.HasIndex(relacion => new { relacion.EmpleadoId, relacion.ServicioId }).IsUnique();
        builder.HasIndex(relacion => relacion.ServicioId);

        builder.HasOne(relacion => relacion.Empleado)
            .WithMany(empleado => empleado.ServiciosProfesionales)
            .HasForeignKey(relacion => relacion.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(relacion => relacion.Servicio)
            .WithMany(servicio => servicio.ProfesionalesServicio)
            .HasForeignKey(relacion => relacion.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(relacion => !relacion.EstaEliminado);
    }
}
