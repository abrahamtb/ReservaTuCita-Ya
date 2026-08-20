using Microsoft.EntityFrameworkCore;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using ReservaTuCitaYa.Infrastructure.Data;
using ReservaTuCitaYa.Infrastructure.Repositories;

namespace ReservaTuCitaYa.IntegrationTests;

public sealed class CrudCategoriasServiciosSqlServerTests : IAsyncLifetime
{
    private const string CadenaConexion =
        "Server=(localdb)\\MSSQLLocalDB;Database=ReservaTuCitaYa_RG017_Pruebas;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
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
    public async Task Categorias_RespetanUnicidadPorOrganizacionYFiltros()
    {
        await using var contexto = CrearContexto();
        var datos = await PrepararAsync(contexto);
        var repositorio = new CategoriaServicioRepository(contexto);
        var servicio = new CategoriaServicioService(repositorio, new OrganizacionRepository(contexto));

        var primera = await servicio.CrearAsync(new()
        {
            OrganizacionId = datos.Organizacion.Id,
            Nombre = "Terapias"
        });
        var duplicada = await servicio.CrearAsync(new()
        {
            OrganizacionId = datos.Organizacion.Id,
            Nombre = "Terapias"
        });
        var otraOrganizacion = await servicio.CrearAsync(new()
        {
            OrganizacionId = datos.OtraOrganizacion.Id,
            Nombre = "Terapias"
        });

        Assert.True(primera.EsExitoso);
        Assert.False(duplicada.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, duplicada.TipoError);
        Assert.True(otraOrganizacion.EsExitoso);

        Assert.True((await servicio.EliminarAsync(primera.Valor)).EsExitoso);
        contexto.ChangeTracker.Clear();
        Assert.False(await contexto.CategoriasServicio.AnyAsync(c => c.Id == primera.Valor));
        Assert.True(await contexto.CategoriasServicio.IgnoreQueryFilters()
            .AnyAsync(c => c.Id == primera.Valor && c.EstaEliminado));
    }

    [Fact]
    public async Task ServicioYSedes_SeCreanYActualizanEnTransaccion()
    {
        await using var contexto = CrearContexto();
        var datos = await PrepararAsync(contexto);
        var servicio = CrearServicioAplicacion(contexto);

        var creado = await servicio.CrearAsync(Solicitud(datos,
        [
            new() { SedeId = datos.SedeUno.Id },
            new() { SedeId = datos.SedeDos.Id, PrecioEspecial = 75 }
        ]));
        Assert.True(creado.EsExitoso);
        contexto.ChangeTracker.Clear();

        var detalle = await servicio.ObtenerPorIdAsync(creado.Valor);
        Assert.True(detalle.EsExitoso);
        Assert.Equal(2, detalle.Valor!.Sedes.Count);
        Assert.Contains(detalle.Valor.Sedes, sede => sede.PrecioEspecial == 75);
        Assert.Contains(detalle.Valor.Sedes, sede =>
            sede.SedeId == datos.SedeUno.Id &&
            sede.PrecioEspecial is null &&
            sede.PrecioAplicable == 100);

        var listado = await servicio.ListarAsync(new ServicioFiltroDto(
            datos.Organizacion.Id,
            "integral",
            datos.Categoria.Id,
            ModalidadServicio.Presencial,
            EstadoFiltro.Activos));
        Assert.True(listado.EsExitoso);
        Assert.Single(listado.Valor!.Elementos);

        var actualizado = await servicio.ActualizarAsync(Actualizar(creado.Valor, datos,
            [new() { SedeId = datos.SedeDos.Id, PrecioEspecial = 70 }]));
        Assert.True(actualizado.EsExitoso);
        contexto.ChangeTracker.Clear();

        var relaciones = await contexto.ServiciosSede.IgnoreQueryFilters()
            .Where(relacion => relacion.ServicioId == creado.Valor).ToListAsync();
        Assert.Contains(relaciones, r => r.SedeId == datos.SedeUno.Id && r.EstaEliminado);
        Assert.Contains(relaciones, r => r.SedeId == datos.SedeDos.Id && r.EstaActivo && r.PrecioEspecial == 70);
    }

    [Fact]
    public async Task Servicio_RechazaDuplicadoYRelacionSedeDuplicada()
    {
        await using var contexto = CrearContexto();
        var datos = await PrepararAsync(contexto);
        var servicio = CrearServicioAplicacion(contexto);
        var primera = await servicio.CrearAsync(Solicitud(datos,
            [new() { SedeId = datos.SedeUno.Id }]));
        var servicioDuplicado = await servicio.CrearAsync(Solicitud(datos, []));
        var sedeDuplicada = await servicio.ActualizarAsync(Actualizar(primera.Valor, datos,
        [
            new() { SedeId = datos.SedeUno.Id },
            new() { SedeId = datos.SedeUno.Id }
        ]));

        Assert.True(primera.EsExitoso);
        Assert.False(servicioDuplicado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, servicioDuplicado.TipoError);
        Assert.False(sedeDuplicada.EsExitoso);
    }

    [Theory]
    [InlineData("duracion")]
    [InlineData("precio")]
    [InlineData("adelanto")]
    [InlineData("capacidad")]
    public async Task SqlServer_AplicaChecksDeServicio(string caso)
    {
        await using var contexto = CrearContexto();
        var datos = await PrepararAsync(contexto);
        var entidad = new Servicio
        {
            OrganizacionId = datos.Organizacion.Id,
            CategoriaServicioId = datos.Categoria.Id,
            Nombre = $"Inválido {caso}",
            DuracionMinutos = caso == "duracion" ? 0 : 30,
            Precio = caso == "precio" ? -1 : 50,
            MontoAdelanto = caso == "adelanto" ? 60 : 10,
            Modalidad = ModalidadServicio.Presencial,
            EsGrupal = false,
            CapacidadMaxima = caso == "capacidad" ? 2 : 1
        };
        contexto.Servicios.Add(entidad);
        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
    }

    [Fact]
    public async Task EliminarServicio_LoExcluyeSinBorrarRelaciones()
    {
        await using var contexto = CrearContexto();
        var datos = await PrepararAsync(contexto);
        var servicio = CrearServicioAplicacion(contexto);
        var creado = await servicio.CrearAsync(Solicitud(datos,
            [new() { SedeId = datos.SedeUno.Id }]));
        Assert.True((await servicio.EliminarAsync(creado.Valor)).EsExitoso);
        contexto.ChangeTracker.Clear();
        Assert.False(await contexto.Servicios.AnyAsync(s => s.Id == creado.Valor));
        Assert.True(await contexto.Servicios.IgnoreQueryFilters()
            .AnyAsync(s => s.Id == creado.Valor && s.EstaEliminado));
        Assert.True(await contexto.ServiciosSede.AnyAsync(r => r.ServicioId == creado.Valor));
    }

    private ServicioService CrearServicioAplicacion(ApplicationDbContext contexto) => new(
        new ServicioRepository(contexto),
        new CategoriaServicioRepository(contexto),
        new OrganizacionRepository(contexto));

    private static CrearServicioSolicitud Solicitud(
        Datos datos,
        IReadOnlyList<SedeAsignacionSolicitud> sedes) => new()
    {
        OrganizacionId = datos.Organizacion.Id,
        CategoriaServicioId = datos.Categoria.Id,
        Nombre = "Consulta general",
        Descripcion = "Atención integral",
        DuracionMinutos = 30,
        Precio = 100,
        MontoAdelanto = 20,
        Modalidad = ModalidadServicio.Presencial,
        CapacidadMaxima = 1,
        Sedes = sedes
    };

    private static ActualizarServicioSolicitud Actualizar(
        Guid id,
        Datos datos,
        IReadOnlyList<SedeAsignacionSolicitud> sedes) => new()
    {
        Id = id,
        CategoriaServicioId = datos.Categoria.Id,
        Nombre = "Consulta general",
        Descripcion = "Actualizada",
        DuracionMinutos = 45,
        Precio = 100,
        MontoAdelanto = 20,
        Modalidad = ModalidadServicio.Presencial,
        CapacidadMaxima = 1,
        Sedes = sedes
    };

    private static async Task<Datos> PrepararAsync(ApplicationDbContext contexto)
    {
        var tipo = new TipoOrganizacion { Nombre = "Centro" };
        var organizacion = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Centro Uno",
            NumeroDocumento = "20111111111"
        };
        var otraOrganizacion = new Organizacion
        {
            TipoOrganizacion = tipo,
            NombreComercial = "Centro Dos",
            NumeroDocumento = "20222222222"
        };
        var categoria = new CategoriaServicio
        {
            Organizacion = organizacion,
            Nombre = "Consultas"
        };
        var sedeUno = new Sede
        {
            Organizacion = organizacion,
            Nombre = "Principal",
            Direccion = "Av. Uno"
        };
        var sedeDos = new Sede
        {
            Organizacion = organizacion,
            Nombre = "Norte",
            Direccion = "Av. Dos"
        };
        contexto.AddRange(tipo, organizacion, otraOrganizacion, categoria, sedeUno, sedeDos);
        await contexto.SaveChangesAsync();
        return new(organizacion, otraOrganizacion, categoria, sedeUno, sedeDos);
    }

    private ApplicationDbContext CrearContexto() => new(_opciones);
    private sealed record Datos(
        Organizacion Organizacion,
        Organizacion OtraOrganizacion,
        CategoriaServicio Categoria,
        Sede SedeUno,
        Sede SedeDos);
}
