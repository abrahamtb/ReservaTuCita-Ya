using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Infrastructure.Data.Seed;

/// <summary>Información ficticia, idempotente y exclusiva para el entorno local de demostración.</summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IConfiguration configuration, ILogger logger, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("DemoData:Enabled")) return;
        if (await db.Clientes.IgnoreQueryFilters().AnyAsync(x => x.Correo == "ana.lopez@demo.reservatucitaya.pe", cancellationToken)) return;

        var tipo = await db.TiposOrganizacion.FirstOrDefaultAsync(cancellationToken);
        if (tipo is null)
        {
            tipo = new TipoOrganizacion { Nombre = "Barbería", Descripcion = "Negocio de demostración" };
            db.TiposOrganizacion.Add(tipo);
        }

        var organizacion = await db.Organizaciones.FirstOrDefaultAsync(x => x.NombreComercial == "Barbería Central", cancellationToken)
            ?? new Organizacion { TipoOrganizacion = tipo, NombreComercial = "Barbería Central", RazonSocial = "Barbería Central Demo S.A.C.", NumeroDocumento = "20999999991", Telefono = "987 654 321", Correo = "hola@barberiacentral.demo", DireccionPrincipal = "Av. Larco 728, Miraflores" };
        if (organizacion.Id == Guid.Empty || !await db.Organizaciones.AnyAsync(x => x.Id == organizacion.Id, cancellationToken)) db.Organizaciones.Add(organizacion);

        var sedeMiraflores = new Sede { Organizacion = organizacion, Nombre = "Miraflores", Direccion = "Av. Larco 728, Miraflores", Telefono = "987 654 321" };
        var sedeSanIsidro = new Sede { Organizacion = organizacion, Nombre = "San Isidro", Direccion = "Av. Javier Prado 420, San Isidro", Telefono = "987 654 322" };
        db.Sedes.AddRange(sedeMiraflores, sedeSanIsidro);

        var categoria = new CategoriaServicio { Organizacion = organizacion, Nombre = "Cortes y barbería", Descripcion = "Servicios de demostración" };
        db.CategoriasServicio.Add(categoria);
        var corte = Servicio(organizacion, categoria, "Corte clásico - Demo", 45, 45m, 15m);
        var corteBarba = Servicio(organizacion, categoria, "Corte + barba - Demo", 60, 65m, 20m);
        var barba = Servicio(organizacion, categoria, "Barba premium - Demo", 30, 35m, 10m);
        db.Servicios.AddRange(corte, corteBarba, barba);
        db.ServiciosSede.AddRange(
            new ServicioSede { Servicio = corte, Sede = sedeMiraflores }, new ServicioSede { Servicio = corte, Sede = sedeSanIsidro },
            new ServicioSede { Servicio = corteBarba, Sede = sedeMiraflores }, new ServicioSede { Servicio = barba, Sede = sedeMiraflores });

        var javier = Profesional(organizacion, "Javier", "Morales", "70100001", "Barbero senior");
        var lucia = Profesional(organizacion, "Lucía", "Medina", "70100002", "Estilista");
        var renato = Profesional(organizacion, "Renato", "Cruz", "70100003", "Barbero");
        db.Empleados.AddRange(javier, lucia, renato);
        db.EmpleadosSede.AddRange(new EmpleadoSede { Empleado = javier, Sede = sedeMiraflores }, new EmpleadoSede { Empleado = lucia, Sede = sedeMiraflores }, new EmpleadoSede { Empleado = renato, Sede = sedeSanIsidro });
        db.ProfesionalesServicio.AddRange(new ProfesionalServicio { Empleado = javier, Servicio = corte }, new ProfesionalServicio { Empleado = javier, Servicio = corteBarba }, new ProfesionalServicio { Empleado = lucia, Servicio = corte }, new ProfesionalServicio { Empleado = lucia, Servicio = barba }, new ProfesionalServicio { Empleado = renato, Servicio = corteBarba });

        var clientes = new[]
        {
            Cliente(organizacion, "Ana", "López", "72458123", "ana.lopez@demo.reservatucitaya.pe"), Cliente(organizacion, "Luis", "Vega", "48372109", "luis.vega@demo.reservatucitaya.pe"),
            Cliente(organizacion, "María", "Ponce", "10394821", "maria.ponce@demo.reservatucitaya.pe"), Cliente(organizacion, "José", "Pérez", "73190284", "jose.perez@demo.reservatucitaya.pe"),
            Cliente(organizacion, "Carla", "Ruiz", "66492831", "carla.ruiz@demo.reservatucitaya.pe"), Cliente(organizacion, "Diego", "García", "51672839", "diego.garcia@demo.reservatucitaya.pe"),
            Cliente(organizacion, "Valeria", "Flores", "45819273", "valeria.flores@demo.reservatucitaya.pe"), Cliente(organizacion, "Sofía", "Ramos", "39281746", "sofia.ramos@demo.reservatucitaya.pe")
        };
        db.Clientes.AddRange(clientes);

        var recurso = new Recurso { Organizacion = organizacion, Sede = sedeMiraflores, Nombre = "Sillón Barbería 01", Codigo = "S-01", TipoRecurso = "Puesto", Capacidad = 1, EstadoRecurso = EstadoRecurso.Disponible, UbicacionInterna = "Zona principal" };
        db.Recurso.Add(recurso);
        db.ServiciosRecurso.AddRange(new ServicioRecurso { Servicio = corte, Recurso = recurso }, new ServicioRecurso { Servicio = corteBarba, Recurso = recurso }, new ServicioRecurso { Servicio = barba, Recurso = recurso });

        foreach (var day in new[] { DiaSemana.Lunes, DiaSemana.Martes, DiaSemana.Miercoles, DiaSemana.Jueves, DiaSemana.Viernes, DiaSemana.Sabado })
        {
            db.HorarioSede.Add(new HorarioSede { Sede = sedeMiraflores, DiaSemana = day, HoraInicio = new TimeOnly(9, 0), HoraFin = new TimeOnly(18, 0) });
            db.HorarioSede.Add(new HorarioSede { Sede = sedeSanIsidro, DiaSemana = day, HoraInicio = new TimeOnly(9, 0), HoraFin = new TimeOnly(18, 0) });
            db.HorarioProfesional.AddRange(new HorarioProfesional { Empleado = javier, Sede = sedeMiraflores, DiaSemana = day, HoraInicio = new TimeOnly(9, 0), HoraFin = new TimeOnly(18, 0) }, new HorarioProfesional { Empleado = lucia, Sede = sedeMiraflores, DiaSemana = day, HoraInicio = new TimeOnly(9, 0), HoraFin = new TimeOnly(18, 0) });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        db.Reservas.AddRange(
            Reserva(organizacion, sedeMiraflores, clientes[0], corte, javier, recurso, today, 10, 0, EstadoReserva.Confirmada, "RES-DEMO-1001"),
            Reserva(organizacion, sedeMiraflores, clientes[1], corteBarba, lucia, recurso, today, 12, 0, EstadoReserva.Pendiente, "RES-DEMO-1002"),
            Reserva(organizacion, sedeMiraflores, clientes[2], barba, javier, recurso, today.AddDays(1), 11, 0, EstadoReserva.Confirmada, "RES-DEMO-1003"),
            Reserva(organizacion, sedeMiraflores, clientes[3], corte, lucia, recurso, today.AddDays(-1), 14, 0, EstadoReserva.Atendida, "RES-DEMO-1004"),
            Reserva(organizacion, sedeMiraflores, clientes[4], corteBarba, javier, recurso, today.AddDays(-3), 15, 0, EstadoReserva.Cancelada, "RES-DEMO-1005"));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Datos ficticios de demostración creados para {Organizacion}.", organizacion.NombreComercial);
    }

    private static Servicio Servicio(Organizacion org, CategoriaServicio categoria, string nombre, int minutos, decimal precio, decimal adelanto) => new() { Organizacion = org, CategoriaServicio = categoria, Nombre = nombre, DuracionMinutos = minutos, Precio = precio, MontoAdelanto = adelanto, Modalidad = ModalidadServicio.Presencial, CapacidadMaxima = 1, RequiereProfesional = true, RequiereRecurso = true, PermiteCancelacion = true, PermiteReprogramacion = true, HorasLimiteCancelacion = 24 };
    private static Empleado Profesional(Organizacion org, string nombres, string apellidos, string dni, string cargo) => new() { Organizacion = org, TipoDocumento = TipoDocumento.DNI, NumeroDocumento = dni, Nombres = nombres, Apellidos = apellidos, Correo = $"{nombres.ToLowerInvariant()}@barberiacentral.demo", Telefono = "987 654 321", Cargo = cargo, Especialidad = "Barbería", EsProfesional = true };
    private static Cliente Cliente(Organizacion org, string nombres, string apellidos, string dni, string correo) => new() { Organizacion = org, TipoDocumento = TipoDocumento.DNI, NumeroDocumento = dni, Nombres = nombres, Apellidos = apellidos, Correo = correo, Telefono = "987 654 321", Direccion = "Miraflores, Lima" };
    private static Reserva Reserva(Organizacion org, Sede sede, Cliente cliente, Servicio servicio, Empleado profesional, Recurso recurso, DateOnly fecha, int hora, int minuto, EstadoReserva estado, string codigo) { var inicio = new TimeOnly(hora, minuto); var fin = inicio.AddMinutes(servicio.DuracionMinutos); return new Reserva { Organizacion = org, Sede = sede, Cliente = cliente, Servicio = servicio, Profesional = profesional, Recurso = recurso, Codigo = codigo, Fecha = fecha, HoraInicio = inicio, HoraFinServicio = fin, HoraInicioOcupacion = inicio, HoraFinOcupacion = fin, DuracionMinutos = servicio.DuracionMinutos, PrecioTotal = servicio.Precio, AdelantoRequerido = servicio.MontoAdelanto, CapacidadMaxima = 1, CantidadParticipantes = 1, EstadoReserva = estado }; }
}
