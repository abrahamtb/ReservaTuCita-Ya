using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class EmpleadoSedeConfiguration : IEntityTypeConfiguration<EmpleadoSede>
{
    public void Configure(EntityTypeBuilder<EmpleadoSede> builder)
    {
        builder.ToTable("EmpleadosSede");
        builder.HasKey(relacion => relacion.Id);
        builder.HasIndex(relacion => new { relacion.EmpleadoId, relacion.SedeId }).IsUnique();
        builder.HasIndex(relacion => relacion.SedeId);

        builder.HasOne(relacion => relacion.Empleado)
            .WithMany(empleado => empleado.Sedes)
            .HasForeignKey(relacion => relacion.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(relacion => relacion.Sede)
            .WithMany(sede => sede.EmpleadosSede)
            .HasForeignKey(relacion => relacion.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(relacion => !relacion.EstaEliminado);
    }
}
