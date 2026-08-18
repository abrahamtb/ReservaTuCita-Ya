using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Application.Services;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.UnitTests.Application;

public sealed class CalificacionServiceTests
{
    [Fact]
    public async Task Crear_ReservaAtendida_CreaCalificacionConAtencionReal()
    {
        var repo = new FakeRepo();
        var reserva = CrearReservaAtendida();
        repo.Reserva = reserva;
        var service = new CalificacionService(repo);

        var result = await service.CrearCalificacionAsync(
            reserva.Id,
            new CrearCalificacionRequest { Puntuacion = 5, Comentario = " Excelente " });

        Assert.NotNull(repo.Creada);
        Assert.Equal(reserva.Id, repo.Creada!.ReservaId);
        Assert.Equal(reserva.Atencion!.Id, repo.Creada.AtencionId);
        Assert.Equal(5, result.Puntuacion);
        Assert.Equal("Excelente", result.Comentario);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Crear_PuntuacionFueraDeRango_Rechaza(int puntuacion)
    {
        var service = new CalificacionService(new FakeRepo { Reserva = CrearReservaAtendida() });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CrearCalificacionAsync(Guid.NewGuid(), new CrearCalificacionRequest { Puntuacion = puntuacion }));
    }

    [Fact]
    public async Task Crear_ReservaNoAtendida_Rechaza()
    {
        var reserva = CrearReservaAtendida();
        reserva.EstadoReserva = EstadoReserva.Confirmada;
        var service = new CalificacionService(new FakeRepo { Reserva = reserva });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearCalificacionAsync(reserva.Id, new CrearCalificacionRequest { Puntuacion = 4 }));
    }

    [Fact]
    public async Task Crear_DuplicadaPorReserva_Rechaza()
    {
        var reserva = CrearReservaAtendida();
        var service = new CalificacionService(new FakeRepo { Reserva = reserva, ExisteReserva = true });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearCalificacionAsync(reserva.Id, new CrearCalificacionRequest { Puntuacion = 4 }));
    }

    [Fact]
    public async Task Resumen_CalculaPromedioYDistribucion()
    {
        var profesionalId = Guid.NewGuid();
        var repo = new FakeRepo
        {
            Profesional = new Empleado
            {
                Id = profesionalId,
                Nombres = "Ana",
                Apellidos = "Pérez",
                EsProfesional = true
            },
            Conteos = new Dictionary<int, int> { [4] = 1, [5] = 2 }
        };
        var service = new CalificacionService(repo);

        var result = await service.ObtenerResumenProfesionalAsync(profesionalId);

        Assert.Equal("Ana Pérez", result.ProfesionalNombre);
        Assert.Equal(3, result.TotalCalificaciones);
        Assert.Equal(4.67, result.Promedio);
        Assert.Equal(5, result.Distribucion.Count);
    }

    private static Reserva CrearReservaAtendida()
    {
        var reserva = new Reserva
        {
            Id = Guid.NewGuid(),
            ProfesionalId = Guid.NewGuid(),
            EstadoReserva = EstadoReserva.Atendida
        };
        reserva.Atencion = new Atencion
        {
            Id = Guid.NewGuid(),
            ReservaId = reserva.Id,
            Reserva = reserva
        };
        return reserva;
    }

    private sealed class FakeRepo : ICalificacionRepository
    {
        public Reserva? Reserva { get; set; }
        public Empleado? Profesional { get; set; }
        public bool ExisteReserva { get; set; }
        public bool ExisteAtencion { get; set; }
        public Calificacion? Creada { get; private set; }
        public Dictionary<int, int> Conteos { get; set; } = new();

        public Task<Calificacion?> ObtenerPorReservaAsync(Guid reservaId) => Task.FromResult(Creada);
        public Task<Reserva?> ObtenerReservaParaCalificarAsync(Guid reservaId) => Task.FromResult(Reserva);
        public Task<Empleado?> ObtenerProfesionalAsync(Guid profesionalId) => Task.FromResult(Profesional);
        public Task<IReadOnlyCollection<Calificacion>> ListarPorProfesionalAsync(Guid profesionalId, int pagina, int tamanoPagina, int? puntuacion) =>
            Task.FromResult<IReadOnlyCollection<Calificacion>>(Array.Empty<Calificacion>());
        public Task<int> ContarPorProfesionalAsync(Guid profesionalId, int? puntuacion = null) =>
            Task.FromResult(puntuacion.HasValue && Conteos.TryGetValue(puntuacion.Value, out var valor) ? valor : 0);
        public Task CrearAsync(Calificacion calificacion)
        {
            Creada = calificacion;
            return Task.CompletedTask;
        }
        public Task<bool> ExistePorReservaAsync(Guid reservaId) => Task.FromResult(ExisteReserva);
        public Task<bool> ExistePorAtencionAsync(Guid atencionId) => Task.FromResult(ExisteAtencion);
        public Task GuardarCambiosAsync() => Task.CompletedTask;
    }
}
