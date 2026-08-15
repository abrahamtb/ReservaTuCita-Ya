using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PrecioTotal).HasPrecision(18, 2);
        builder.Property(x => x.AdelantoRequerido).HasPrecision(18, 2);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => new { x.OrganizacionId, x.Fecha, x.HoraInicio });
        builder.HasIndex(x => new { x.ProfesionalId, x.Fecha, x.HoraInicioOcupacion, x.HoraFinOcupacion });
        builder.HasIndex(x => new { x.RecursoId, x.Fecha, x.HoraInicioOcupacion, x.HoraFinOcupacion });
        builder.HasOne(x => x.Organizacion).WithMany().HasForeignKey(x => x.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sede).WithMany().HasForeignKey(x => x.SedeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Servicio).WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Profesional).WithMany().HasForeignKey(x => x.ProfesionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Recurso).WithMany().HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Organizacion.EstaEliminado && !x.Sede.EstaEliminado &&
            !x.Cliente.EstaEliminado && !x.Servicio.EstaEliminado);
    }
}

public sealed class ReservaParticipanteConfiguration : IEntityTypeConfiguration<ReservaParticipante>
{
    public void Configure(EntityTypeBuilder<ReservaParticipante> builder)
    {
        builder.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Observaciones).HasMaxLength(500);
        builder.HasOne(x => x.Reserva).WithMany(x => x.Participantes).HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Reserva.EstaEliminado);
    }
}

public sealed class HistorialReservaConfiguration : IEntityTypeConfiguration<HistorialReserva>
{
    public void Configure(EntityTypeBuilder<HistorialReserva> builder)
    {
        builder.Property(x => x.Motivo).HasMaxLength(250);
        builder.Property(x => x.Observacion).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ReservaId, x.FechaAccion });
        builder.HasOne(x => x.Reserva).WithMany(x => x.Historial).HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Reserva.EstaEliminado);
    }
}

public sealed class CancelacionReservaConfiguration : IEntityTypeConfiguration<CancelacionReserva>
{
    public void Configure(EntityTypeBuilder<CancelacionReserva> builder)
    {
        builder.Property(x => x.Comentario).HasMaxLength(1000);
        builder.Property(x => x.PoliticaAplicada).HasMaxLength(500);
        builder.HasIndex(x => x.ReservaId).IsUnique();
        builder.HasOne(x => x.Reserva).WithMany().HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Reserva.EstaEliminado);
    }
}

public sealed class ReprogramacionReservaConfiguration : IEntityTypeConfiguration<ReprogramacionReserva>
{
    public void Configure(EntityTypeBuilder<ReprogramacionReserva> builder)
    {
        builder.Property(x => x.Observacion).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ReservaId, x.FechaReprogramacion });
        builder.HasOne<Reserva>().WithMany().HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProfesionalAnterior).WithMany().HasForeignKey(x => x.ProfesionalAnteriorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProfesionalNuevo).WithMany().HasForeignKey(x => x.ProfesionalNuevoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecursoAnterior).WithMany().HasForeignKey(x => x.RecursoAnteriorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecursoNuevo).WithMany().HasForeignKey(x => x.RecursoNuevoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado);
    }
}
