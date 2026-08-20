using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public sealed class CalificacionConfiguration : IEntityTypeConfiguration<Calificacion>
    {
        public void Configure(EntityTypeBuilder<Calificacion> builder)
        {
            builder.ToTable("Calificaciones");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Puntuacion)
                   .IsRequired();

            builder.Property(c => c.Comentario)
                   .HasMaxLength(1000);

            builder.Property(c => c.FechaCalificacion)
                   .IsRequired();

            builder.HasIndex(c => c.ReservaId).IsUnique();
            builder.HasIndex(c => c.AtencionId).IsUnique();

   
            builder.HasOne(c => c.Reserva)
                   .WithOne(r => r.Calificacion)
                   .HasForeignKey<Calificacion>(c => c.ReservaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Atencion)
                   .WithOne(a => a.Calificacion)
                   .HasForeignKey<Calificacion>(c => c.AtencionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Atencion is required and has a global soft-delete filter.  The
            // dependent must use the same visibility rule to avoid returning
            // a Calificacion whose required principal was filtered out.
            builder.HasQueryFilter(c =>
                !c.EstaEliminado &&
                !c.Reserva.EstaEliminado &&
                !c.Reserva.Organizacion.EstaEliminado &&
                !c.Reserva.Sede.EstaEliminado &&
                !c.Reserva.Cliente.EstaEliminado &&
                !c.Reserva.Servicio.EstaEliminado &&
                !c.Atencion.EstaEliminado);
        }
    }
}
