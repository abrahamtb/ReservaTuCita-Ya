using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Identity;
using ReservaTuCitaYa.IntegrationTests.Api;

namespace ReservaTuCitaYa.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<Reserva> CrearReservaParaAtencionAsync(
        IServiceProvider services,
        Guid organizacionId,
        EstadoReserva estado = EstadoReserva.Confirmada,
        string sufijo = "At")
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // =========================
        // SEDE
        // =========================
        var sede = new Sede
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            Nombre = $"Sede-{sufijo}",
            Direccion = "Direccion de prueba",
            Telefono = "999999999",
            Correo = $"sede-{Guid.NewGuid():N}@test.com",
            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        // =========================
        // CLIENTE
        // =========================
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = Random.Shared
                .Next(10000000, 99999999)
                .ToString(),

            Nombres = "Cliente",
            Apellidos = sufijo,
            Correo = $"cliente-{Guid.NewGuid():N}@test.com",
            Telefono = "988888888",

            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        // =========================
        // CATEGORIA
        // =========================
        var categoria = new CategoriaServicio
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            Nombre = $"Categoria-{sufijo}-{Guid.NewGuid():N}"[..25],

            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        // =========================
        // SERVICIO
        // =========================
        var servicio = new Servicio
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            CategoriaServicioId = categoria.Id,

            Nombre = $"Servicio-{sufijo}",
            Descripcion = "Servicio para pruebas de RG025",

            DuracionMinutos = 30,
            Precio = 25m,

            Modalidad = ModalidadServicio.Presencial,

            EsGrupal = false,
            CapacidadMaxima = 1,

            RequiereProfesional = false,
            RequiereRecurso = false,

            PermiteCancelacion = true,
            PermiteReprogramacion = true,

            HorasLimiteCancelacion = 24,

            TiempoPreparacionMinutos = 0,
            TiempoPosteriorMinutos = 0,

            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        // Primero agregamos las entidades necesarias
        db.Sedes.Add(sede);
        db.Clientes.Add(cliente);
        db.CategoriasServicio.Add(categoria);
        db.Servicios.Add(servicio);

        // =========================
        // RESERVA
        // =========================
        var reserva = new Reserva
        {
            Id = Guid.NewGuid(),

            Codigo =
                $"RES-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",

            OrganizacionId = organizacionId,
            SedeId = sede.Id,
            ClienteId = cliente.Id,
            ServicioId = servicio.Id,

            ProfesionalId = null,
            RecursoId = null,

            // Hoy para que MarcarPresenteAsync no la rechace
            Fecha = DateOnly.FromDateTime(DateTime.Today),

            HoraInicio = new TimeOnly(10, 0),
            HoraFinServicio = new TimeOnly(10, 30),

            HoraInicioOcupacion = new TimeOnly(10, 0),
            HoraFinOcupacion = new TimeOnly(10, 30),

            DuracionMinutos = 30,
            TiempoPreparacionMinutos = 0,
            TiempoPosteriorMinutos = 0,

            PrecioTotal = 25m,
            AdelantoRequerido = null,

            EsGrupal = false,
            CapacidadMaxima = 1,
            CantidadParticipantes = 1,

            EstadoReserva = estado,

            Observaciones = "Reserva creada para pruebas RG025",

            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        db.Reservas.Add(reserva);

        // =========================
        // PARTICIPANTE
        // =========================
        var participante = new ReservaParticipante
        {
            Id = Guid.NewGuid(),
            ReservaId = reserva.Id,
            ClienteId = cliente.Id,

            NombreCompleto =
                $"{cliente.Nombres} {cliente.Apellidos}",

            EsTitular = true,
            Observaciones = "Participante de prueba",

            EstaActivo = true,
            EstaEliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        db.ReservaParticipantes.Add(participante);

        await db.SaveChangesAsync();

        return reserva;
    }
    public static async Task<ApplicationUser> CrearAdminDeOrganizacionAsync(
    IServiceProvider services,
    Guid organizacionId,
    string sufijo)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var email =
            $"admin-{sufijo}-{Guid.NewGuid():N}@test.local";

        var usuario = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nombres = "Administrador",
            Apellidos = sufijo,
            NumeroDocumento =
                Random.Shared.Next(10000000, 99999999).ToString(),
            Telefono = "999999999",
            EstaActivo = true,
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(
            usuario,
            ReservaTuCitaYaApiFactory.AdminPassword);

        if (!resultado.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    resultado.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(
            usuario,
            RoleNames.Administrador);

        db.UsuariosOrganizaciones.Add(
            new UsuarioOrganizacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                OrganizacionId = organizacionId,
                EsPrincipal = true,

                EstaActivo = true,
                EstaEliminado = false,
                FechaCreacion = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        return usuario;
    }
}