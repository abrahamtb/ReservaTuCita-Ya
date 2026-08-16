using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Repositories;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class ClientesSqlServerTests : IAsyncLifetime
{
    private const string CadenaConexion =
        "Server=(localdb)\\MSSQLLocalDB;Database=ReservaTuCitaYa_RG018_Pruebas;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
    private readonly DbContextOptions<ApplicationDbContext> _opciones =
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(CadenaConexion).Options;

    public async Task InitializeAsync()
    {
        await using var contexto = CrearContexto();
        await contexto.Database.EnsureDeletedAsync();
        await contexto.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var contexto = CrearContexto();
        await contexto.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Persistencia_AplicaFkUnicidadCompuestaYAislamiento()
    {
        await using var contexto = CrearContexto();
        var (organizacionA, organizacionB) = await PrepararOrganizacionesAsync(contexto);
        var servicio = CrearServicio(contexto);

        var primero = await servicio.CrearAsync(Solicitud(organizacionA.Id));
        var duplicado = await servicio.CrearAsync(Solicitud(organizacionA.Id));
        var otroTipo = await servicio.CrearAsync(Solicitud(
            organizacionA.Id, TipoDocumento.Pasaporte));
        var otraOrganizacion = await servicio.CrearAsync(Solicitud(organizacionB.Id));

        Assert.True(primero.EsExitoso);
        Assert.False(duplicado.EsExitoso);
        Assert.True(otroTipo.EsExitoso);
        Assert.True(otraOrganizacion.EsExitoso);
        contexto.ChangeTracker.Clear();
        var persistido = await contexto.Clientes.SingleAsync(c => c.Id == primero.Valor);
        Assert.Equal(organizacionA.Id, persistido.OrganizacionId);

        var listaA = await servicio.ListarAsync(new(organizacionA.Id));
        var listaB = await servicio.ListarAsync(new(organizacionB.Id));
        Assert.Equal(2, listaA.Valor!.TotalElementos);
        Assert.Single(listaB.Valor!.Elementos);
    }

    [Fact]
    public async Task ActualizarEstadoYSoftDelete_SePersistenYRespetanQueryFilter()
    {
        await using var contexto = CrearContexto();
        var (organizacion, _) = await PrepararOrganizacionesAsync(contexto);
        var servicio = CrearServicio(contexto);
        var creado = await servicio.CrearAsync(Solicitud(organizacion.Id));

        Assert.True((await servicio.ActualizarAsync(new ActualizarClienteSolicitud
        {
            Id = creado.Valor,
            TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "87654321",
            Nombres = "Ana María",
            Apellidos = "López",
            Correo = "ana.actualizada@test.local"
        })).EsExitoso);
        Assert.True((await servicio.CambiarEstadoAsync(creado.Valor, false)).EsExitoso);
        contexto.ChangeTracker.Clear();
        var actualizado = await contexto.Clientes.SingleAsync(c => c.Id == creado.Valor);
        Assert.Equal("87654321", actualizado.NumeroDocumento);
        Assert.False(actualizado.EstaActivo);

        Assert.True((await servicio.EliminarAsync(creado.Valor)).EsExitoso);
        contexto.ChangeTracker.Clear();
        Assert.False(await contexto.Clientes.AnyAsync(c => c.Id == creado.Valor));
        Assert.True(await contexto.Clientes.IgnoreQueryFilters()
            .AnyAsync(c => c.Id == creado.Valor && c.EstaEliminado && !c.EstaActivo));

        var reutilizacion = await servicio.CrearAsync(new CrearClienteSolicitud
        {
            OrganizacionId = organizacion.Id,
            TipoDocumento = TipoDocumento.DNI,
            NumeroDocumento = "87654321",
            Nombres = "Otra",
            Apellidos = "Persona"
        });
        Assert.False(reutilizacion.EsExitoso);
    }

    [Fact]
    public async Task Listado_BusquedaFiltrosOrdenYPaginacionSeEjecutanEnSql()
    {
        await using var contexto = CrearContexto();
        var (organizacion, otraOrganizacion) = await PrepararOrganizacionesAsync(contexto);
        contexto.Clientes.AddRange(
            Cliente(organizacion.Id, "11111111", "Beatriz", "Zuluaga", TipoDocumento.DNI, true),
            Cliente(organizacion.Id, "22222222", "Ana", "Alonso", TipoDocumento.Pasaporte, true,
                "ana@test.local", "999123456"),
            Cliente(organizacion.Id, "33333333", "Carlos", "Mendoza", TipoDocumento.DNI, false),
            Cliente(otraOrganizacion.Id, "44444444", "Ana", "Ajena", TipoDocumento.Pasaporte, true));
        await contexto.SaveChangesAsync();
        var repositorio = new ClienteRepository(contexto);

        var busqueda = await repositorio.ListarAsync(new(
            organizacion.Id, "ana@test", TipoDocumento.Pasaporte, EstadoFiltro.Activos));
        var pagina = await repositorio.ListarAsync(new(
            organizacion.Id, Estado: EstadoFiltro.Todos, Pagina: 2, TamanoPagina: 1));

        Assert.Single(busqueda.Elementos);
        Assert.Equal("Ana", busqueda.Elementos[0].Nombres);
        Assert.Single(pagina.Elementos);
        Assert.Equal(3, pagina.TotalElementos);
        Assert.Equal(2, pagina.PaginaActual);
        Assert.DoesNotContain(busqueda.Elementos, c => c.OrganizacionId == otraOrganizacion.Id);
    }

    private ClienteService CrearServicio(ApplicationDbContext contexto) => new(
        new ClienteRepository(contexto), new OrganizacionRepository(contexto));

    private static CrearClienteSolicitud Solicitud(
        Guid organizacionId, TipoDocumento tipo = TipoDocumento.DNI) => new()
    {
        OrganizacionId = organizacionId,
        TipoDocumento = tipo,
        NumeroDocumento = "76543210",
        Nombres = "Ana",
        Apellidos = "López"
    };

    private static Cliente Cliente(
        Guid organizacionId, string documento, string nombres, string apellidos,
        TipoDocumento tipo, bool activo, string? correo = null, string? telefono = null) => new()
    {
        OrganizacionId = organizacionId,
        NumeroDocumento = documento,
        TipoDocumento = tipo,
        Nombres = nombres,
        Apellidos = apellidos,
        Correo = correo,
        Telefono = telefono,
        EstaActivo = activo
    };

    private static async Task<(Organizacion A, Organizacion B)> PrepararOrganizacionesAsync(
        ApplicationDbContext contexto)
    {
        var tipo = new TipoOrganizacion { Nombre = "Centro" };
        var a = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Centro A",
            NumeroDocumento = "20111111111"
        };
        var b = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Centro B",
            NumeroDocumento = "20222222222"
        };
        contexto.AddRange(tipo, a, b);
        await contexto.SaveChangesAsync();
        return (a, b);
    }

    private ApplicationDbContext CrearContexto() => new(_opciones);
}
