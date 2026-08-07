using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations
{
    public class CategoriaServicioConfiguration : IEntityTypeConfiguration<CategoriaServicio>
    {
        public void Configure(EntityTypeBuilder<CategoriaServicio> builder)
        {
            builder.ToTable("CategoriasServicio");
            builder.HasKey(categoria => categoria.Id);

            builder.Property(categoria => categoria.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(categoria => categoria.Descripcion)
                .HasMaxLength(500);

            builder.HasIndex(categoria => categoria.OrganizacionId);

            builder.HasIndex(categoria => new { categoria.OrganizacionId, categoria.Nombre })
                .IsUnique()
                .HasFilter("[EstaActivo] = 1 AND [EstaEliminado] = 0");

            builder.HasOne(categoria => categoria.Organizacion)
                .WithMany(organizacion => organizacion.CategoriasServicio)
                .HasForeignKey(categoria => categoria.OrganizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(categoria => !categoria.EstaEliminado);
        }
    }
}
