using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Clientes;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class ClienteServiceTests
{
    [Fact]
    public async Task Crear_Valido_NormalizaYAsignaOrganizacion()
    {
        var e = CrearEscenario();
        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id,
            nombres: "  Ana María  ", apellidos: "  López  "));

        Assert.True(resultado.EsExitoso);
        var cliente = Assert.Single(e.Clientes.Clientes);
        Assert.Equal(e.Organizacion.Id, cliente.OrganizacionId);
        Assert.Equal("Ana María", cliente.Nombres);
        Assert.Equal("López", cliente.Apellidos);
        Assert.Null(cliente.Correo);
        Assert.Null(cliente.Telefono);
        Assert.Null(cliente.FechaNacimiento);
    }

    [Fact]
    public async Task Crear_AceptaCamposOpcionalesValidos()
    {
        var e = CrearEscenario();
        var fecha = new DateOnly(1998, 6, 10);
        var solicitud = Solicitud(e.Organizacion.Id, correo: " ana@test.local ", fecha: fecha);
        solicitud = new CrearClienteSolicitud
        {
            OrganizacionId = solicitud.OrganizacionId,
            TipoDocumento = solicitud.TipoDocumento,
            NumeroDocumento = solicitud.NumeroDocumento,
            Nombres = solicitud.Nombres,
            Apellidos = solicitud.Apellidos,
            Correo = solicitud.Correo,
            Telefono = " 999888777 ",
            Direccion = " Lima ",
            FechaNacimiento = solicitud.FechaNacimiento,
            Observaciones = " Frecuente "
        };

        Assert.True((await e.Servicio.CrearAsync(solicitud)).EsExitoso);
        var cliente = Assert.Single(e.Clientes.Clientes);
        Assert.Equal("ana@test.local", cliente.Correo);
        Assert.Equal("999888777", cliente.Telefono);
        Assert.Equal(fecha, cliente.FechaNacimiento);
    }

    [Theory]
    [InlineData("organizacion_inexistente")]
    [InlineData("organizacion_eliminada")]
    public async Task Crear_RechazaOrganizacionNoDisponible(string caso)
    {
        var e = CrearEscenario();
        if (caso == "organizacion_inexistente") e.Organizaciones.Organizaciones.Clear();
        else e.Organizacion.EstaEliminado = true;

        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id));

        Assert.False(resultado.EsExitoso);
        Assert.Empty(e.Clientes.Clientes);
    }

    [Fact]
    public async Task Crear_RechazaDocumentoDuplicadoInclusoSiEstaEliminado()
    {
        var e = CrearEscenario();
        e.Clientes.Clientes.Add(Cliente(e.Organizacion.Id, eliminado: true));

        var resultado = await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id));

        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorOperacion.Conflicto, resultado.TipoError);
    }

    [Theory]
    [InlineData(true, "otra_organizacion")]
    [InlineData(true, "otro_tipo")]
    public async Task Crear_PermiteDocumentoCuandoLaClaveCompuestaEsDistinta(bool esperado, string caso)
    {
        var e = CrearEscenario();
        e.Clientes.Clientes.Add(caso == "otra_organizacion"
            ? Cliente(Guid.NewGuid())
            : Cliente(e.Organizacion.Id, TipoDocumento.Pasaporte));

        Assert.Equal(esperado, (await e.Servicio.CrearAsync(Solicitud(e.Organizacion.Id))).EsExitoso);
    }

    [Theory]
    [InlineData("documento")]
    [InlineData("nombres")]
    [InlineData("apellidos")]
    [InlineData("tipo")]
    public async Task Crear_RechazaCamposObligatoriosInvalidos(string campo)
    {
        var e = CrearEscenario();
        var solicitud = campo switch
        {
            "documento" => Solicitud(e.Organizacion.Id, documento: "   "),
            "nombres" => Solicitud(e.Organizacion.Id, nombres: "   "),
            "apellidos" => Solicitud(e.Organizacion.Id, apellidos: "   "),
            _ => Solicitud(e.Organizacion.Id, tipo: TipoDocumento.NoDefinido)
        };
        Assert.False((await e.Servicio.CrearAsync(solicitud)).EsExitoso);
    }

    [Theory]
    [InlineData("correo")]
    [InlineData("fecha")]
    [InlineData("documento_largo")]
    [InlineData("nombres_largos")]
    [InlineData("apellidos_largos")]
    public async Task Crear_RechazaFormatosYLongitudesInvalidas(string caso)
    {
        var e = CrearEscenario();
        var solicitud = caso switch
        {
            "correo" => Solicitud(e.Organizacion.Id, correo: "correo-invalido"),
            "fecha" => Solicitud(e.Organizacion.Id, fecha: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            "documento_largo" => Solicitud(e.Organizacion.Id, documento: new string('1', 21)),
            "nombres_largos" => Solicitud(e.Organizacion.Id, nombres: new string('A', 101)),
            _ => Solicitud(e.Organizacion.Id, apellidos: new string('A', 101))
        };
        Assert.False((await e.Servicio.CrearAsync(solicitud)).EsExitoso);
    }

    [Fact]
    public async Task Actualizar_ValidoMantieneOrganizacionYPermiteDocumentoPropio()
    {
        var e = CrearEscenario();
        var cliente = Cliente(e.Organizacion.Id);
        e.Clientes.Clientes.Add(cliente);

        var resultado = await e.Servicio.ActualizarAsync(Actualizar(cliente.Id,
            nombres: "Ana Actualizada"));

        Assert.True(resultado.EsExitoso);
        Assert.Equal(e.Organizacion.Id, cliente.OrganizacionId);
        Assert.Equal("Ana Actualizada", cliente.Nombres);
    }

    [Fact]
    public async Task Actualizar_RechazaDocumentoDeOtroCliente()
    {
        var e = CrearEscenario();
        var cliente = Cliente(e.Organizacion.Id, documento: "11111111");
        e.Clientes.Clientes.AddRange([cliente, Cliente(e.Organizacion.Id)]);

        var resultado = await e.Servicio.ActualizarAsync(Actualizar(cliente.Id));

        Assert.False(resultado.EsExitoso);
    }

    [Fact]
    public async Task CambiarEstado_DesactivaYReactiva()
    {
        var e = CrearEscenario();
        var cliente = Cliente(e.Organizacion.Id);
        e.Clientes.Clientes.Add(cliente);

        Assert.True((await e.Servicio.CambiarEstadoAsync(cliente.Id, false)).EsExitoso);
        Assert.False(cliente.EstaActivo);
        Assert.True((await e.Servicio.CambiarEstadoAsync(cliente.Id, true)).EsExitoso);
        Assert.True(cliente.EstaActivo);
    }

    [Fact]
    public async Task Eliminar_AplicaSoftDeleteYLoExcluyeDelListado()
    {
        var e = CrearEscenario();
        var cliente = Cliente(e.Organizacion.Id);
        e.Clientes.Clientes.Add(cliente);

        Assert.True((await e.Servicio.EliminarAsync(cliente.Id)).EsExitoso);
        var listado = await e.Servicio.ListarAsync(new(e.Organizacion.Id));

        Assert.True(cliente.EstaEliminado);
        Assert.False(cliente.EstaActivo);
        Assert.Empty(listado.Valor!.Elementos);
    }

    [Theory]
    [InlineData("76543210")]
    [InlineData("Ana")]
    [InlineData("López")]
    [InlineData("ana@test.local")]
    [InlineData("999888777")]
    [InlineData("Ana López")]
    public async Task Listar_BuscaEnTodosLosCamposRequeridos(string busqueda)
    {
        var e = CrearEscenario();
        e.Clientes.Clientes.Add(Cliente(e.Organizacion.Id,
            correo: "ana@test.local", telefono: "999888777"));

        var resultado = await e.Servicio.ListarAsync(new(e.Organizacion.Id, busqueda));

        Assert.Single(resultado.Valor!.Elementos);
    }

    [Theory]
    [InlineData(TipoDocumento.DNI, EstadoFiltro.Activos, 1)]
    [InlineData(TipoDocumento.Pasaporte, EstadoFiltro.Activos, 0)]
    [InlineData(TipoDocumento.DNI, EstadoFiltro.Inactivos, 0)]
    public async Task Listar_FiltraTipoYEstado(
        TipoDocumento tipo, EstadoFiltro estado, int cantidad)
    {
        var e = CrearEscenario();
        e.Clientes.Clientes.Add(Cliente(e.Organizacion.Id));
        var resultado = await e.Servicio.ListarAsync(new(
            e.Organizacion.Id, TipoDocumento: tipo, Estado: estado));
        Assert.Equal(cantidad, resultado.Valor!.Elementos.Count);
    }

    [Fact]
    public async Task Listar_PaginaYLimitaTamanoMaximo()
    {
        var e = CrearEscenario();
        for (var i = 0; i < 105; i++)
            e.Clientes.Clientes.Add(Cliente(e.Organizacion.Id, documento: $"{i:00000000}"));

        var resultado = await e.Servicio.ListarAsync(new(
            e.Organizacion.Id, Pagina: 2, TamanoPagina: 200));

        Assert.Equal(100, resultado.Valor!.TamanoPagina);
        Assert.Equal(5, resultado.Valor.Elementos.Count);
        Assert.Equal(105, resultado.Valor.TotalElementos);
    }

    private static Escenario CrearEscenario()
    {
        var organizaciones = new RepositorioOrganizacionesFalso();
        var organizacion = CategoriaServicioServiceTests.CrearOrganizacion();
        organizaciones.Organizaciones.Add(organizacion);
        var clientes = new RepositorioClientesFalso();
        return new(new ClienteService(clientes, organizaciones), clientes, organizaciones, organizacion);
    }

    private static CrearClienteSolicitud Solicitud(
        Guid organizacionId,
        string documento = "76543210",
        string nombres = "Ana",
        string apellidos = "López",
        TipoDocumento tipo = TipoDocumento.DNI,
        string? correo = null,
        DateOnly? fecha = null) => new()
    {
        OrganizacionId = organizacionId,
        TipoDocumento = tipo,
        NumeroDocumento = documento,
        Nombres = nombres,
        Apellidos = apellidos,
        Correo = correo,
        FechaNacimiento = fecha
    };

    private static ActualizarClienteSolicitud Actualizar(Guid id, string nombres = "Ana") => new()
    {
        Id = id,
        TipoDocumento = TipoDocumento.DNI,
        NumeroDocumento = "76543210",
        Nombres = nombres,
        Apellidos = "López"
    };

    private static Cliente Cliente(
        Guid organizacionId,
        TipoDocumento tipo = TipoDocumento.DNI,
        string documento = "76543210",
        bool eliminado = false,
        string? correo = null,
        string? telefono = null) => new()
    {
        OrganizacionId = organizacionId,
        TipoDocumento = tipo,
        NumeroDocumento = documento,
        Nombres = "Ana",
        Apellidos = "López",
        Correo = correo,
        Telefono = telefono,
        EstaEliminado = eliminado
    };

    private sealed record Escenario(
        ClienteService Servicio,
        RepositorioClientesFalso Clientes,
        RepositorioOrganizacionesFalso Organizaciones,
        Organizacion Organizacion);
}

internal sealed class RepositorioClientesFalso : IClienteRepository
{
    public List<Cliente> Clientes { get; } = [];

    public Task<PaginaResultado<ClienteListaDto>> ListarAsync(
        ClienteFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var consulta = Clientes.Where(c =>
            !c.EstaEliminado && c.OrganizacionId == filtro.OrganizacionId);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.Trim();
            consulta = consulta.Where(c => new[]
            {
                c.NumeroDocumento, c.Nombres, c.Apellidos, $"{c.Nombres} {c.Apellidos}",
                c.Correo ?? string.Empty, c.Telefono ?? string.Empty
            }.Any(v => v.Contains(b, StringComparison.OrdinalIgnoreCase)));
        }
        if (filtro.TipoDocumento.HasValue)
            consulta = consulta.Where(c => c.TipoDocumento == filtro.TipoDocumento);
        consulta = filtro.Estado switch
        {
            EstadoFiltro.Activos => consulta.Where(c => c.EstaActivo),
            EstadoFiltro.Inactivos => consulta.Where(c => !c.EstaActivo),
            _ => consulta
        };
        var ordenada = consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToArray();
        var items = ordenada.Skip((pagina - 1) * tamano).Take(tamano).Select(Mapear).ToArray();
        return Task.FromResult(new PaginaResultado<ClienteListaDto>(
            items, pagina, tamano, ordenada.Length));
    }

    public Task<ClienteDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = Clientes.SingleOrDefault(c => c.Id == id && !c.EstaEliminado);
        return Task.FromResult(c is null ? null : new ClienteDetalleDto(
            c.Id, c.OrganizacionId, c.TipoDocumento, c.NumeroDocumento, c.Nombres,
            c.Apellidos, $"{c.Nombres} {c.Apellidos}", c.Correo, c.Telefono,
            c.Direccion, c.FechaNacimiento, c.Observaciones, c.EstaActivo,
            c.FechaCreacion, c.FechaModificacion, c.CreadoPorUsuarioId, c.ModificadoPorUsuarioId));
    }

    public Task<Cliente?> ObtenerParaModificarAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Clientes.SingleOrDefault(c => c.Id == id));

    public Task<bool> ExisteDocumentoAsync(
        Guid organizacionId, TipoDocumento tipoDocumento, string numeroDocumento,
        Guid? excluirId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Clientes.Any(c => c.Id != excluirId &&
            c.OrganizacionId == organizacionId && c.TipoDocumento == tipoDocumento &&
            c.NumeroDocumento.Equals(numeroDocumento, StringComparison.OrdinalIgnoreCase)));

    public void Agregar(Cliente cliente) => Clientes.Add(cliente);
    public Task GuardarAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private static ClienteListaDto Mapear(Cliente c) => new(
        c.Id, c.OrganizacionId, c.TipoDocumento, c.NumeroDocumento, c.Nombres,
        c.Apellidos, $"{c.Nombres} {c.Apellidos}", c.Correo, c.Telefono, c.EstaActivo);
}
