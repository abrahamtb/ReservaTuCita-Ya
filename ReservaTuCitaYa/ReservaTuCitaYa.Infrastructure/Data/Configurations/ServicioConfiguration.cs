using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            builder.ToTable("Servicios", tabla =>
            {
                tabla.HasCheckConstraint("CK_Servicios_DuracionMinutos", "[DuracionMinutos] > 0");
                tabla.HasCheckConstraint("CK_Servicios_Precio", "[Precio] >= 0");
                tabla.HasCheckConstraint(
                    "CK_Servicios_MontoAdelanto",
                    "[MontoAdelanto] >= 0 AND [MontoAdelanto] <= [Precio]");
                tabla.HasCheckConstraint("CK_Servicios_CapacidadMaxima", "[CapacidadMaxima] > 0");
                tabla.HasCheckConstraint(
                    "CK_Servicios_CapacidadIndividual",
                    "[EsGrupal] = 1 OR [CapacidadMaxima] = 1");
                tabla.HasCheckConstraint(
                    "CK_Servicios_TiemposNoNegativos",
                    "[HorasLimiteCancelacion] >= 0 AND [TiempoPreparacionMinutos] >= 0 AND [TiempoPosteriorMinutos] >= 0");
            });

            builder.HasKey(servicio => servicio.Id);

            builder.Property(servicio => servicio.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(servicio => servicio.Descripcion)
                .HasMaxLength(1000);

            builder.Property(servicio => servicio.Precio)
                .HasPrecision(18, 2);

            builder.Property(servicio => servicio.MontoAdelanto)
                .HasPrecision(18, 2);

            builder.Property(servicio => servicio.Modalidad)
                .IsRequired();

            builder.HasIndex(servicio => servicio.OrganizacionId);
            builder.HasIndex(servicio => servicio.CategoriaServicioId);

            builder.HasIndex(servicio => new { servicio.OrganizacionId, servicio.Nombre })
                .IsUnique()
                .HasFilter("[EstaActivo] = 1 AND [EstaEliminado] = 0");

            builder.HasOne(servicio => servicio.Organizacion)
                .WithMany(organizacion => organizacion.Servicios)
                .HasForeignKey(servicio => servicio.OrganizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(servicio => servicio.CategoriaServicio)
                .WithMany(categoria => categoria.Servicios)
                .HasForeignKey(servicio => servicio.CategoriaServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(servicio => !servicio.EstaEliminado);
        }
    }
}
