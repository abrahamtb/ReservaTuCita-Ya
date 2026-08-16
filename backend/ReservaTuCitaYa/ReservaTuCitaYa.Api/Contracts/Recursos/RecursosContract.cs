// Api/Contracts/Recursos/RecursoRequests.cs
using ReservaTuCitaYa.Application.DTOs.Recursos;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Api.Contracts.Recursos;

public sealed record CrearRecursosRequest(
    string Nombre, string? Codigo, string? Descripcion, string TipoRecurso,
    int Capacidad, string? UbicacionInterna, string? Observaciones,
    IReadOnlyList<AsignacionServicioRecurso> Servicios);

public sealed record ActualizarRecursosRequest(
    string Nombre, string? Codigo, string? Descripcion, string TipoRecurso,
    int Capacidad, string? UbicacionInterna, string? Observaciones);

public sealed record CambiarEstadoRecursosRequest(bool EstaActivo);

public sealed record ReemplazarServiciosRecursosRequest(
    IReadOnlyList<AsignacionServicioRecurso> Servicios);

public sealed record CrearBloqueoRequest(
    DateTime FechaHoraInicio, DateTime FechaHoraFin, TipoBloqueo TipoBloqueo,
    string Motivo, string? Observaciones);

public sealed record ActualizarBloqueoRequest(
    DateTime FechaHoraInicio, DateTime FechaHoraFin, TipoBloqueo TipoBloqueo,
    string Motivo, string? Observaciones);