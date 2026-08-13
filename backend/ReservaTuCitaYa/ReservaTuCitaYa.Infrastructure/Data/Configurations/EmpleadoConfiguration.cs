using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(empleado => empleado.Id);
        builder.Property(empleado => empleado.TipoDocumento).IsRequired();
        builder.Property(empleado => empleado.NumeroDocumento).IsRequired().HasMaxLength(20);
        builder.Property(empleado => empleado.Nombres).IsRequired().HasMaxLength(100);
        builder.Property(empleado => empleado.Apellidos).IsRequired().HasMaxLength(100);
        builder.Property(empleado => empleado.Correo).HasMaxLength(150);
        builder.Property(empleado => empleado.Telefono).HasMaxLength(30);
        builder.Property(empleado => empleado.Direccion).HasMaxLength(250);
        builder.Property(empleado => empleado.FechaNacimiento).HasColumnType("date");
        builder.Property(empleado => empleado.Cargo).IsRequired().HasMaxLength(100);
        builder.Property(empleado => empleado.Especialidad).HasMaxLength(150);
        builder.Property(empleado => empleado.NumeroColegiatura).HasMaxLength(50);
        builder.Property(empleado => empleado.Observaciones).HasMaxLength(500);

        builder.HasIndex(empleado => empleado.OrganizacionId);
        builder.HasIndex(empleado => new
            { empleado.OrganizacionId, empleado.TipoDocumento, empleado.NumeroDocumento })
            .IsUnique();

        builder.HasOne(empleado => empleado.Organizacion)
            .WithMany(organizacion => organizacion.Empleados)
            .HasForeignKey(empleado => empleado.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(empleado => !empleado.EstaEliminado);
    }
}
