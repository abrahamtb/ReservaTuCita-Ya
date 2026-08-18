using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Application.Services;

public sealed class CalificacionService : ICalificacionService
{
    private readonly ICalificacionRepository _repository;

    public CalificacionService(ICalificacionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CalificacionDto> CrearCalificacionAsync(Guid reservaId, CrearCalificacionRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.Puntuacion is < 1 or > 5)
            throw new ArgumentException("La puntuación debe estar entre 1 y 5.", nameof(request));

        var reserva = await _repository.ObtenerReservaParaCalificarAsync(reservaId)
            ?? throw new InvalidOperationException("La reserva indicada no existe.");

        if (reserva.EstadoReserva != EstadoReserva.Atendida)
            throw new InvalidOperationException("Solo se puede calificar una reserva atendida.");

        if (reserva.Atencion is null)
            throw new InvalidOperationException("La reserva atendida no tiene una atención asociada.");

        if (!reserva.ProfesionalId.HasValue)
            throw new InvalidOperationException("La reserva no tiene un profesional asociado.");

        if (await _repository.ExistePorReservaAsync(reservaId))
            throw new InvalidOperationException("La reserva ya fue calificada.");

        if (await _repository.ExistePorAtencionAsync(reserva.Atencion.Id))
            throw new InvalidOperationException("La atención ya fue calificada.");

        var calificacion = new Calificacion
        {
            ReservaId = reserva.Id,
            AtencionId = reserva.Atencion.Id,
            Puntuacion = request.Puntuacion,
            Comentario = string.IsNullOrWhiteSpace(request.Comentario)
                ? null
                : request.Comentario.Trim(),
            FechaCalificacion = DateTime.UtcNow
        };

        await _repository.CrearAsync(calificacion);
        await _repository.GuardarCambiosAsync();

        return Map(calificacion);
    }

    public async Task<CalificacionDto?> ObtenerPorReservaAsync(Guid reservaId)
    {
        var calificacion = await _repository.ObtenerPorReservaAsync(reservaId);
        return calificacion is null ? null : Map(calificacion);
    }

    public async Task<ResumenProfesionalDto> ObtenerResumenProfesionalAsync(Guid profesionalId)
    {
        var profesional = await _repository.ObtenerProfesionalAsync(profesionalId)
            ?? throw new InvalidOperationException("El profesional indicado no existe.");

        var distribucion = new List<DistribucionEstrellasDto>(5);
        var total = 0;
        var suma = 0;

        for (var estrellas = 1; estrellas <= 5; estrellas++)
        {
            var cantidad = await _repository.ContarPorProfesionalAsync(profesionalId, estrellas);
            total += cantidad;
            suma += estrellas * cantidad;
            distribucion.Add(new DistribucionEstrellasDto
            {
                Estrellas = estrellas,
                Cantidad = cantidad
            });
        }

        return new ResumenProfesionalDto
        {
            ProfesionalId = profesionalId,
            ProfesionalNombre = $"{profesional.Nombres} {profesional.Apellidos}".Trim(),
            Promedio = total == 0 ? null : Math.Round((double)suma / total, 2),
            TotalCalificaciones = total,
            Distribucion = distribucion
        };
    }

    public async Task<IReadOnlyCollection<CalificacionDto>> ListarPorProfesionalAsync(
        Guid profesionalId,
        int pagina,
        int tamanoPagina,
        int? puntuacion)
    {
        if (pagina < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.", nameof(pagina));

        if (tamanoPagina is < 1 or > 100)
            throw new ArgumentException("El tamaño de página debe estar entre 1 y 100.", nameof(tamanoPagina));

        if (puntuacion.HasValue && puntuacion.Value is < 1 or > 5)
            throw new ArgumentException("La puntuación debe estar entre 1 y 5.", nameof(puntuacion));

        _ = await _repository.ObtenerProfesionalAsync(profesionalId)
            ?? throw new InvalidOperationException("El profesional indicado no existe.");

        var calificaciones = await _repository.ListarPorProfesionalAsync(
            profesionalId,
            pagina,
            tamanoPagina,
            puntuacion);

        return calificaciones.Select(Map).ToArray();
    }

    private static CalificacionDto Map(Calificacion calificacion) => new()
    {
        Id = calificacion.Id,
        ReservaId = calificacion.ReservaId,
        Puntuacion = calificacion.Puntuacion,
        Comentario = calificacion.Comentario,
        FechaCalificacion = calificacion.FechaCalificacion
    };
}
