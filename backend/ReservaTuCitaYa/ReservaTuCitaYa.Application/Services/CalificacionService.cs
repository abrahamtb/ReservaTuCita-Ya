using ReservaTuCitaYa.Application.DTOs.Calificaciones;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Application.Interfaces.Repository;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservaTuCitaYa.Application.Services
{
    public sealed class CalificacionService : ICalificacionService
    {
        private readonly ICalificacionRepository _repository;

        public CalificacionService(ICalificacionRepository repository)
        {
            _repository = repository;
        }

        public async Task<CalificacionDto> CrearCalificacionAsync(Guid reservaId, CrearCalificacionRequest request)
        {
            if (request.Puntuacion < 1 || request.Puntuacion > 5)
                throw new ArgumentException("La puntuación debe estar entre 1 y 5.");

            var existe = await _repository.ExistePorReservaAsync(reservaId);
            if (existe)
                throw new InvalidOperationException("La atención ya fue calificada.");

            var calificacion = new Calificacion
            {
                Id = Guid.NewGuid(),
                ReservaId = reservaId,
                AtencionId = Guid.Empty, 
                Puntuacion = request.Puntuacion,
                Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim(),
                FechaCalificacion = DateTime.UtcNow
            };

            await _repository.CrearAsync(calificacion);
            await _repository.GuardarCambiosAsync();

            return new CalificacionDto
            {
                Id = calificacion.Id,
                ReservaId = calificacion.ReservaId,
                Puntuacion = calificacion.Puntuacion,
                Comentario = calificacion.Comentario,
                FechaCalificacion = calificacion.FechaCalificacion
            };
        }

        public async Task<CalificacionDto?> ObtenerPorReservaAsync(Guid reservaId)
        {
            var calificacion = await _repository.ObtenerPorReservaAsync(reservaId);
            if (calificacion is null) return null;

            return new CalificacionDto
            {
                Id = calificacion.Id,
                ReservaId = calificacion.ReservaId,
                Puntuacion = calificacion.Puntuacion,
                Comentario = calificacion.Comentario,
                FechaCalificacion = calificacion.FechaCalificacion
            };
        }

        public async Task<ResumenProfesionalDto> ObtenerResumenProfesionalAsync(Guid profesionalId)
        {
        
            return new ResumenProfesionalDto
            {
                ProfesionalId = profesionalId,
                ProfesionalNombre = "Pendiente",
                Promedio = null,
                TotalCalificaciones = 0,
                Distribucion = Enumerable.Range(1, 5)
                    .Select(e => new DistribucionEstrellasDto { Estrellas = e, Cantidad = 0 })
                    .ToList()
            };
        }

        public async Task<IReadOnlyCollection<CalificacionDto>> ListarPorProfesionalAsync(Guid profesionalId, int pagina, int tamanoPagina, int? puntuacion)
        {
            return Array.Empty<CalificacionDto>();
        }
    }

}
