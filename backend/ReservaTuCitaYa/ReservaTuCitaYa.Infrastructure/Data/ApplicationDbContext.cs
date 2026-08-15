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
        public DbSet<ServicioRecurso> ServiciosRecurso => Set<ServicioRecurso>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Empleado> Empleados => Set<Empleado>();
        public DbSet<ProfesionalServicio> ProfesionalesServicio => Set<ProfesionalServicio>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UsuarioOrganizacion> UsuariosOrganizaciones => Set<UsuarioOrganizacion>();
        public DbSet<UsuarioEmpleado> UsuariosEmpleados => Set<UsuarioEmpleado>();
        public DbSet<UsuarioCliente> UsuariosClientes => Set<UsuarioCliente>();
        public DbSet<EmpleadoSede> EmpleadosSede => Set<EmpleadoSede>();
        public DbSet<ExcepcionHorarioSede> ExcepcionHorarioSede => Set<ExcepcionHorarioSede>();
        public DbSet<ExcepcionHorarioRecurso> ExcepcionesHorarioRecurso => Set<ExcepcionHorarioRecurso>();
        public DbSet<ExcepcionHorarioProfesional> ExcepcionHorarioProfesional => Set<ExcepcionHorarioProfesional>();
        public DbSet<ReprogramacionReserva> ReprogramacionesReserva => Set<ReprogramacionReserva>();
        public DbSet<CancelacionReserva> CancelacionesReserva => Set<CancelacionReserva>();
        public DbSet<Reserva> Reservas => Set<Reserva>();
        public DbSet<ReservaParticipante> ReservaParticipantes => Set<ReservaParticipante>();
        public DbSet<HistorialReserva> HistorialReservas => Set<HistorialReserva>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
