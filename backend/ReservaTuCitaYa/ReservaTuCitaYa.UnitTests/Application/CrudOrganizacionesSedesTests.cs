using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;
using ReservaTuCitaYa.Application.DTOs.Sedes;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class CrudOrganizacionesSedesTests
{
    [Fact]
    public async Task CrearOrganizacion_ConDatosValidos_NormalizaYGuarda()
    {
        var repositorio = new RepositorioOrganizacionesFalso();
        var tipo = repositorio.AgregarTipo("Consultorio");
        var servicio = new OrganizacionService(repositorio);

        var resultado = await servicio.CrearAsync(new CrearOrganizacionSolicitud
        {
            TipoOrganizacionId = tipo.Id,
            NombreComercial = "  Salud Central  ",
            NumeroDocumento = " 20123456789 ",
            Correo = " contacto@salud.pe "
        });

        Assert.True(resultado.EsExitoso);
        var organizacion = Assert.Single(repositorio.Organizaciones);
        Assert.Equal("Salud Central", organizacion.NombreComercial);
        Assert.Equal("20123456789", organizacion.NumeroDocumento);
        Assert.Equal("contacto@salud.pe", organizacion.Correo);
    }

    [Fact]
    public async Task CrearOrganizacion_DocumentoDeEliminadaSigueReservado_DevuelveConflicto()
    {
        var repositorio = new RepositorioOrganizacionesFalso();
        var tipo = repositorio.AgregarTipo("Clínica");
        repositorio.Organizaciones.Add(new Organizacion
        {
            TipoOrganizacionId = tipo.Id,
            NombreComercial = "Anterior",
            NumeroDocumento = "20123456789",
            EstaEliminado = true
        });

        var resultado = await new OrganizacionService(repositorio).CrearAsync(
            SolicitudOrganizacion(tipo.Id, "20123456789"));

        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Fact]
    public async Task CrearOrganizacion_TipoInactivo_EsRechazada()
    {
        var repositorio = new RepositorioOrganizacionesFalso();
        var tipo = repositorio.AgregarTipo("Clínica", activo: false);

        var resultado = await new OrganizacionService(repositorio).CrearAsync(
            SolicitudOrganizacion(tipo.Id, "20987654321"));

        Assert.False(resultado.EsExitoso);
        Assert.Empty(repositorio.Organizaciones);
    }

    [Fact]
    public async Task EliminarOrganizacion_AplicaEliminacionLogica()
    {
        var repositorio = new RepositorioOrganizacionesFalso();
        var tipo = repositorio.AgregarTipo("Centro");
        var organizacion = new Organizacion
        {
            TipoOrganizacionId = tipo.Id,
            NombreComercial = "Centro Norte",
            NumeroDocumento = "20111111111"
        };
        repositorio.Organizaciones.Add(organizacion);

        var resultado = await new OrganizacionService(repositorio).EliminarAsync(organizacion.Id);

        Assert.True(resultado.EsExitoso);
        Assert.True(organizacion.EstaEliminado);
        Assert.False(organizacion.EstaActivo);
        Assert.NotNull(organizacion.FechaModificacion);
    }

    [Fact]
    public async Task CrearSede_OrganizacionInactiva_EsRechazada()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var tipo = organizaciones.AgregarTipo("Centro");
        var organizacion = CrearOrganizacion(tipo.Id, activo: false);
        organizaciones.Organizaciones.Add(organizacion);
        var sedes = new RepositorioSedesFalso();

        var resultado = await new SedeService(sedes, organizaciones).CrearAsync(
            SolicitudSede(organizacion.Id, "Principal"));

        Assert.False(resultado.EsExitoso);
        Assert.Empty(sedes.Sedes);
    }

    [Fact]
    public async Task CrearSede_NombreActivoDuplicadoEnMismaOrganizacion_DevuelveConflicto()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var tipo = organizaciones.AgregarTipo("Centro");
        var organizacion = CrearOrganizacion(tipo.Id);
        organizaciones.Organizaciones.Add(organizacion);
        var sedes = new RepositorioSedesFalso();
        sedes.Sedes.Add(new Sede
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Principal",
            Direccion = "Av. Uno"
        });

        var resultado = await new SedeService(sedes, organizaciones).CrearAsync(
            SolicitudSede(organizacion.Id, " principal "));

        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Fact]
    public async Task CrearSede_MismoNombreInactivo_EstaPermitido()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var tipo = organizaciones.AgregarTipo("Centro");
        var organizacion = CrearOrganizacion(tipo.Id);
        organizaciones.Organizaciones.Add(organizacion);
        var sedes = new RepositorioSedesFalso();
        sedes.Sedes.Add(new Sede
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Principal",
            Direccion = "Av. Antigua",
            EstaActivo = false
        });

        var resultado = await new SedeService(sedes, organizaciones).CrearAsync(
            SolicitudSede(organizacion.Id, "Principal"));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(2, sedes.Sedes.Count);
    }

    [Fact]
    public async Task ActivarSede_SiExisteOtraActivaConMismoNombre_DevuelveConflicto()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var tipo = organizaciones.AgregarTipo("Centro");
        var organizacion = CrearOrganizacion(tipo.Id);
        organizaciones.Organizaciones.Add(organizacion);
        var sedes = new RepositorioSedesFalso();
        var sedeInactiva = new Sede
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Principal",
            Direccion = "Av. Antigua",
            EstaActivo = false
        };
        sedes.Sedes.Add(sedeInactiva);
        sedes.Sedes.Add(new Sede
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Principal",
            Direccion = "Av. Nueva"
        });

        var resultado = await new SedeService(sedes, organizaciones)
            .CambiarEstadoAsync(sedeInactiva.Id);

        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
        Assert.False(sedeInactiva.EstaActivo);
    }

    [Fact]
    public async Task EliminarSede_AplicaEliminacionLogica()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var sedes = new RepositorioSedesFalso();
        var sede = new Sede { Nombre = "Norte", Direccion = "Av. Norte" };
        sedes.Sedes.Add(sede);

        var resultado = await new SedeService(sedes, organizaciones).EliminarAsync(sede.Id);

        Assert.True(resultado.EsExitoso);
        Assert.True(sede.EstaEliminado);
        Assert.False(sede.EstaActivo);
    }

    private static CrearOrganizacionSolicitud SolicitudOrganizacion(Guid tipoId, string documento) =>
        new()
        {
            TipoOrganizacionId = tipoId,
            NombreComercial = "Organización de prueba",
            NumeroDocumento = documento
        };

    private static CrearSedeSolicitud SolicitudSede(Guid organizacionId, string nombre) =>
        new()
        {
            OrganizacionId = organizacionId,
            Nombre = nombre,
            Direccion = "Av. Principal 123"
        };

    private static Organizacion CrearOrganizacion(Guid tipoId, bool activo = true) =>
        new()
        {
            TipoOrganizacionId = tipoId,
            NombreComercial = "Centro de prueba",
            NumeroDocumento = Guid.NewGuid().ToString("N")[..20],
            EstaActivo = activo
        };
}

internal sealed class RepositorioOrganizacionesFalso : IOrganizacionRepository
{
    public List<Organizacion> Organizaciones { get; } = [];
    public List<TipoOrganizacion> Tipos { get; } = [];

    public TipoOrganizacion AgregarTipo(string nombre, bool activo = true)
    {
        var tipo = new TipoOrganizacion { Nombre = nombre, EstaActivo = activo };
        Tipos.Add(tipo);
        return tipo;
    }

    public Task<IReadOnlyList<OrganizacionListaDto>> ListarAsync(OrganizacionFiltroDto filtro, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<OrganizacionListaDto>>([]);

    public Task<OrganizacionDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<OrganizacionDetalleDto?>(null);

    public Task<Organizacion?> ObtenerParaModificarAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Organizaciones.SingleOrDefault(x => x.Id == id));

    public Task<bool> ExisteDocumentoAsync(string numeroDocumento, Guid? excluirId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Organizaciones.Any(x => x.Id != excluirId &&
            x.NumeroDocumento.Equals(numeroDocumento, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> TipoValidoAsync(Guid tipoOrganizacionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tipos.Any(x => x.Id == tipoOrganizacionId && x.EstaActivo && !x.EstaEliminado));

    public Task<IReadOnlyList<TipoOrganizacionOpcionDto>> ListarTiposActivosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TipoOrganizacionOpcionDto>>(Tipos
            .Where(x => x.EstaActivo && !x.EstaEliminado)
            .Select(x => new TipoOrganizacionOpcionDto(x.Id, x.Nombre)).ToArray());

    public void Agregar(Organizacion organizacion) => Organizaciones.Add(organizacion);
    public Task GuardarAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class RepositorioSedesFalso : ISedeRepository
{
    public List<Sede> Sedes { get; } = [];

    public Task<IReadOnlyList<SedeListaDto>> ListarAsync(SedeFiltroDto filtro, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SedeListaDto>>([]);

    public Task<SedeDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<SedeDetalleDto?>(null);

    public Task<Sede?> ObtenerParaModificarAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sedes.SingleOrDefault(x => x.Id == id));

    public Task<bool> ExisteNombreActivoAsync(Guid organizacionId, string nombre, Guid? excluirId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sedes.Any(x => x.Id != excluirId && x.OrganizacionId == organizacionId &&
            x.EstaActivo && !x.EstaEliminado && x.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)));

    public void Agregar(Sede sede) => Sedes.Add(sede);
    public Task GuardarAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
