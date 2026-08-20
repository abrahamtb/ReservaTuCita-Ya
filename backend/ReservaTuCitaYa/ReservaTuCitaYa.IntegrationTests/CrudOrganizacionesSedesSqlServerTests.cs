using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Repositories;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class CrudOrganizacionesSedesSqlServerTests : IAsyncLifetime
{
    private const string CadenaConexion =
        "Server=(localdb)\\MSSQLLocalDB;Database=ReservaTuCitaYa_RG016_Pruebas;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

    private readonly DbContextOptions<ApplicationDbContext> _opciones =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(CadenaConexion)
            .Options;

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
    public async Task CrudCompleto_PersisteYConsultaOrganizacionYSede()
    {
        await using var contexto = CrearContexto();
        var tipo = new TipoOrganizacion { Nombre = "Consultorio" };
        contexto.TiposOrganizacion.Add(tipo);
        await contexto.SaveChangesAsync();

        var organizaciones = new OrganizacionService(new OrganizacionRepository(contexto));
        var resultadoOrganizacion = await organizaciones.CrearAsync(new CrearOrganizacionSolicitud
        {
            TipoOrganizacionId = tipo.Id,
            NombreComercial = "Salud Central",
            NumeroDocumento = "20123456789",
            Correo = "contacto@saludcentral.pe"
        });
        Assert.True(resultadoOrganizacion.EsExitoso);

        var sedes = new SedeService(
            new SedeRepository(contexto),
            new OrganizacionRepository(contexto));
        var resultadoSede = await sedes.CrearAsync(new CrearSedeSolicitud
        {
            OrganizacionId = resultadoOrganizacion.Valor,
            Nombre = "Sede Principal",
            Direccion = "Av. Principal 123"
        });
        Assert.True(resultadoSede.EsExitoso);

        contexto.ChangeTracker.Clear();
        var detalle = await organizaciones.ObtenerPorIdAsync(resultadoOrganizacion.Valor);
        var listaSedes = await sedes.ListarPorOrganizacionAsync(new SedeFiltroDto(
            resultadoOrganizacion.Valor,
            "Principal",
            EstadoFiltro.Activos));

        Assert.True(detalle.EsExitoso);
        Assert.Equal(1, detalle.Valor!.CantidadSedesActivas);
        Assert.True(listaSedes.EsExitoso);
        Assert.Single(listaSedes.Valor!);
    }

    [Fact]
    public async Task RestriccionesSql_DetectanDocumentosYNombresActivosDuplicados()
    {
        await using var contexto = CrearContexto();
        var tipo = new TipoOrganizacion { Nombre = "Clínica" };
        var organizacion = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Clínica Uno",
            NumeroDocumento = "20999999991"
        };
        contexto.Organizaciones.Add(organizacion);
        await contexto.SaveChangesAsync();

        var organizaciones = new OrganizacionService(new OrganizacionRepository(contexto));
        var documentoDuplicado = await organizaciones.CrearAsync(new CrearOrganizacionSolicitud
        {
            TipoOrganizacionId = tipo.Id,
            NombreComercial = "Clínica Dos",
            NumeroDocumento = organizacion.NumeroDocumento
        });
        Assert.False(documentoDuplicado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, documentoDuplicado.TipoError);

        var sedes = new SedeService(
            new SedeRepository(contexto),
            new OrganizacionRepository(contexto));
        var primera = await sedes.CrearAsync(new CrearSedeSolicitud
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Centro",
            Direccion = "Av. Uno"
        });
        var duplicada = await sedes.CrearAsync(new CrearSedeSolicitud
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Centro",
            Direccion = "Av. Dos"
        });

        Assert.True(primera.EsExitoso);
        Assert.False(duplicada.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, duplicada.TipoError);
    }

    [Fact]
    public async Task EliminacionLogica_OcultaRegistrosConFiltrosGlobales()
    {
        await using var contexto = CrearContexto();
        var tipo = new TipoOrganizacion { Nombre = "Centro médico" };
        var organizacion = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Centro Norte",
            NumeroDocumento = "20888888881"
        };
        contexto.Organizaciones.Add(organizacion);
        await contexto.SaveChangesAsync();

        var servicio = new OrganizacionService(new OrganizacionRepository(contexto));
        var eliminado = await servicio.EliminarAsync(organizacion.Id);
        contexto.ChangeTracker.Clear();

        Assert.True(eliminado.EsExitoso);
        Assert.False(await contexto.Organizaciones.AnyAsync(x => x.Id == organizacion.Id));
        Assert.True(await contexto.Organizaciones.IgnoreQueryFilters()
            .AnyAsync(x => x.Id == organizacion.Id && x.EstaEliminado && !x.EstaActivo));
    }

    private ApplicationDbContext CrearContexto() => new(_opciones);
}
