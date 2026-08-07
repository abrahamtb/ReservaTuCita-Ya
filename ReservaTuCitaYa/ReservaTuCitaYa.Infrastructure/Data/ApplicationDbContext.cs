using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TipoOrganizacion> TiposOrganizacion => Set<TipoOrganizacion>();
        public DbSet<Organizacion> Organizaciones => Set<Organizacion>();
        public DbSet<Sede> Sedes => Set<Sede>();
        public DbSet<CategoriaServicio> CategoriasServicio => Set<CategoriaServicio>();
        public DbSet<Servicio> Servicios => Set<Servicio>();
        public DbSet<ServicioSede> ServiciosSede => Set<ServicioSede>();
        public DbSet<BloqueoProfesional> BloqueoProfesional => Set<BloqueoProfesional>();
        public DbSet<BloqueoRecurso> BloqueoRecurso => Set<BloqueoRecurso>();
        public DbSet<HorarioProfesional> HorarioProfesional => Set<HorarioProfesional>();
        public DbSet<HorarioRecurso> HorarioRecurso => Set<HorarioRecurso>();
        public DbSet<HorarioSede> HorarioSede => Set<HorarioSede>();
        public DbSet<Recurso> Recurso => Set<Recurso>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
