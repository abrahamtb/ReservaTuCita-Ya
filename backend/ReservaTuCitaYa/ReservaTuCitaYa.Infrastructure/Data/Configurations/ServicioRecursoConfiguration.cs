using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class ServicioRecursoConfiguration : IEntityTypeConfiguration<ServicioRecurso>
{
    public void Configure(EntityTypeBuilder<ServicioRecurso> builder)
    {
        builder.ToTable("ServiciosRecurso");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ServicioId, x.RecursoId }).IsUnique();
        builder.Property(x => x.CantidadRequerida).HasDefaultValue(1);
        builder.HasOne(x => x.Servicio).WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Recurso).WithMany(x => x.Servicios).HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Servicio.EstaEliminado && !x.Recurso.EstaEliminado);
    }
}
