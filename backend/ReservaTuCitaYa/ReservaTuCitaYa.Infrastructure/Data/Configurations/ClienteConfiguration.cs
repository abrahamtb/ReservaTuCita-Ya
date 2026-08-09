using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.TipoDocumento).IsRequired();
        builder.Property(cliente => cliente.NumeroDocumento).IsRequired().HasMaxLength(20);
        builder.Property(cliente => cliente.Nombres).IsRequired().HasMaxLength(100);
        builder.Property(cliente => cliente.Apellidos).IsRequired().HasMaxLength(100);
        builder.Property(cliente => cliente.Correo).HasMaxLength(150);
        builder.Property(cliente => cliente.Telefono).HasMaxLength(30);
        builder.Property(cliente => cliente.Direccion).HasMaxLength(250);
        builder.Property(cliente => cliente.FechaNacimiento).HasColumnType("date");
        builder.Property(cliente => cliente.Observaciones).HasMaxLength(500);

        builder.HasIndex(cliente => cliente.OrganizacionId);
        builder.HasIndex(cliente => new
            { cliente.OrganizacionId, cliente.TipoDocumento, cliente.NumeroDocumento })
            .IsUnique();

        builder.HasOne(cliente => cliente.Organizacion)
            .WithMany(organizacion => organizacion.Clientes)
            .HasForeignKey(cliente => cliente.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(cliente => !cliente.EstaEliminado);
    }
}
