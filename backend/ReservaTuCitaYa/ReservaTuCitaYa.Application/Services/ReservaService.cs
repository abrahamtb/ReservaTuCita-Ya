using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Reservas;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Services;

public sealed class ReservaService(
    IReservaRepository reservaRepository,
    IOrganizacionRepository organizacionRepository,
    ISedeRepository sedeRepository,
    IClienteRepository clienteRepository,
    IServicioRepository servicioRepository,
    IEmpleadoRepository empleadoRepository,
    IRecursoRepository recursoRepository,
    IDisponibilidadService disponibilidadService) : IReservaService
{
    public const string ClienteInvalido = "El cliente no existe o está inactivo.";
    public const string ServicioInvalido = "El servicio no existe o está inactivo.";
    public const string SedeInvalida = "La sede no existe o está inactiva.";
    public const string ServicioNoOfrecidoEnSede = "El servicio no se ofrece en esta sede.";
    public const string FechaHoraPasada = "No se pueden crear reservas en el pasado.";
    public const string SinCombinacionDisponible = "No hay una combinación de profesional/recurso disponible para ese horario.";
    public const string HorarioOcupado = "El horario fue ocupado por otro usuario.";
    public const string CapacidadExcedida = "La cantidad de participantes supera la capacidad disponible.";
    public const string ParticipantesInvalidos = "Debe existir exactamente un titular entre los participantes.";

    public async Task<ResultadoOperacion<ReservaCreadaDto>> CrearAsync(
        Guid organizacionId, CrearReservaSolicitud solicitud, string? usuarioId,
        CancellationToken ct = default)
    {
        var errorBasico = ValidarBasico(solicitud);
        if (errorBasico is not null) return ResultadoOperacion<ReservaCreadaDto>.Fallo(errorBasico);

        var organizacion = await organizacionRepository.ObtenerParaModificarAsync(organizacionId, ct);
        if (organizacion is null || organizacion.EstaEliminado || !organizacion.EstaActivo)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo("La organización no existe o está inactiva.", TipoErrorOperacion.NoEncontrado);

        var sede = await sedeRepository.ObtenerParaModificarAsync(solicitud.SedeId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo || sede.OrganizacionId != organizacionId)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        var cliente = await clienteRepository.ObtenerParaModificarAsync(solicitud.ClienteId, ct);
        if (cliente is null || cliente.EstaEliminado || !cliente.EstaActivo)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(ClienteInvalido, TipoErrorOperacion.NoEncontrado);

        var servicio = await servicioRepository.ObtenerParaModificarAsync(solicitud.ServicioId, ct);
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo || servicio.OrganizacionId != organizacionId)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(ServicioInvalido, TipoErrorOperacion.NoEncontrado);

        var servicioSede = await servicioRepository.ObtenerServicioSedeAsync(solicitud.ServicioId, solicitud.SedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(ServicioNoOfrecidoEnSede, TipoErrorOperacion.Conflicto);

        var ahora = DateTime.Now;
        if (solicitud.Fecha < DateOnly.FromDateTime(ahora) ||
            (solicitud.Fecha == DateOnly.FromDateTime(ahora) && solicitud.HoraInicio <= TimeOnly.FromDateTime(ahora)))
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(FechaHoraPasada, TipoErrorOperacion.Validacion);

        var errorParticipantes = ValidarParticipantes(solicitud, servicio.CapacidadMaxima, servicio.EsGrupal);
        if (errorParticipantes is not null)
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(errorParticipantes);

        var precioTotal = servicioSede.PrecioEspecial ?? servicio.Precio;
        var horaFinServicio = solicitud.HoraInicio.AddMinutes(servicio.DuracionMinutos);
        var horaInicioOcupacion = solicitud.HoraInicio.AddMinutes(-servicio.TiempoPreparacionMinutos);
        var horaFinOcupacion = horaFinServicio.AddMinutes(servicio.TiempoPosteriorMinutos);

        try
        {
            return await reservaRepository.EjecutarEnTransaccionAsync(async innerCt =>
            {
                Guid? profesionalId = null;
                if (servicio.RequiereProfesional)
                {
                    var resuelto = await ResolverProfesionalAsync(
                        solicitud.ProfesionalId, solicitud.ServicioId, solicitud.SedeId,
                        solicitud.Fecha, horaInicioOcupacion, horaFinOcupacion, innerCt);
                    if (resuelto is null)
                        throw new ConflictoDisponibilidadException(
                            solicitud.ProfesionalId.HasValue ? HorarioOcupado : SinCombinacionDisponible);
                    profesionalId = resuelto;
                }

                Guid? recursoId = null;
                if (servicio.RequiereRecurso)
                {
                    var resuelto = await ResolverRecursoAsync(
                        solicitud.RecursoId, solicitud.ServicioId, solicitud.SedeId,
                        solicitud.Fecha, horaInicioOcupacion, horaFinOcupacion, innerCt);
                    if (resuelto is null)
                        throw new ConflictoDisponibilidadException(
                            solicitud.RecursoId.HasValue ? HorarioOcupado : SinCombinacionDisponible);
                    recursoId = resuelto;
                }

                var slotValido = await disponibilidadService.ValidarSlotEspecificoAsync(
                    solicitud.SedeId, solicitud.ServicioId, solicitud.Fecha, solicitud.HoraInicio,
                    profesionalId, recursoId, innerCt);
                if (!slotValido)
                    throw new ConflictoDisponibilidadException(HorarioOcupado);

                if (!servicio.EsGrupal)
                {
                    var conflictos = await reservaRepository.ObtenerConflictosAsync(
                        profesionalId, recursoId, solicitud.Fecha, ct: innerCt);
                    var haySolapamiento = conflictos.Any(r =>
                        EstadosReserva.OcupanHorario.Contains(r.EstadoReserva) &&
                        horaInicioOcupacion < r.HoraFinOcupacion && horaFinOcupacion > r.HoraInicioOcupacion);
                    if (haySolapamiento)
                        throw new ConflictoDisponibilidadException(HorarioOcupado);
                }
                else
                {
                    var ocupada = await reservaRepository.ObtenerCapacidadOcupadaAsync(
                        solicitud.ServicioId, solicitud.SedeId, solicitud.Fecha, solicitud.HoraInicio,
                        profesionalId, recursoId, ct: innerCt);
                    if (ocupada + solicitud.CantidadParticipantes > servicio.CapacidadMaxima)
                        throw new ConflictoDisponibilidadException(CapacidadExcedida);
                }

                var codigo = await GenerarCodigoUnicoAsync(innerCt);

                var reserva = new Reserva
                {
                    Codigo = codigo,
                    OrganizacionId = organizacionId,
                    SedeId = solicitud.SedeId,
                    ClienteId = solicitud.ClienteId,
                    ServicioId = solicitud.ServicioId,
                    ProfesionalId = profesionalId,
                    RecursoId = recursoId,
                    Fecha = solicitud.Fecha,
                    HoraInicio = solicitud.HoraInicio,
                    HoraFinServicio = horaFinServicio,
                    HoraInicioOcupacion = horaInicioOcupacion,
                    HoraFinOcupacion = horaFinOcupacion,
                    DuracionMinutos = servicio.DuracionMinutos,
                    TiempoPreparacionMinutos = servicio.TiempoPreparacionMinutos,
                    TiempoPosteriorMinutos = servicio.TiempoPosteriorMinutos,
                    PrecioTotal = precioTotal,
                    AdelantoRequerido = servicio.MontoAdelanto > 0 ? servicio.MontoAdelanto : null,
                    EsGrupal = servicio.EsGrupal,
                    CapacidadMaxima = servicio.CapacidadMaxima,
                    CantidadParticipantes = solicitud.CantidadParticipantes,
                    EstadoReserva = EstadoReserva.Confirmada,
                    Observaciones = string.IsNullOrWhiteSpace(solicitud.Observaciones) ? null : solicitud.Observaciones.Trim()
                };
                reservaRepository.Agregar(reserva);

                foreach (var p in solicitud.Participantes)
                    reservaRepository.AgregarParticipante(new ReservaParticipante
                    {
                        ReservaId = reserva.Id,
                        ClienteId = p.ClienteId,
                        NombreCompleto = p.NombreCompleto.Trim(),
                        EsTitular = p.EsTitular,
                        Observaciones = string.IsNullOrWhiteSpace(p.Observaciones) ? null : p.Observaciones.Trim()
                    });

                reservaRepository.AgregarHistorial(new HistorialReserva
                {
                    ReservaId = reserva.Id,
                    EstadoAnterior = null,
                    EstadoNuevo = EstadoReserva.Confirmada,
                    TipoAccion = TipoAccionReserva.Creada,
                    FechaAccion = DateTime.UtcNow,
                    UsuarioId = usuarioId
                });

                await reservaRepository.GuardarAsync(innerCt);

                return ResultadoOperacion<ReservaCreadaDto>.Exito(new ReservaCreadaDto(
                    reserva.Id, reserva.Codigo, reserva.EstadoReserva.ToString(),
                    new EntidadResumenDto(cliente.Id, ConstruirNombreCompleto(cliente)),
                    new EntidadResumenDto(servicio.Id, servicio.Nombre),
                    new EntidadResumenDto(sede.Id, sede.Nombre),
                    profesionalId.HasValue ? new EntidadResumenDto(profesionalId.Value, "") : null,
                    recursoId.HasValue ? new EntidadResumenDto(recursoId.Value, "") : null,
                    reserva.Fecha, reserva.HoraInicio, reserva.HoraFinServicio,
                    reserva.DuracionMinutos, reserva.CantidadParticipantes,
                    reserva.PrecioTotal, reserva.AdelantoRequerido));
            }, ct);
        }
        catch (ConflictoDisponibilidadException ex)
        {
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(ex.Message, TipoErrorOperacion.Conflicto);
        }
        catch (ConflictoPersistenciaException)
        {
            return ResultadoOperacion<ReservaCreadaDto>.Fallo(HorarioOcupado, TipoErrorOperacion.Conflicto);
        }
    }

    public async Task<ResultadoOperacion<ReservaDetalleDto>> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var detalle = await reservaRepository.ObtenerDetalleAsync(id, ct);
        return detalle is null
            ? ResultadoOperacion<ReservaDetalleDto>.Fallo("La reserva no existe.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<ReservaDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<ReservaDetalleDto>> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var detalle = await reservaRepository.ObtenerDetallePorCodigoAsync(codigo, ct);
        return detalle is null
            ? ResultadoOperacion<ReservaDetalleDto>.Fallo("La reserva no existe.", TipoErrorOperacion.NoEncontrado)
            : ResultadoOperacion<ReservaDetalleDto>.Exito(detalle);
    }

    public async Task<ResultadoOperacion<PaginaResultado<ReservaListaDto>>> ListarAsync(
        ReservaFiltroDto filtro, CancellationToken ct = default) =>
        ResultadoOperacion<PaginaResultado<ReservaListaDto>>.Exito(await reservaRepository.ListarAsync(filtro, ct));

    // --- Helpers privados ---

    private async Task<Guid?> ResolverProfesionalAsync(
        Guid? profesionalId, Guid servicioId, Guid sedeId, DateOnly fecha,
        TimeOnly inicioOcupacion, TimeOnly finOcupacion, CancellationToken ct)
    {
        if (profesionalId.HasValue)
        {
            var esValido = await EsProfesionalCompatibleAsync(profesionalId.Value, servicioId, sedeId, ct);
            if (!esValido) return null;
            var libre = await disponibilidadService.ProfesionalDisponibleAsync(
                profesionalId.Value, fecha, inicioOcupacion, finOcupacion, ct);
            return libre ? profesionalId : null;
        }
        return await disponibilidadService.ObtenerProfesionalDisponibleAsync(
            servicioId, sedeId, fecha, inicioOcupacion, finOcupacion, ct);
    }

    private async Task<Guid?> ResolverRecursoAsync(
        Guid? recursoId, Guid servicioId, Guid sedeId, DateOnly fecha,
        TimeOnly inicioOcupacion, TimeOnly finOcupacion, CancellationToken ct)
    {
        if (recursoId.HasValue)
        {
            var esValido = await EsRecursoCompatibleAsync(recursoId.Value, servicioId, sedeId, ct);
            if (!esValido) return null;
            var libre = await disponibilidadService.RecursoDisponibleAsync(
                recursoId.Value, fecha, inicioOcupacion, finOcupacion, ct);
            return libre ? recursoId : null;
        }
        return await disponibilidadService.ObtenerRecursoDisponibleAsync(
            servicioId, sedeId, fecha, inicioOcupacion, finOcupacion, ct);
    }

    /// El empleado debe estar activo, ser profesional, estar asignado a la sede y tener el servicio asignado.
    private async Task<bool> EsProfesionalCompatibleAsync(
        Guid empleadoId, Guid servicioId, Guid sedeId, CancellationToken ct)
    {
        var empleado = await empleadoRepository.ObtenerParaModificarAsync(empleadoId, ct);
        if (empleado is null || empleado.EstaEliminado || !empleado.EstaActivo || !empleado.EsProfesional)
            return false;

        var sedes = await empleadoRepository.ObtenerRelacionesSedeAsync(empleadoId, ct);
        if (sedes.All(s => s.SedeId != sedeId || !s.EstaActivo)) return false;

        var servicios = await empleadoRepository.ObtenerRelacionesServicioAsync(empleadoId, ct);
        return servicios.Any(s => s.ServicioId == servicioId && s.EstaActivo);
    }

    /// El recurso debe estar activo, pertenecer a la sede y ser compatible con el servicio.
    private async Task<bool> EsRecursoCompatibleAsync(
        Guid recursoId, Guid servicioId, Guid sedeId, CancellationToken ct)
    {
        var recurso = await recursoRepository.ObtenerParaModificarAsync(recursoId, ct);
        if (recurso is null || recurso.EstaEliminado || !recurso.EstaActivo || recurso.SedeId != sedeId)
            return false;

        var servicios = await recursoRepository.ObtenerRelacionesServicioAsync(recursoId, ct);
        return servicios.Any(s => s.ServicioId == servicioId && s.EstaActivo);
    }

    private async Task<string> GenerarCodigoUnicoAsync(CancellationToken ct)
    {
        for (var intento = 0; intento < 5; intento++)
        {
            var codigo = $"RES-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";
            if (!await reservaRepository.ExisteCodigoAsync(codigo, ct)) return codigo;
        }
        throw new InvalidOperationException("No se pudo generar un código único de reserva.");
    }

    private static string ConstruirNombreCompleto(Cliente c) => $"{c.Nombres} {c.Apellidos}".Trim();

    private static string? ValidarBasico(CrearReservaSolicitud s)
    {
        if (s.CantidadParticipantes <= 0) return "La cantidad de participantes debe ser mayor que cero.";
        if (s.Participantes.Count == 0) return "Debe registrar al menos un participante.";
        if (s.Observaciones?.Trim().Length > 1000) return "Las observaciones no pueden superar 1000 caracteres.";
        return null;
    }

    private static string? ValidarParticipantes(CrearReservaSolicitud s, int capacidadMaxima, bool esGrupal)
    {
        var titulares = s.Participantes.Count(p => p.EsTitular);
        if (titulares != 1) return ParticipantesInvalidos;
        if (s.Participantes.Any(p => p.ClienteId is null && string.IsNullOrWhiteSpace(p.NombreCompleto)))
            return "El nombre es obligatorio para participantes sin cliente registrado.";
        if (!esGrupal && s.CantidadParticipantes != 1)
            return "Un servicio individual solo admite un participante.";
        if (s.CantidadParticipantes > capacidadMaxima)
            return CapacidadExcedida;
        return null;
    }
}

internal sealed class ConflictoDisponibilidadException(string mensaje) : Exception(mensaje);