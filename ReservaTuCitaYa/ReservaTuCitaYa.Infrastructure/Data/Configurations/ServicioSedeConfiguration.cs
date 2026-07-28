using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class ServicioSedeConfiguration : IEntityTypeConfiguration<ServicioSede>
    {
        public void Configure(EntityTypeBuilder<ServicioSede> builder)
        {
            builder.ToTable("ServiciosSede", tabla =>
            {
                tabla.HasCheckConstraint(
                    "CK_ServiciosSede_PrecioEspecial",
                    "[PrecioEspecial] IS NULL OR [PrecioEspecial] >= 0");
            });

            builder.HasKey(servicioSede => servicioSede.Id);

            builder.Property(servicioSede => servicioSede.PrecioEspecial)
                .HasPrecision(18, 2);

            builder.HasIndex(servicioSede => new { servicioSede.ServicioId, servicioSede.SedeId })
                .IsUnique()
                .HasFilter("[EstaActivo] = 1 AND [EstaEliminado] = 0");

            builder.HasOne(servicioSede => servicioSede.Servicio)
                .WithMany(servicio => servicio.ServiciosSede)
                .HasForeignKey(servicioSede => servicioSede.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(servicioSede => servicioSede.Sede)
                .WithMany(sede => sede.ServiciosSede)
                .HasForeignKey(servicioSede => servicioSede.SedeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(servicioSede => !servicioSede.EstaEliminado);
        }
    }
}
