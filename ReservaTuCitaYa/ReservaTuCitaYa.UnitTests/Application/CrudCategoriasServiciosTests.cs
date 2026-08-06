using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class CategoriaServicioServiceTests
{
    [Fact]
    public async Task Crear_Valida_NormalizaYGuarda()
    {
        var escenario = CrearEscenario();
        var resultado = await escenario.Servicio.CrearAsync(new()
        {
            OrganizacionId = escenario.Organizacion.Id,
            Nombre = "  Terapias  ",
            Descripcion = "  Terapias generales  "
        });
        Assert.True(resultado.EsExitoso);
        var categoria = Assert.Single(escenario.Categorias.Categorias);
        Assert.Equal("Terapias", categoria.Nombre);
        Assert.Equal("Terapias generales", categoria.Descripcion);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Crear_RechazaOrganizacionInexistenteOInactiva(bool agregar, bool inactiva)
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var organizacion = CrearOrganizacion(activa: !inactiva);
        if (agregar) organizaciones.Organizaciones.Add(organizacion);
        var categorias = new RepositorioCategoriasFalso();
        var servicio = new CategoriaServicioService(categorias, organizaciones);
        var resultado = await servicio.CrearAsync(new()
        {
            OrganizacionId = organizacion.Id,
            Nombre = "Terapias"
        });
        Assert.False(resultado.EsExitoso);
        Assert.Empty(categorias.Categorias);
    }

    [Fact]
    public async Task Crear_RechazaNombreActivoDuplicadoEnMismaOrganizacion()
    {
        var escenario = CrearEscenario();
        escenario.Categorias.Categorias.Add(CrearCategoria(escenario.Organizacion.Id, "Terapias"));
        var resultado = await escenario.Servicio.CrearAsync(new()
        {
            OrganizacionId = escenario.Organizacion.Id,
            Nombre = "Terapias"
        });
        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Fact]
    public async Task Crear_PermiteMismoNombreEnOtraOrganizacion()
    {
        var escenario = CrearEscenario();
        escenario.Categorias.Categorias.Add(CrearCategoria(Guid.NewGuid(), "Terapias"));
        var resultado = await escenario.Servicio.CrearAsync(new()
        {
            OrganizacionId = escenario.Organizacion.Id,
            Nombre = "Terapias"
        });
        Assert.True(resultado.EsExitoso);
    }

    [Fact]
    public async Task Actualizar_ExcluyeRegistroActualYConservaOrganizacion()
    {
        var escenario = CrearEscenario();
        var categoria = CrearCategoria(escenario.Organizacion.Id, "Terapias");
        escenario.Categorias.Categorias.Add(categoria);
        var resultado = await escenario.Servicio.ActualizarAsync(new()
        {
            Id = categoria.Id,
            Nombre = "Terapias",
            Descripcion = "Actualizada"
        });
        Assert.True(resultado.EsExitoso);
        Assert.Equal(escenario.Organizacion.Id, categoria.OrganizacionId);
        Assert.Equal("Actualizada", categoria.Descripcion);
    }

    [Fact]
    public async Task Desactivar_ConServiciosActivos_RequiereConfirmacion()
    {
        var escenario = CrearEscenario();
        var categoria = CrearCategoria(escenario.Organizacion.Id, "Terapias");
        escenario.Categorias.Categorias.Add(categoria);
        escenario.Categorias.Servicios.Add(CrearServicio(escenario.Organizacion.Id, categoria.Id));
        var sinConfirmar = await escenario.Servicio.CambiarEstadoAsync(categoria.Id, false);
        var confirmado = await escenario.Servicio.CambiarEstadoAsync(categoria.Id, true);
        Assert.False(sinConfirmar.EsExitoso);
        Assert.True(confirmado.EsExitoso);
        Assert.False(categoria.EstaActivo);
    }

    [Fact]
    public async Task Eliminar_SinServiciosActivos_AplicaEliminacionLogica()
    {
        var escenario = CrearEscenario();
        var categoria = CrearCategoria(escenario.Organizacion.Id, "Terapias");
        escenario.Categorias.Categorias.Add(categoria);
        var resultado = await escenario.Servicio.EliminarAsync(categoria.Id);
        Assert.True(resultado.EsExitoso);
        Assert.True(categoria.EstaEliminado);
        Assert.False(categoria.EstaActivo);
    }

    [Fact]
    public async Task Listar_AplicaBusquedaEstadoPaginacionYExcluyeEliminadas()
    {
        var escenario = CrearEscenario();
        escenario.Categorias.Categorias.Add(CrearCategoria(escenario.Organizacion.Id, "Terapias físicas"));
        escenario.Categorias.Categorias.Add(CrearCategoria(escenario.Organizacion.Id, "Terapias eliminadas", eliminada: true));
        escenario.Categorias.Categorias.Add(CrearCategoria(escenario.Organizacion.Id, "Otros", activa: false));
        var resultado = await escenario.Servicio.ListarAsync(new(
            escenario.Organizacion.Id, "Terapias", EstadoFiltro.Activos, 1, 10));
        Assert.True(resultado.EsExitoso);
        Assert.Single(resultado.Valor!.Elementos);
        Assert.Equal("Terapias físicas", resultado.Valor.Elementos[0].Nombre);
    }

    private static (CategoriaServicioService Servicio, RepositorioCategoriasFalso Categorias, Organizacion Organizacion) CrearEscenario()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var organizacion = CrearOrganizacion();
        organizaciones.Organizaciones.Add(organizacion);
        var categorias = new RepositorioCategoriasFalso();
        return (new CategoriaServicioService(categorias, organizaciones), categorias, organizacion);
    }

    internal static Organizacion CrearOrganizacion(bool activa = true) => new()
    {
        NombreComercial = "Centro",
        NumeroDocumento = Guid.NewGuid().ToString("N")[..20],
        EstaActivo = activa
    };

    internal static CategoriaServicio CrearCategoria(Guid organizacionId, string nombre, bool activa = true, bool eliminada = false) => new()
    {
        OrganizacionId = organizacionId,
        Nombre = nombre,
        EstaActivo = activa,
        EstaEliminado = eliminada
    };

    internal static Servicio CrearServicio(Guid organizacionId, Guid categoriaId, string nombre = "Consulta") => new()
    {
        OrganizacionId = organizacionId,
        CategoriaServicioId = categoriaId,
        Nombre = nombre,
        DuracionMinutos = 30,
        Precio = 50,
        Modalidad = ModalidadServicio.Presencial,
        CapacidadMaxima = 1
    };
}

public sealed class ServicioServiceTests
{
    [Fact]
    public async Task Crear_ValidoConVariasSedes_GuardaServicioYRelaciones()
    {
        var e = CrearEscenario();
        var sedeDos = CrearSede(e.Organizacion.Id, "Sur");
        e.Repositorio.Sedes.Add(sedeDos);
        var solicitud = SolicitudValida(e);
        solicitud = Copiar(solicitud, sedes:
        [
            new() { SedeId = e.Sede.Id },
            new() { SedeId = sedeDos.Id, PrecioEspecial = 40 }
        ]);
        var resultado = await e.Servicio.CrearAsync(solicitud);
        Assert.True(resultado.EsExitoso);
        Assert.Single(e.Repositorio.Servicios);
        Assert.Equal(2, e.Repositorio.Relaciones.Count);
        Assert.Contains(e.Repositorio.Relaciones, relacion => relacion.PrecioEspecial == 40);
    }

    [Fact]
    public async Task Crear_RechazaOrganizacionInactiva()
    {
        var e = CrearEscenario(organizacionActiva: false);
        Assert.False((await e.Servicio.CrearAsync(SolicitudValida(e))).EsExitoso);
    }

    [Theory]
    [InlineData("inexistente")]
    [InlineData("otra_organizacion")]
    [InlineData("inactiva")]
    public async Task Crear_RechazaCategoriaInvalida(string caso)
    {
        var e = CrearEscenario();
        var categoriaId = caso switch
        {
            "inexistente" => Guid.NewGuid(),
            "otra_organizacion" => AgregarCategoria(e, Guid.NewGuid(), true).Id,
            _ => AgregarCategoria(e, e.Organizacion.Id, false).Id
        };
        Assert.False((await e.Servicio.CrearAsync(
            Copiar(SolicitudValida(e), categoriaId: categoriaId))).EsExitoso);
    }

    [Fact]
    public async Task Crear_RechazaNombreDuplicado()
    {
        var e = CrearEscenario();
        e.Repositorio.Servicios.Add(CategoriaServicioServiceTests.CrearServicio(
            e.Organizacion.Id, e.Categoria.Id, "Consulta"));
        var resultado = await e.Servicio.CrearAsync(SolicitudValida(e));
        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Theory]
    [InlineData("duracion")]
    [InlineData("precio")]
    [InlineData("adelanto_negativo")]
    [InlineData("adelanto_superior")]
    [InlineData("capacidad_cero")]
    [InlineData("individual_capacidad")]
    [InlineData("cancelacion")]
    [InlineData("preparacion")]
    [InlineData("posterior")]
    public async Task Crear_RechazaValoresNumericosInvalidos(string caso)
    {
        var e = CrearEscenario();
        var baseSolicitud = SolicitudValida(e);
        var solicitud = caso switch
        {
            "duracion" => Copiar(baseSolicitud, duracion: 0),
            "precio" => Copiar(baseSolicitud, precio: -1),
            "adelanto_negativo" => Copiar(baseSolicitud, adelanto: -1),
            "adelanto_superior" => Copiar(baseSolicitud, precio: 10, adelanto: 11),
            "capacidad_cero" => Copiar(baseSolicitud, capacidad: 0),
            "individual_capacidad" => Copiar(baseSolicitud, capacidad: 2),
            "cancelacion" => Copiar(baseSolicitud, cancelacion: -1),
            "preparacion" => Copiar(baseSolicitud, preparacion: -1),
            _ => Copiar(baseSolicitud, posterior: -1)
        };
        Assert.False((await e.Servicio.CrearAsync(solicitud)).EsExitoso);
    }

    [Fact]
    public async Task Crear_ServicioGrupalValido_EsPermitido()
    {
        var e = CrearEscenario();
        var resultado = await e.Servicio.CrearAsync(
            Copiar(SolicitudValida(e), grupal: true, capacidad: 8));
        Assert.True(resultado.EsExitoso);
        Assert.Equal(8, Assert.Single(e.Repositorio.Servicios).CapacidadMaxima);
    }

    [Theory]
    [InlineData("otra")]
    [InlineData("inactiva")]
    [InlineData("eliminada")]
    public async Task Crear_RechazaSedeNoDisponible(string caso)
    {
        var e = CrearEscenario();
        var sede = caso switch
        {
            "otra" => CrearSede(Guid.NewGuid(), "Otra"),
            "inactiva" => CrearSede(e.Organizacion.Id, "Inactiva", activa: false),
            _ => CrearSede(e.Organizacion.Id, "Eliminada", eliminada: true)
        };
        e.Repositorio.Sedes.Add(sede);
        var resultado = await e.Servicio.CrearAsync(Copiar(
            SolicitudValida(e), sedes: [new() { SedeId = sede.Id }]));
        Assert.False(resultado.EsExitoso);
    }

    [Fact]
    public async Task Crear_RechazaSedeDuplicadaEnSolicitud()
    {
        var e = CrearEscenario();
        var resultado = await e.Servicio.CrearAsync(Copiar(SolicitudValida(e), sedes:
        [new() { SedeId = e.Sede.Id }, new() { SedeId = e.Sede.Id }]));
        Assert.False(resultado.EsExitoso);
    }

    [Fact]
    public async Task Crear_RechazaPrecioEspecialNegativo()
    {
        var e = CrearEscenario();
        var resultado = await e.Servicio.CrearAsync(Copiar(SolicitudValida(e), sedes:
        [new() { SedeId = e.Sede.Id, PrecioEspecial = -1 }]));
        Assert.False(resultado.EsExitoso);
    }

    [Fact]
    public async Task ActualizarSedes_RetiraLogicamenteYRestauraRelacionEliminada()
    {
        var e = CrearEscenario();
        var servicio = CategoriaServicioServiceTests.CrearServicio(e.Organizacion.Id, e.Categoria.Id);
        e.Repositorio.Servicios.Add(servicio);
        var retirada = new ServicioSede { ServicioId = servicio.Id, SedeId = e.Sede.Id };
        var sedeRestaurada = CrearSede(e.Organizacion.Id, "Sur");
        e.Repositorio.Sedes.Add(sedeRestaurada);
        var restaurada = new ServicioSede
        {
            ServicioId = servicio.Id,
            SedeId = sedeRestaurada.Id,
            EstaActivo = false,
            EstaEliminado = true
        };
        e.Repositorio.Relaciones.AddRange([retirada, restaurada]);
        var resultado = await e.Servicio.ActualizarAsync(Actualizar(servicio.Id,
            Copiar(SolicitudValida(e), sedes:
            [new() { SedeId = sedeRestaurada.Id, PrecioEspecial = 35 }])));
        Assert.True(resultado.EsExitoso);
        Assert.True(retirada.EstaEliminado);
        Assert.False(retirada.EstaActivo);
        Assert.False(restaurada.EstaEliminado);
        Assert.True(restaurada.EstaActivo);
        Assert.Equal(35, restaurada.PrecioEspecial);
    }

    [Fact]
    public async Task CambiarEstadoYEliminar_AplicanReglasDeEstado()
    {
        var e = CrearEscenario();
        var servicio = CategoriaServicioServiceTests.CrearServicio(e.Organizacion.Id, e.Categoria.Id);
        e.Repositorio.Servicios.Add(servicio);
        Assert.True((await e.Servicio.CambiarEstadoAsync(servicio.Id)).EsExitoso);
        Assert.False(servicio.EstaActivo);
        Assert.True((await e.Servicio.EliminarAsync(servicio.Id)).EsExitoso);
        Assert.True(servicio.EstaEliminado);
    }

    private static EscenarioServicio CrearEscenario(bool organizacionActiva = true)
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var organizacion = CategoriaServicioServiceTests.CrearOrganizacion(organizacionActiva);
        organizaciones.Organizaciones.Add(organizacion);
        var categorias = new RepositorioCategoriasFalso();
        var categoria = AgregarCategoria(categorias, organizacion.Id, true);
        var repositorio = new RepositorioServiciosFalso();
        var sede = CrearSede(organizacion.Id, "Principal");
        repositorio.Sedes.Add(sede);
        return new(new ServicioService(repositorio, categorias, organizaciones), repositorio,
            categorias, organizacion, categoria, sede);
    }

    private static CategoriaServicio AgregarCategoria(EscenarioServicio e, Guid organizacionId, bool activa) =>
        AgregarCategoria(e.Categorias, organizacionId, activa);

    private static CategoriaServicio AgregarCategoria(RepositorioCategoriasFalso repositorio, Guid organizacionId, bool activa)
    {
        var categoria = CategoriaServicioServiceTests.CrearCategoria(
            organizacionId, Guid.NewGuid().ToString("N"), activa);
        repositorio.Categorias.Add(categoria);
        return categoria;
    }

    private static Sede CrearSede(Guid organizacionId, string nombre, bool activa = true, bool eliminada = false) => new()
    {
        OrganizacionId = organizacionId,
        Nombre = nombre,
        Direccion = "Av. Principal",
        EstaActivo = activa,
        EstaEliminado = eliminada
    };

    private static CrearServicioSolicitud SolicitudValida(EscenarioServicio e) => new()
    {
        OrganizacionId = e.Organizacion.Id,
        CategoriaServicioId = e.Categoria.Id,
        Nombre = "Consulta",
        DuracionMinutos = 30,
        Precio = 50,
        MontoAdelanto = 10,
        Modalidad = ModalidadServicio.Presencial,
        CapacidadMaxima = 1,
        Sedes = [new() { SedeId = e.Sede.Id }]
    };

    private static CrearServicioSolicitud Copiar(
        CrearServicioSolicitud s,
        Guid? categoriaId = null,
        int? duracion = null,
        decimal? precio = null,
        decimal? adelanto = null,
        bool? grupal = null,
        int? capacidad = null,
        int? cancelacion = null,
        int? preparacion = null,
        int? posterior = null,
        IReadOnlyList<SedeAsignacionSolicitud>? sedes = null) => new()
    {
        OrganizacionId = s.OrganizacionId,
        CategoriaServicioId = categoriaId ?? s.CategoriaServicioId,
        Nombre = s.Nombre,
        Descripcion = s.Descripcion,
        DuracionMinutos = duracion ?? s.DuracionMinutos,
        Precio = precio ?? s.Precio,
        MontoAdelanto = adelanto ?? s.MontoAdelanto,
        Modalidad = s.Modalidad,
        EsGrupal = grupal ?? s.EsGrupal,
        CapacidadMaxima = capacidad ?? s.CapacidadMaxima,
        HorasLimiteCancelacion = cancelacion ?? s.HorasLimiteCancelacion,
        TiempoPreparacionMinutos = preparacion ?? s.TiempoPreparacionMinutos,
        TiempoPosteriorMinutos = posterior ?? s.TiempoPosteriorMinutos,
        Sedes = sedes ?? s.Sedes
    };

    private static ActualizarServicioSolicitud Actualizar(Guid id, CrearServicioSolicitud s) => new()
    {
        Id = id,
        CategoriaServicioId = s.CategoriaServicioId,
        Nombre = s.Nombre,
        Descripcion = s.Descripcion,
        DuracionMinutos = s.DuracionMinutos,
        Precio = s.Precio,
        MontoAdelanto = s.MontoAdelanto,
        Modalidad = s.Modalidad,
        EsGrupal = s.EsGrupal,
        CapacidadMaxima = s.CapacidadMaxima,
        HorasLimiteCancelacion = s.HorasLimiteCancelacion,
        TiempoPreparacionMinutos = s.TiempoPreparacionMinutos,
        TiempoPosteriorMinutos = s.TiempoPosteriorMinutos,
        Sedes = s.Sedes
    };

    private sealed record EscenarioServicio(
        ServicioService Servicio,
        RepositorioServiciosFalso Repositorio,
        RepositorioCategoriasFalso Categorias,
        Organizacion Organizacion,
        CategoriaServicio Categoria,
        Sede Sede);
}

internal sealed class RepositorioCategoriasFalso : ICategoriaServicioRepository
{
    public List<CategoriaServicio> Categorias { get; } = [];
    public List<Servicio> Servicios { get; } = [];

    public Task<PaginaResultado<CategoriaServicioListaDto>> ListarAsync(CategoriaServicioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var consulta = Categorias.Where(c => !c.EstaEliminado && c.OrganizacionId == filtro.OrganizacionId);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            consulta = consulta.Where(c => c.Nombre.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase) ||
                (c.Descripcion?.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase) ?? false));
        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(c => c.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(c => !c.EstaActivo),
            _ => consulta
        };
        var lista = consulta.Select(c => new CategoriaServicioListaDto(c.Id, c.OrganizacionId,
            "Organización", c.Nombre, c.Descripcion, Servicios.Count(s => s.CategoriaServicioId == c.Id && !s.EstaEliminado), c.EstaActivo)).ToList();
        return Task.FromResult(new PaginaResultado<CategoriaServicioListaDto>(lista, 1, 10, lista.Count));
    }

    public Task<CategoriaServicioDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = Categorias.SingleOrDefault(c => c.Id == id && !c.EstaEliminado);
        return Task.FromResult(c is null ? null : new CategoriaServicioDetalleDto(c.Id, c.OrganizacionId,
            "Organización", c.Nombre, c.Descripcion, c.EstaActivo, c.FechaCreacion, c.FechaModificacion,
            c.CreadoPorUsuarioId, c.ModificadoPorUsuarioId, Servicios.Count(s => s.CategoriaServicioId == c.Id && !s.EstaEliminado),
            Servicios.Count(s => s.CategoriaServicioId == c.Id && s.EstaActivo && !s.EstaEliminado)));
    }

    public Task<CategoriaServicio?> ObtenerParaModificarAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Categorias.SingleOrDefault(c => c.Id == id));
    public Task<bool> ExisteNombreActivoAsync(Guid organizacionId, string nombre, Guid? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(Categorias.Any(c => c.Id != excluirId && c.OrganizacionId == organizacionId && c.EstaActivo && !c.EstaEliminado && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)));
    public Task<bool> TieneServiciosActivosAsync(Guid categoriaId, CancellationToken cancellationToken = default) => Task.FromResult(Servicios.Any(s => s.CategoriaServicioId == categoriaId && s.EstaActivo && !s.EstaEliminado));
    public Task<bool> TieneServiciosAsync(Guid categoriaId, CancellationToken cancellationToken = default) => Task.FromResult(Servicios.Any(s => s.CategoriaServicioId == categoriaId && !s.EstaEliminado));
    public Task<IReadOnlyList<CategoriaServicioOpcionDto>> ListarActivasAsync(Guid organizacionId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CategoriaServicioOpcionDto>>(Categorias.Where(c => c.OrganizacionId == organizacionId && c.EstaActivo && !c.EstaEliminado).Select(c => new CategoriaServicioOpcionDto(c.Id, c.Nombre)).ToArray());
    public void Agregar(CategoriaServicio categoria) => Categorias.Add(categoria);
    public Task GuardarAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class RepositorioServiciosFalso : IServicioRepository
{
    public List<Servicio> Servicios { get; } = [];
    public List<Sede> Sedes { get; } = [];
    public List<ServicioSede> Relaciones { get; } = [];
    public Task<PaginaResultado<ServicioListaDto>> ListarAsync(ServicioFiltroDto filtro, CancellationToken cancellationToken = default) => Task.FromResult(new PaginaResultado<ServicioListaDto>([], 1, 10, 0));
    public Task<ServicioDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<ServicioDetalleDto?>(null);
    public Task<Servicio?> ObtenerParaModificarAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Servicios.SingleOrDefault(s => s.Id == id));
    public Task<bool> ExisteNombreActivoAsync(Guid organizacionId, string nombre, Guid? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(Servicios.Any(s => s.Id != excluirId && s.OrganizacionId == organizacionId && s.EstaActivo && !s.EstaEliminado && s.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)));
    public Task<IReadOnlyList<SedeAsignacionDto>> ListarSedesParaAsignarAsync(Guid organizacionId, Guid? servicioId = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SedeAsignacionDto>>(Sedes.Where(s => s.OrganizacionId == organizacionId && s.EstaActivo && !s.EstaEliminado).Select(s => new SedeAsignacionDto(s.Id, s.Nombre, s.EstaActivo, Relaciones.Any(r => r.ServicioId == servicioId && r.SedeId == s.Id && r.EstaActivo && !r.EstaEliminado), Relaciones.FirstOrDefault(r => r.ServicioId == servicioId && r.SedeId == s.Id && r.EstaActivo && !r.EstaEliminado)?.PrecioEspecial)).ToArray());
    public Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(IReadOnlyCollection<Guid> sedeIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Sede>>(Sedes.Where(s => sedeIds.Contains(s.Id)).ToArray());
    public Task<IReadOnlyList<ServicioSede>> ObtenerRelacionesSedeAsync(Guid servicioId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ServicioSede>>(Relaciones.Where(r => r.ServicioId == servicioId).ToArray());
    public void Agregar(Servicio servicio) => Servicios.Add(servicio);
    public void AgregarRelacion(ServicioSede servicioSede) => Relaciones.Add(servicioSede);
    public Task GuardarAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken cancellationToken = default) => operacion(cancellationToken);
}
