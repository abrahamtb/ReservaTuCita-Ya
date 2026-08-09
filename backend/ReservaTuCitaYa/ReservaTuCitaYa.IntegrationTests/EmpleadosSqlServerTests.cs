using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Repositories;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class EmpleadosSqlServerTests : IAsyncLifetime
{
    private const string Cadena =
        "Server=.\\SQLEXPRESS;Database=ReservaTuCitaYa_RG019_Pruebas;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
    private readonly DbContextOptions<ApplicationDbContext> _opciones =
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(Cadena).Options;

    public async Task InitializeAsync()
    {
        await using var db = CrearContexto();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CrearContexto();
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Persistencia_AplicaFkDocumentoUnicoYAislamiento()
    {
        await using var db = CrearContexto();
        var d = await PrepararAsync(db);
        var servicio = App(db);
        var primero = await servicio.CrearAsync(Solicitud(d.OrganizacionA.Id));
        var duplicado = await servicio.CrearAsync(Solicitud(d.OrganizacionA.Id));
        var otroTipo = await servicio.CrearAsync(Solicitud(d.OrganizacionA.Id, TipoDocumento.Pasaporte));
        var otraOrg = await servicio.CrearAsync(Solicitud(d.OrganizacionB.Id));
        Assert.True(primero.EsExitoso);
        Assert.False(duplicado.EsExitoso);
        Assert.True(otroTipo.EsExitoso);
        Assert.True(otraOrg.EsExitoso);
        db.ChangeTracker.Clear();
        Assert.Equal(d.OrganizacionA.Id,
            (await db.Empleados.SingleAsync(e => e.Id == primero.Valor)).OrganizacionId);
        Assert.Equal(2, (await servicio.ListarAsync(new(d.OrganizacionA.Id))).Valor!.TotalElementos);
        Assert.Single((await servicio.ListarAsync(new(d.OrganizacionB.Id))).Valor!.Elementos);
    }

    [Fact]
    public async Task Relaciones_PersistenSonUnicasYValidanOrganizacion()
    {
        await using var db = CrearContexto();
        var d = await PrepararAsync(db);
        var servicio = App(db);
        var creado = await servicio.CrearAsync(new CrearEmpleadoSolicitud
        {
            OrganizacionId = d.OrganizacionA.Id,
            TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "71234567",
            Nombres = "Carlos",
            Apellidos = "Ramirez",
            Cargo = "Barbero",
            EsProfesional = true,
            SedeIds = [d.SedeA.Id],
            ServicioIds = [d.ServicioA.Id]
        });
        Assert.True(creado.EsExitoso);
        db.ChangeTracker.Clear();
        Assert.Single(await db.EmpleadosSede.Where(r => r.EmpleadoId == creado.Valor).ToListAsync());
        Assert.Single(await db.ProfesionalesServicio.Where(r => r.EmpleadoId == creado.Valor).ToListAsync());

        var sedeAjena = await servicio.ReemplazarSedesAsync(creado.Valor, [d.SedeB.Id]);
        var servicioAjeno = await servicio.ReemplazarServiciosAsync(creado.Valor, [d.ServicioB.Id]);
        Assert.False(sedeAjena.EsExitoso);
        Assert.False(servicioAjeno.EsExitoso);

        db.EmpleadosSede.Add(new EmpleadoSede { EmpleadoId = creado.Valor, SedeId = d.SedeA.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        db.ProfesionalesServicio.Add(new ProfesionalServicio
            { EmpleadoId = creado.Valor, ServicioId = d.ServicioA.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Listado_BuscaFiltraSedeServicioProfesionalEstadoYPaginaEnSql()
    {
        await using var db = CrearContexto();
        var d = await PrepararAsync(db);
        db.Empleados.AddRange(
            new Empleado
            {
                OrganizacionId = d.OrganizacionA.Id, TipoDocumento = TipoDocumento.DNI,
                NumeroDocumento = "11111111", Nombres = "Ana", Apellidos = "Alonso",
                Cargo = "Médica", Especialidad = "Cardiología", EsProfesional = true,
                Sedes = [new() { SedeId = d.SedeA.Id }],
                ServiciosProfesionales = [new() { ServicioId = d.ServicioA.Id }]
            },
            new Empleado
            {
                OrganizacionId = d.OrganizacionA.Id, TipoDocumento = TipoDocumento.DNI,
                NumeroDocumento = "22222222", Nombres = "Bruno", Apellidos = "Zuluaga",
                Cargo = "Recepcionista", EsProfesional = false, EstaActivo = false
            },
            new Empleado
            {
                OrganizacionId = d.OrganizacionB.Id, TipoDocumento = TipoDocumento.DNI,
                NumeroDocumento = "33333333", Nombres = "Ana", Apellidos = "Ajena",
                Cargo = "Médica", EsProfesional = true
            });
        await db.SaveChangesAsync();
        var repo = new EmpleadoRepository(db);
        var filtrado = await repo.ListarAsync(new(
            d.OrganizacionA.Id, "Cardio", EsProfesional: true,
            Estado: EstadoFiltro.Activos, SedeId: d.SedeA.Id, ServicioId: d.ServicioA.Id));
        var pagina = await repo.ListarAsync(new(
            d.OrganizacionA.Id, Estado: EstadoFiltro.Todos, Pagina: 2, TamanoPagina: 1));
        Assert.Single(filtrado.Elementos);
        Assert.Equal("Ana", filtrado.Elementos[0].Nombres);
        Assert.Single(pagina.Elementos);
        Assert.Equal(2, pagina.TotalElementos);
        Assert.DoesNotContain(filtrado.Elementos, e => e.OrganizacionId == d.OrganizacionB.Id);
    }

    [Fact]
    public async Task EstadoActualizacionYSoftDelete_PersistenConRelacionesYQueryFilter()
    {
        await using var db = CrearContexto();
        var d = await PrepararAsync(db);
        var servicio = App(db);
        var creado = await servicio.CrearAsync(new CrearEmpleadoSolicitud
        {
            OrganizacionId = d.OrganizacionA.Id, TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "71234567", Nombres = "Carlos", Apellidos = "Ramirez",
            Cargo = "Barbero", EsProfesional = true,
            SedeIds = [d.SedeA.Id], ServicioIds = [d.ServicioA.Id]
        });
        Assert.True((await servicio.ActualizarAsync(new ActualizarEmpleadoSolicitud
        {
            Id = creado.Valor, TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "87654321", Nombres = "Carlos Alberto",
            Apellidos = "Ramirez", Cargo = "Barbero Senior", EsProfesional = true
        })).EsExitoso);
        Assert.True((await servicio.CambiarEstadoAsync(creado.Valor, false)).EsExitoso);
        db.ChangeTracker.Clear();
        var persistido = await db.Empleados.SingleAsync(e => e.Id == creado.Valor);
        Assert.Equal("87654321", persistido.NumeroDocumento);
        Assert.False(persistido.EstaActivo);

        Assert.True((await servicio.EliminarAsync(creado.Valor)).EsExitoso);
        db.ChangeTracker.Clear();
        Assert.False(await db.Empleados.AnyAsync(e => e.Id == creado.Valor));
        Assert.True(await db.Empleados.IgnoreQueryFilters().AnyAsync(e =>
            e.Id == creado.Valor && e.EstaEliminado));
        Assert.True(await db.EmpleadosSede.IgnoreQueryFilters().AnyAsync(r =>
            r.EmpleadoId == creado.Valor && r.EstaEliminado));
        Assert.True(await db.ProfesionalesServicio.IgnoreQueryFilters().AnyAsync(r =>
            r.EmpleadoId == creado.Valor && r.EstaEliminado));
        Assert.False((await servicio.CrearAsync(new CrearEmpleadoSolicitud
        {
            OrganizacionId = d.OrganizacionA.Id, TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "87654321", Nombres = "Otro", Apellidos = "Empleado",
            Cargo = "Cargo"
        })).EsExitoso);
    }

    private EmpleadoService App(ApplicationDbContext db) =>
        new(new EmpleadoRepository(db), new OrganizacionRepository(db));

    private static CrearEmpleadoSolicitud Solicitud(
        Guid organizacionId, TipoDocumento tipo = TipoDocumento.DNI) => new()
    {
        OrganizacionId = organizacionId, TipoDocumento = tipo,
        NumeroDocumento = "71234567", Nombres = "Carlos",
        Apellidos = "Ramirez", Cargo = "Barbero"
    };

    private static async Task<Datos> PrepararAsync(ApplicationDbContext db)
    {
        var tipo = new TipoOrganizacion { Nombre = "Centro" };
        var a = new Organizacion { TipoOrganizacion = tipo, NombreComercial = "A", NumeroDocumento = "20111111111" };
        var b = new Organizacion { TipoOrganizacion = tipo, NombreComercial = "B", NumeroDocumento = "20222222222" };
        var categoriaA = new CategoriaServicio { Organizacion = a, Nombre = "Consultas A" };
        var categoriaB = new CategoriaServicio { Organizacion = b, Nombre = "Consultas B" };
        var sedeA = new Sede { Organizacion = a, Nombre = "Sede A", Direccion = "Lima" };
        var sedeB = new Sede { Organizacion = b, Nombre = "Sede B", Direccion = "Lima" };
        var servicioA = new Servicio { Organizacion = a, CategoriaServicio = categoriaA, Nombre = "Servicio A", DuracionMinutos = 30, Precio = 50, CapacidadMaxima = 1 };
        var servicioB = new Servicio { Organizacion = b, CategoriaServicio = categoriaB, Nombre = "Servicio B", DuracionMinutos = 30, Precio = 50, CapacidadMaxima = 1 };
        db.AddRange(tipo, a, b, categoriaA, categoriaB, sedeA, sedeB, servicioA, servicioB);
        await db.SaveChangesAsync();
        return new(a, b, sedeA, sedeB, servicioA, servicioB);
    }

    private ApplicationDbContext CrearContexto() => new(_opciones);
    private sealed record Datos(
        Organizacion OrganizacionA, Organizacion OrganizacionB,
        Sede SedeA, Sede SedeB, Servicio ServicioA, Servicio ServicioB);
}
