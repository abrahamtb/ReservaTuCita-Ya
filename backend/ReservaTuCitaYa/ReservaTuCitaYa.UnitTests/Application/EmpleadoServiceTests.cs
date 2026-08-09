using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Empleados;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class EmpleadoServiceTests
{
    [Fact]
    public async Task Crear_ValidoNormalizaYAsignaOrganizacion()
    {
        var e = Escenario();
        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id,
            nombres: " Carlos ", apellidos: " Ramirez ", cargo: " Barbero "));
        Assert.True(resultado.EsExitoso);
        var empleado = Assert.Single(e.Repositorio.Empleados);
        Assert.Equal(e.Organizacion.Id, empleado.OrganizacionId);
        Assert.Equal("Carlos", empleado.Nombres);
        Assert.Equal("Ramirez", empleado.Apellidos);
        Assert.Equal("Barbero", empleado.Cargo);
    }

    [Theory]
    [InlineData("inexistente")]
    [InlineData("eliminada")]
    public async Task Crear_RechazaOrganizacionNoDisponible(string caso)
    {
        var e = Escenario();
        if (caso == "inexistente") e.Organizaciones.Organizaciones.Clear();
        else e.Organizacion.EstaEliminado = true;
        Assert.False((await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id))).EsExitoso);
        Assert.Empty(e.Repositorio.Empleados);
    }

    [Fact]
    public async Task Crear_DocumentoEliminadoSigueReservado()
    {
        var e = Escenario();
        e.Repositorio.Empleados.Add(Empleado(e.Organizacion.Id, eliminado: true));
        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id));
        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Theory]
    [InlineData("otra_organizacion")]
    [InlineData("otro_tipo")]
    public async Task Crear_PermiteClaveDocumentoCompuestaDistinta(string caso)
    {
        var e = Escenario();
        e.Repositorio.Empleados.Add(caso == "otra_organizacion"
            ? Empleado(Guid.NewGuid()) : Empleado(e.Organizacion.Id, TipoDocumento.Pasaporte));
        Assert.True((await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id))).EsExitoso);
    }

    [Theory]
    [InlineData("tipo")]
    [InlineData("documento")]
    [InlineData("nombres")]
    [InlineData("apellidos")]
    [InlineData("cargo")]
    public async Task Crear_RechazaObligatoriosInvalidos(string campo)
    {
        var e = Escenario();
        var s = campo switch
        {
            "tipo" => Solicitud(e.Organizacion.Id, tipo: TipoDocumento.NoDefinido),
            "documento" => Solicitud(e.Organizacion.Id, documento: " "),
            "nombres" => Solicitud(e.Organizacion.Id, nombres: " "),
            "apellidos" => Solicitud(e.Organizacion.Id, apellidos: " "),
            _ => Solicitud(e.Organizacion.Id, cargo: " ")
        };
        Assert.False((await e.Servicio.CrearAsync(s)).EsExitoso);
    }

    [Fact]
    public async Task Crear_AceptaOpcionalesNulos()
    {
        var e = Escenario();
        Assert.True((await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id))).EsExitoso);
        var empleado = Assert.Single(e.Repositorio.Empleados);
        Assert.Null(empleado.Correo);
        Assert.Null(empleado.Telefono);
        Assert.Null(empleado.FechaNacimiento);
    }

    [Theory]
    [InlineData("correo")]
    [InlineData("fecha")]
    public async Task Crear_RechazaFormatoInvalido(string caso)
    {
        var e = Escenario();
        var s = caso == "correo"
            ? Solicitud(e.Organizacion.Id, correo: "invalido")
            : Solicitud(e.Organizacion.Id, fecha: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        Assert.False((await e.Servicio.CrearAsync(s)).EsExitoso);
    }

    [Theory]
    [InlineData("documento")]
    [InlineData("nombres")]
    [InlineData("apellidos")]
    [InlineData("cargo")]
    [InlineData("especialidad")]
    [InlineData("colegiatura")]
    public async Task Crear_RechazaLongitudesExcedidas(string campo)
    {
        var e = Escenario();
        var s = campo switch
        {
            "documento" => Solicitud(e.Organizacion.Id, documento: new string('1', 21)),
            "nombres" => Solicitud(e.Organizacion.Id, nombres: new string('A', 101)),
            "apellidos" => Solicitud(e.Organizacion.Id, apellidos: new string('A', 101)),
            "cargo" => Solicitud(e.Organizacion.Id, cargo: new string('A', 101)),
            "especialidad" => Solicitud(e.Organizacion.Id, especialidad: new string('A', 151)),
            _ => Solicitud(e.Organizacion.Id, colegiatura: new string('A', 51))
        };
        Assert.False((await e.Servicio.CrearAsync(s)).EsExitoso);
    }

    [Theory]
    [InlineData(false, null, null)]
    [InlineData(true, "Corte y barba", null)]
    [InlineData(true, "Medicina", "CMP-123")]
    public async Task Crear_AceptaVariantesProfesionales(
        bool profesional, string? especialidad, string? colegiatura)
    {
        var e = Escenario();
        var resultado = await e.Servicio.CrearAsync(Solicitud(
            e.Organizacion.Id, profesional: profesional,
            especialidad: especialidad, colegiatura: colegiatura));
        Assert.True(resultado.EsExitoso);
        Assert.Equal(profesional, Assert.Single(e.Repositorio.Empleados).EsProfesional);
    }

    [Fact]
    public async Task Actualizar_MantieneOrganizacionYDocumentoPropio()
    {
        var e = Escenario();
        var empleado = Empleado(e.Organizacion.Id);
        e.Repositorio.Empleados.Add(empleado);
        var resultado = await e.Servicio.ActualizarAsync(Actualizar(empleado.Id, "Carlos Alberto"));
        Assert.True(resultado.EsExitoso);
        Assert.Equal(e.Organizacion.Id, empleado.OrganizacionId);
        Assert.Equal("Carlos Alberto", empleado.Nombres);
    }

    [Fact]
    public async Task Actualizar_RevalidaDocumentoDuplicado()
    {
        var e = Escenario();
        var empleado = Empleado(e.Organizacion.Id, documento: "11111111");
        e.Repositorio.Empleados.AddRange([empleado, Empleado(e.Organizacion.Id)]);
        Assert.False((await e.Servicio.ActualizarAsync(Actualizar(empleado.Id))).EsExitoso);
    }

    [Fact]
    public async Task CambiarEstado_DesactivaYActiva()
    {
        var e = Escenario();
        var empleado = Empleado(e.Organizacion.Id);
        e.Repositorio.Empleados.Add(empleado);
        Assert.True((await e.Servicio.CambiarEstadoAsync(empleado.Id, false)).EsExitoso);
        Assert.False(empleado.EstaActivo);
        Assert.True((await e.Servicio.CambiarEstadoAsync(empleado.Id, true)).EsExitoso);
        Assert.True(empleado.EstaActivo);
    }

    [Fact]
    public async Task Eliminar_AplicaSoftDeleteYLoExcluye()
    {
        var e = Escenario();
        var empleado = Empleado(e.Organizacion.Id);
        e.Repositorio.Empleados.Add(empleado);
        Assert.True((await e.Servicio.EliminarAsync(empleado.Id)).EsExitoso);
        var lista = await e.Servicio.ListarAsync(new(e.Organizacion.Id));
        Assert.True(empleado.EstaEliminado);
        Assert.False(empleado.EstaActivo);
        Assert.Empty(lista.Valor!.Elementos);
    }

    [Theory]
    [InlineData("71234567")]
    [InlineData("Carlos")]
    [InlineData("Ramirez")]
    [InlineData("Barbero")]
    [InlineData("Corte")]
    public async Task Listar_BuscaCamposRequeridos(string busqueda)
    {
        var e = Escenario();
        e.Repositorio.Empleados.Add(Empleado(e.Organizacion.Id,
            especialidad: "Corte y barba"));
        Assert.Single((await e.Servicio.ListarAsync(new(
            e.Organizacion.Id, busqueda))).Valor!.Elementos);
    }

    [Theory]
    [InlineData(true, EstadoFiltro.Todos, 1)]
    [InlineData(false, EstadoFiltro.Todos, 0)]
    [InlineData(null, EstadoFiltro.Activos, 1)]
    [InlineData(null, EstadoFiltro.Inactivos, 0)]
    public async Task Listar_FiltraProfesionalYEstado(bool? profesional, EstadoFiltro estado, int cantidad)
    {
        var e = Escenario();
        e.Repositorio.Empleados.Add(Empleado(e.Organizacion.Id, profesional: true));
        var lista = await e.Servicio.ListarAsync(new(
            e.Organizacion.Id, EsProfesional: profesional, Estado: estado));
        Assert.Equal(cantidad, lista.Valor!.Elementos.Count);
    }

    [Fact]
    public async Task Listar_PaginaYLimitaTamano()
    {
        var e = Escenario();
        for (var i = 0; i < 105; i++)
            e.Repositorio.Empleados.Add(Empleado(e.Organizacion.Id, documento: $"{i:00000000}"));
        var lista = await e.Servicio.ListarAsync(new(
            e.Organizacion.Id, Pagina: 2, TamanoPagina: 200));
        Assert.Equal(100, lista.Valor!.TamanoPagina);
        Assert.Equal(5, lista.Valor.Elementos.Count);
    }

    [Fact]
    public async Task Crear_ProfesionalConSedeYServicioEsAtomico()
    {
        var e = Escenario();
        var sede = AgregarSede(e);
        var servicio = AgregarServicio(e);
        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id,
            profesional: true, sedeIds: [sede.Id], servicioIds: [servicio.Id]));
        Assert.True(resultado.EsExitoso);
        Assert.Single(e.Repositorio.RelacionesSede);
        Assert.Single(e.Repositorio.RelacionesServicio);
    }

    [Fact]
    public async Task Crear_NoProfesionalNoRecibeServicios()
    {
        var e = Escenario();
        var servicio = AgregarServicio(e);
        var resultado = await e.Servicio.CrearAsync(Solicitud(
            e.Organizacion.Id, servicioIds: [servicio.Id]));
        Assert.False(resultado.EsExitoso);
        Assert.Empty(e.Repositorio.Empleados);
    }

    [Fact]
    public async Task ReemplazarSedes_QuitaMantieneAgregaYRestaura()
    {
        var e = Escenario();
        var empleado = Empleado(e.Organizacion.Id);
        var a = AgregarSede(e, "A"); var b = AgregarSede(e, "B"); var c = AgregarSede(e, "C");
        e.Repositorio.Empleados.Add(empleado);
        e.Repositorio.RelacionesSede.AddRange([
            new() { EmpleadoId = empleado.Id, SedeId = a.Id },
            new() { EmpleadoId = empleado.Id, SedeId = b.Id },
            new() { EmpleadoId = empleado.Id, SedeId = c.Id, EstaActivo = false, EstaEliminado = true }]);
        Assert.True((await e.Servicio.ReemplazarSedesAsync(empleado.Id, [a.Id, c.Id])).EsExitoso);
        Assert.True(e.Repositorio.RelacionesSede.Single(r => r.SedeId == b.Id).EstaEliminado);
        Assert.False(e.Repositorio.RelacionesSede.Single(r => r.SedeId == c.Id).EstaEliminado);
    }

    [Theory]
    [InlineData("otra_organizacion")]
    [InlineData("duplicada")]
    public async Task ReemplazarSedes_RechazaAsignacionInvalida(string caso)
    {
        var e = Escenario(); var empleado = Empleado(e.Organizacion.Id); e.Repositorio.Empleados.Add(empleado);
        var sede = AgregarSede(e, organizacionId: caso == "otra_organizacion" ? Guid.NewGuid() : null);
        var ids = caso == "duplicada" ? new[] { sede.Id, sede.Id } : new[] { sede.Id };
        Assert.False((await e.Servicio.ReemplazarSedesAsync(empleado.Id, ids)).EsExitoso);
    }

    [Fact]
    public async Task ReemplazarServicios_QuitaMantieneAgregaYRestaura()
    {
        var e = Escenario(); var empleado = Empleado(e.Organizacion.Id, profesional: true);
        var a = AgregarServicio(e, "A"); var b = AgregarServicio(e, "B"); var c = AgregarServicio(e, "C");
        e.Repositorio.Empleados.Add(empleado);
        e.Repositorio.RelacionesServicio.AddRange([
            new() { EmpleadoId = empleado.Id, ServicioId = a.Id },
            new() { EmpleadoId = empleado.Id, ServicioId = b.Id },
            new() { EmpleadoId = empleado.Id, ServicioId = c.Id, EstaActivo = false, EstaEliminado = true }]);
        Assert.True((await e.Servicio.ReemplazarServiciosAsync(empleado.Id, [a.Id, c.Id])).EsExitoso);
        Assert.True(e.Repositorio.RelacionesServicio.Single(r => r.ServicioId == b.Id).EstaEliminado);
        Assert.False(e.Repositorio.RelacionesServicio.Single(r => r.ServicioId == c.Id).EstaEliminado);
    }

    [Theory]
    [InlineData("no_profesional")]
    [InlineData("otra_organizacion")]
    [InlineData("duplicado")]
    public async Task ReemplazarServicios_RechazaAsignacionInvalida(string caso)
    {
        var e = Escenario(); var empleado = Empleado(e.Organizacion.Id, profesional: caso != "no_profesional");
        e.Repositorio.Empleados.Add(empleado);
        var servicio = AgregarServicio(e, organizacionId: caso == "otra_organizacion" ? Guid.NewGuid() : null);
        var ids = caso == "duplicado" ? new[] { servicio.Id, servicio.Id } : new[] { servicio.Id };
        Assert.False((await e.Servicio.ReemplazarServiciosAsync(empleado.Id, ids)).EsExitoso);
    }

    [Fact]
    public async Task Actualizar_NoQuitaProfesionalConServicios()
    {
        var e = Escenario(); var empleado = Empleado(e.Organizacion.Id, profesional: true);
        e.Repositorio.Empleados.Add(empleado);
        e.Repositorio.RelacionesServicio.Add(new() { EmpleadoId = empleado.Id, ServicioId = Guid.NewGuid() });
        var resultado = await e.Servicio.ActualizarAsync(Actualizar(empleado.Id, profesional: false));
        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Fact]
    public async Task ListarProfesionales_DevuelveSoloMarcados()
    {
        var e = Escenario();
        e.Repositorio.Empleados.AddRange([
            Empleado(e.Organizacion.Id, profesional: true),
            Empleado(e.Organizacion.Id, documento: "99999999")]);
        var lista = await e.Servicio.ListarAsync(new(e.Organizacion.Id, EsProfesional: true));
        Assert.Single(lista.Valor!.Elementos);
        Assert.True(lista.Valor.Elementos[0].EsProfesional);
    }

    private static EscenarioPrueba Escenario()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var organizacion = CategoriaServicioServiceTests.CrearOrganizacion();
        organizaciones.Organizaciones.Add(organizacion);
        var repo = new RepositorioEmpleadosFalso();
        return new(new EmpleadoService(repo, organizaciones), repo, organizaciones, organizacion);
    }

    private static CrearEmpleadoSolicitud Solicitud(
        Guid organizacionId, string documento = "71234567", string nombres = "Carlos",
        string apellidos = "Ramirez", string cargo = "Barbero",
        TipoDocumento tipo = TipoDocumento.DNI, string? correo = null, DateOnly? fecha = null,
        bool profesional = false, string? especialidad = null, string? colegiatura = null,
        IReadOnlyList<Guid>? sedeIds = null, IReadOnlyList<Guid>? servicioIds = null) => new()
    {
        OrganizacionId = organizacionId, TipoDocumento = tipo, NumeroDocumento = documento,
        Nombres = nombres, Apellidos = apellidos, Cargo = cargo, Correo = correo,
        FechaNacimiento = fecha, EsProfesional = profesional, Especialidad = especialidad,
        NumeroColegiatura = colegiatura, SedeIds = sedeIds ?? [], ServicioIds = servicioIds ?? []
    };

    private static ActualizarEmpleadoSolicitud Actualizar(
        Guid id, string nombres = "Carlos", bool profesional = false) => new()
    {
        Id = id, TipoDocumento = TipoDocumento.DNI, NumeroDocumento = "71234567",
        Nombres = nombres, Apellidos = "Ramirez", Cargo = "Barbero", EsProfesional = profesional
    };

    private static Empleado Empleado(
        Guid organizacionId, TipoDocumento tipo = TipoDocumento.DNI,
        string documento = "71234567", bool profesional = false,
        bool eliminado = false, string? especialidad = null) => new()
    {
        OrganizacionId = organizacionId, TipoDocumento = tipo, NumeroDocumento = documento,
        Nombres = "Carlos", Apellidos = "Ramirez", Cargo = "Barbero",
        Especialidad = especialidad, EsProfesional = profesional, EstaEliminado = eliminado
    };

    private static Sede AgregarSede(
        EscenarioPrueba e, string nombre = "Principal", Guid? organizacionId = null)
    {
        var sede = new Sede { OrganizacionId = organizacionId ?? e.Organizacion.Id, Nombre = nombre, Direccion = "Lima" };
        e.Repositorio.Sedes.Add(sede); return sede;
    }

    private static Servicio AgregarServicio(
        EscenarioPrueba e, string nombre = "Consulta", Guid? organizacionId = null)
    {
        var servicio = new Servicio
        {
            OrganizacionId = organizacionId ?? e.Organizacion.Id, Nombre = nombre,
            DuracionMinutos = 30, Precio = 50, CapacidadMaxima = 1
        };
        e.Repositorio.Servicios.Add(servicio); return servicio;
    }

    private sealed record EscenarioPrueba(
        EmpleadoService Servicio, RepositorioEmpleadosFalso Repositorio,
        RepositorioOrganizacionesFalso Organizaciones, Organizacion Organizacion);
}

internal sealed class RepositorioEmpleadosFalso : IEmpleadoRepository
{
    public List<Empleado> Empleados { get; } = [];
    public List<Sede> Sedes { get; } = [];
    public List<Servicio> Servicios { get; } = [];
    public List<EmpleadoSede> RelacionesSede { get; } = [];
    public List<ProfesionalServicio> RelacionesServicio { get; } = [];

    public Task<PaginaResultado<EmpleadoListaDto>> ListarAsync(EmpleadoFiltroDto f, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, f.Pagina); var tamano = Math.Clamp(f.TamanoPagina, 1, 100);
        var q = Empleados.Where(e => !e.EstaEliminado && e.OrganizacionId == f.OrganizacionId);
        if (!string.IsNullOrWhiteSpace(f.Busqueda)) { var b = f.Busqueda; q = q.Where(e => new[] { e.NumeroDocumento, e.Nombres, e.Apellidos, e.Cargo, e.Especialidad ?? "", e.Correo ?? "", e.Telefono ?? "" }.Any(v => v.Contains(b, StringComparison.OrdinalIgnoreCase))); }
        if (f.TipoDocumento.HasValue) q = q.Where(e => e.TipoDocumento == f.TipoDocumento);
        if (f.EsProfesional.HasValue) q = q.Where(e => e.EsProfesional == f.EsProfesional);
        if (f.SedeId.HasValue) q = q.Where(e => RelacionesSede.Any(r => r.EmpleadoId == e.Id && r.SedeId == f.SedeId && r.EstaActivo && !r.EstaEliminado));
        if (f.ServicioId.HasValue) q = q.Where(e => RelacionesServicio.Any(r => r.EmpleadoId == e.Id && r.ServicioId == f.ServicioId && r.EstaActivo && !r.EstaEliminado));
        q = f.Estado switch { EstadoFiltro.Activos => q.Where(e => e.EstaActivo), EstadoFiltro.Inactivos => q.Where(e => !e.EstaActivo), _ => q };
        var all = q.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres).ToArray();
        var items = all.Skip((pagina - 1) * tamano).Take(tamano).Select(Mapear).ToArray();
        return Task.FromResult(new PaginaResultado<EmpleadoListaDto>(items, pagina, tamano, all.Length));
    }

    public Task<EmpleadoDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken ct = default)
    {
        var e = Empleados.SingleOrDefault(x => x.Id == id && !x.EstaEliminado);
        return Task.FromResult(e is null ? null : new EmpleadoDetalleDto(e.Id, e.OrganizacionId,
            e.TipoDocumento, e.NumeroDocumento, e.Nombres, e.Apellidos, $"{e.Nombres} {e.Apellidos}",
            e.Correo, e.Telefono, e.Direccion, e.FechaNacimiento, e.Cargo, e.Especialidad,
            e.EsProfesional, e.NumeroColegiatura, e.Observaciones, e.EstaActivo, e.FechaCreacion,
            e.FechaModificacion, e.CreadoPorUsuarioId, e.ModificadoPorUsuarioId, [], []));
    }
    public Task<Empleado?> ObtenerParaModificarAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Empleados.SingleOrDefault(e => e.Id == id));
    public Task<bool> ExisteDocumentoAsync(Guid oid, TipoDocumento tipo, string numero, Guid? excluir = null, CancellationToken ct = default) => Task.FromResult(Empleados.Any(e => e.Id != excluir && e.OrganizacionId == oid && e.TipoDocumento == tipo && e.NumeroDocumento.Equals(numero, StringComparison.OrdinalIgnoreCase)));
    public Task<IReadOnlyList<Sede>> ObtenerSedesParaValidarAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Sede>>(Sedes.Where(s => ids.Contains(s.Id)).ToArray());
    public Task<IReadOnlyList<Servicio>> ObtenerServiciosParaValidarAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Servicio>>(Servicios.Where(s => ids.Contains(s.Id)).ToArray());
    public Task<IReadOnlyList<EmpleadoSede>> ObtenerRelacionesSedeAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<EmpleadoSede>>(RelacionesSede.Where(r => r.EmpleadoId == id).ToArray());
    public Task<IReadOnlyList<ProfesionalServicio>> ObtenerRelacionesServicioAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ProfesionalServicio>>(RelacionesServicio.Where(r => r.EmpleadoId == id).ToArray());
    public Task<IReadOnlyList<EmpleadoSedeDto>> ListarSedesAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<EmpleadoSedeDto>>(RelacionesSede.Where(r => r.EmpleadoId == id && r.EstaActivo && !r.EstaEliminado).Select(r => new EmpleadoSedeDto(r.Id, r.SedeId, Sedes.Single(s => s.Id == r.SedeId).Nombre, true)).ToArray());
    public Task<IReadOnlyList<ProfesionalServicioDto>> ListarServiciosAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ProfesionalServicioDto>>(RelacionesServicio.Where(r => r.EmpleadoId == id && r.EstaActivo && !r.EstaEliminado).Select(r => new ProfesionalServicioDto(r.Id, r.ServicioId, Servicios.Single(s => s.Id == r.ServicioId).Nombre, true)).ToArray());
    public Task<bool> TieneServiciosActivosAsync(Guid id, CancellationToken ct = default) => Task.FromResult(RelacionesServicio.Any(r => r.EmpleadoId == id && r.EstaActivo && !r.EstaEliminado));
    public void Agregar(Empleado e) => Empleados.Add(e);
    public void AgregarRelacion(EmpleadoSede r) => RelacionesSede.Add(r);
    public void AgregarRelacion(ProfesionalServicio r) => RelacionesServicio.Add(r);
    public Task GuardarAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> op, CancellationToken ct = default) => op(ct);
    private EmpleadoListaDto Mapear(Empleado e) => new(e.Id, e.OrganizacionId, e.TipoDocumento,
        e.NumeroDocumento, e.Nombres, e.Apellidos, $"{e.Nombres} {e.Apellidos}", e.Correo,
        e.Telefono, e.Cargo, e.Especialidad, e.EsProfesional,
        RelacionesSede.Count(r => r.EmpleadoId == e.Id && r.EstaActivo && !r.EstaEliminado),
        RelacionesServicio.Count(r => r.EmpleadoId == e.Id && r.EstaActivo && !r.EstaEliminado), e.EstaActivo);
}
