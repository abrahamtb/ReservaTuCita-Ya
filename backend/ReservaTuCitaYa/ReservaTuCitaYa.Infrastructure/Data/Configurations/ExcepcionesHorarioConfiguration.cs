using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.Infrastructure.Data.Configurations;

public sealed class ExcepcionHorarioSedeConfiguration : IEntityTypeConfiguration<ExcepcionHorarioSede>
{
    public void Configure(EntityTypeBuilder<ExcepcionHorarioSede> builder)
    {
        ConfigurarBase(builder);
        builder.HasIndex(x => new { x.SedeId, x.Fecha });
        builder.HasOne(x => x.Sede).WithMany().HasForeignKey(x => x.SedeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Sede.EstaEliminado);
    }

    internal static void ConfigurarBase<T>(EntityTypeBuilder<T> builder) where T : ReservaTuCitaYa.Domain.Common.BaseEntity
    {
        builder.Property("Fecha").HasColumnType("date");
        builder.Property("HoraInicio").HasColumnType("time");
        builder.Property("HoraFin").HasColumnType("time");
        builder.Property("Motivo").HasMaxLength(250).IsRequired();
        builder.Property("Observaciones").HasMaxLength(500);
    }
}

public sealed class ExcepcionHorarioProfesionalConfiguration : IEntityTypeConfiguration<ExcepcionHorarioProfesional>
{
    public void Configure(EntityTypeBuilder<ExcepcionHorarioProfesional> builder)
    {
        ExcepcionHorarioSedeConfiguration.ConfigurarBase(builder);
        builder.HasIndex(x => new { x.EmpleadoId, x.SedeId, x.Fecha });
        builder.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.EmpleadoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sede).WithMany().HasForeignKey(x => x.SedeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Empleado.EstaEliminado && !x.Sede.EstaEliminado);
    }
}

public sealed class ExcepcionHorarioRecursoConfiguration : IEntityTypeConfiguration<ExcepcionHorarioRecurso>
{
    public void Configure(EntityTypeBuilder<ExcepcionHorarioRecurso> builder)
    {
        ExcepcionHorarioSedeConfiguration.ConfigurarBase(builder);
        builder.HasIndex(x => new { x.RecursoId, x.Fecha });
        builder.HasOne(x => x.Recurso).WithMany().HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.EstaEliminado && !x.Recurso.EstaEliminado);
    }
}
