using ReservaTuCitaYa.Application.Abstractions.Persistence;
using ReservaTuCitaYa.Application.Common;
using ReservaTuCitaYa.Application.Common.Disponibilidad;
using ReservaTuCitaYa.Application.DTOs.Disponibilidad;
using ReservaTuCitaYa.Application.Interfaces;
using ReservaTuCitaYa.Domain.Entities;
using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Services;

public class DisponibilidadService(
    IDisponibilidadRepository repository,
    DisponibilidadOptions opciones)
    : IDisponibilidadService
{
    private readonly DisponibilidadOptions _opciones = opciones;

    public const string SedeInvalida = "La sede no existe, fue eliminada o está inactiva.";
    public const string ServicioInvalido = "El servicio no existe, fue eliminado o está inactivo.";
    public const string ServicioNoOfrecidoEnSede = "El servicio no se ofrece en esta sede.";
    public const string RangoInvalido = "La fecha hasta no puede ser menor que la fecha desde.";
    public const string RangoExcesivo = "El rango de fechas supera el máximo permitido.";

    public async Task<ResultadoOperacion<DisponibilidadRespuestaDto>> ConsultarAsync(
        ConsultarDisponibilidadSolicitud solicitud, CancellationToken ct = default)
    {
        if (solicitud.FechaHasta < solicitud.FechaDesde)
            return ResultadoOperacion<DisponibilidadRespuestaDto>.Fallo(RangoInvalido, TipoErrorOperacion.Validacion);
        if (solicitud.FechaHasta.DayNumber - solicitud.FechaDesde.DayNumber + 1 > _opciones.RangoMaximoDias)
            return ResultadoOperacion<DisponibilidadRespuestaDto>.Fallo(RangoExcesivo, TipoErrorOperacion.Validacion);

        var sede = await repository.ObtenerSedeAsync(solicitud.SedeId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo)
            return ResultadoOperacion<DisponibilidadRespuestaDto>.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        var servicio = await repository.ObtenerServicioAsync(solicitud.ServicioId, ct);
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo || servicio.OrganizacionId != sede.OrganizacionId)
            return ResultadoOperacion<DisponibilidadRespuestaDto>.Fallo(ServicioInvalido, TipoErrorOperacion.NoEncontrado);

        var servicioSede = await repository.ObtenerServicioSedeAsync(solicitud.ServicioId, solicitud.SedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado)
            return ResultadoOperacion<DisponibilidadRespuestaDto>.Fallo(ServicioNoOfrecidoEnSede, TipoErrorOperacion.Conflicto);

        var contexto = await CargarContextoAsync(sede, servicio, solicitud.FechaDesde, solicitud.FechaHasta, null, ct);

        var tiempos = new TiemposServicio(
            servicio.TiempoPreparacionMinutos, servicio.DuracionMinutos, servicio.TiempoPosteriorMinutos);

        var dias = new List<DisponibilidadDiaDto>();
        var ahora = DateTime.Now;
        for (var fecha = solicitud.FechaDesde; fecha <= solicitud.FechaHasta; fecha = fecha.AddDays(1))
        {
            var noAntesDe = fecha == DateOnly.FromDateTime(ahora) ? TimeOnly.FromDateTime(ahora) : (TimeOnly?)null;
            var horarios = CalcularHorariosDelDia(
                fecha, servicio, contexto, tiempos, noAntesDe, solicitud.ProfesionalId, solicitud.RecursoId);
            dias.Add(new DisponibilidadDiaDto(fecha, horarios.Count > 0, horarios));
        }

        return ResultadoOperacion<DisponibilidadRespuestaDto>.Exito(new DisponibilidadRespuestaDto(
            solicitud.SedeId, solicitud.ServicioId, servicio.DuracionMinutos,
            servicio.TiempoPreparacionMinutos, servicio.TiempoPosteriorMinutos, dias));
    }

    public async Task<ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>> ListarProfesionalesCompatiblesAsync(
        Guid sedeId, Guid servicioId, DateOnly? fecha, CancellationToken ct = default)
    {
        var sede = await repository.ObtenerSedeAsync(sedeId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo)
            return ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        var servicio = await repository.ObtenerServicioAsync(servicioId, ct);
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo || servicio.OrganizacionId != sede.OrganizacionId)
            return ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>.Fallo(ServicioInvalido, TipoErrorOperacion.NoEncontrado);

        var servicioSede = await repository.ObtenerServicioSedeAsync(servicioId, sedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado)
            return ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>.Fallo(ServicioNoOfrecidoEnSede, TipoErrorOperacion.Conflicto);

        var profesionales = await repository.ObtenerProfesionalesCompatiblesAsync(servicioId, sedeId, ct);
        if (fecha.HasValue)
        {
            var contexto = await CargarContextoAsync(sede, servicio, fecha.Value, fecha.Value, null, ct);
            var tiempos = new TiemposServicio(
                servicio.TiempoPreparacionMinutos, servicio.DuracionMinutos, servicio.TiempoPosteriorMinutos);
            profesionales = profesionales
                .Where(p => CalcularHorariosDelDia(
                    fecha.Value, servicio, contexto, tiempos, null, p.Id, null).Count > 0)
                .ToList();
        }

        var lista = profesionales
            .Select(p => new ProfesionalDisponibleDto(p.Id, $"{p.Nombres} {p.Apellidos}".Trim()))
            .ToList();
        return ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>.Exito(lista);
    }

    public async Task<ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>> ListarRecursosCompatiblesAsync(
        Guid sedeId, Guid servicioId, DateOnly? fecha, CancellationToken ct = default)
    {
        var sede = await repository.ObtenerSedeAsync(sedeId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo)
            return ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>.Fallo(SedeInvalida, TipoErrorOperacion.NoEncontrado);

        var servicio = await repository.ObtenerServicioAsync(servicioId, ct);
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo || servicio.OrganizacionId != sede.OrganizacionId)
            return ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>.Fallo(ServicioInvalido, TipoErrorOperacion.NoEncontrado);

        var servicioSede = await repository.ObtenerServicioSedeAsync(servicioId, sedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado)
            return ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>.Fallo(ServicioNoOfrecidoEnSede, TipoErrorOperacion.Conflicto);

        var recursos = await repository.ObtenerRecursosCompatiblesAsync(servicioId, sedeId, ct);
        if (fecha.HasValue)
        {
            var contexto = await CargarContextoAsync(sede, servicio, fecha.Value, fecha.Value, null, ct);
            var tiempos = new TiemposServicio(
                servicio.TiempoPreparacionMinutos, servicio.DuracionMinutos, servicio.TiempoPosteriorMinutos);
            recursos = recursos
                .Where(r => CalcularHorariosDelDia(
                    fecha.Value, servicio, contexto, tiempos, null, null, r.Id).Count > 0)
                .ToList();
        }

        var lista = recursos.Select(r => new RecursoDisponibleDto(r.Id, r.Nombre)).ToList();
        return ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>.Exito(lista);
    }

    public async Task<bool> ValidarSlotEspecificoAsync(
        Guid sedeId, Guid servicioId, DateOnly fecha, TimeOnly horaInicio,
        Guid? profesionalId, Guid? recursoId, Guid? reservaIdExcluir = null,
        CancellationToken ct = default)
    {
        var sede = await repository.ObtenerSedeAsync(sedeId, ct);
        var servicio = await repository.ObtenerServicioAsync(servicioId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo) return false;
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo ||
            servicio.OrganizacionId != sede.OrganizacionId) return false;
        var servicioSede = await repository.ObtenerServicioSedeAsync(servicioId, sedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado) return false;

        var contexto = await CargarContextoAsync(sede, servicio, fecha, fecha, reservaIdExcluir, ct);
        var tiempos = new TiemposServicio(
            servicio.TiempoPreparacionMinutos, servicio.DuracionMinutos, servicio.TiempoPosteriorMinutos);

        var horarios = CalcularHorariosDelDia(fecha, servicio, contexto, tiempos, null, profesionalId, recursoId);
        return horarios.Any(h => h.HoraInicio == horaInicio);
    }

    public async Task<bool> ProfesionalDisponibleAsync(
        Guid profesionalId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default)
    {
        var empleado = await repository.ObtenerProfesionalAsync(profesionalId, ct);
        if (empleado is null || empleado.EstaEliminado || !empleado.EstaActivo || !empleado.EsProfesional) return false;

        var horarios = await repository.ObtenerHorariosProfesionalesAsync(new[] { profesionalId }, sedeId, ct);
        var excepciones = await repository.ObtenerExcepcionesProfesionalesAsync(new[] { profesionalId }, fecha, fecha, ct);

        var efectivo = CalcularEfectivoEntidad(fecha, horarios.Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList(),
            excepciones.Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList());
        if (!CalculadorIntervalos.CabeCompleto(efectivo, inicioOcupacion, finOcupacion)) return false;
        var reservas = await repository.ObtenerReservasActivasAsync(sedeId, fecha, fecha, reservaIdExcluir, ct);
        return !reservas.Any(r => r.ProfesionalId == profesionalId &&
            inicioOcupacion < r.HoraFinOcupacion && finOcupacion > r.HoraInicioOcupacion);
    }

    public async Task<Guid?> ObtenerProfesionalDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default)
    {
        var candidatos = await repository.ObtenerProfesionalesCompatiblesAsync(servicioId, sedeId, ct);
        foreach (var candidato in candidatos)
            if (await ProfesionalDisponibleAsync(candidato.Id, sedeId, fecha, inicioOcupacion, finOcupacion, reservaIdExcluir, ct))
                return candidato.Id;
        return null;
    }

    public async Task<bool> RecursoDisponibleAsync(
        Guid recursoId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default)
    {
        var recurso = await repository.ObtenerRecursoAsync(recursoId, ct);
        if (recurso is null || recurso.EstaEliminado || !recurso.EstaActivo) return false;

        var horarios = await repository.ObtenerHorariosRecursosAsync(new[] { recursoId }, ct);
        var excepciones = await repository.ObtenerExcepcionesRecursosAsync(new[] { recursoId }, fecha, fecha, ct);
        var bloqueos = await repository.ObtenerBloqueosAsync(new[] { recursoId }, fecha, fecha, ct);

        var efectivo = CalcularEfectivoEntidad(fecha,
            horarios.Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList(),
            excepciones.Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList());

        var bloqueosDelDia = bloqueos
            .Where(b => DateOnly.FromDateTime(b.FechaHoraInicio) <= fecha && DateOnly.FromDateTime(b.FechaHoraFin) >= fecha)
            .Select(b => IntervaloBloqueoEnFecha(b, fecha))
            .ToList();
        var final = CalculadorIntervalos.Restar(efectivo, bloqueosDelDia);

        if (!CalculadorIntervalos.CabeCompleto(final, inicioOcupacion, finOcupacion)) return false;
        var reservas = await repository.ObtenerReservasActivasAsync(sedeId, fecha, fecha, reservaIdExcluir, ct);
        return !reservas.Any(r => r.RecursoId == recursoId &&
            inicioOcupacion < r.HoraFinOcupacion && finOcupacion > r.HoraInicioOcupacion);
    }

    public async Task<Guid?> ObtenerRecursoDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        Guid? reservaIdExcluir = null, CancellationToken ct = default)
    {
        var candidatos = await repository.ObtenerRecursosCompatiblesAsync(servicioId, sedeId, ct);
        foreach (var candidato in candidatos)
            if (await RecursoDisponibleAsync(candidato.Id, sedeId, fecha, inicioOcupacion, finOcupacion, reservaIdExcluir, ct))
                return candidato.Id;
        return null;
    }

    private List<HorarioDisponibleDto> CalcularHorariosDelDia(
        DateOnly fecha, Servicio servicio, ContextoDisponibilidad contexto, TiemposServicio tiempos,
        TimeOnly? noAntesDe, Guid? profesionalIdFiltro, Guid? recursoIdFiltro)
    {
        var dia = MapearDiaSemana(fecha.DayOfWeek);

        var efectivoSede = CalcularEfectivoEntidad(fecha,
            contexto.HorariosSede.Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList(),
            contexto.ExcepcionesSede.Where(e => e.Fecha == fecha)
                .Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList());

        if (efectivoSede.Count == 0) return [];

        var resultado = new List<HorarioDisponibleDto>();

        if (!servicio.RequiereProfesional && !servicio.RequiereRecurso)
        {
            var slots = GeneradorSlots.Generar(efectivoSede, tiempos, _opciones.PasoMinutos, noAntesDe);
            foreach (var inicio in slots)
                resultado.Add(new HorarioDisponibleDto(
                    inicio, inicio.AddMinutes(servicio.DuracionMinutos),
                    inicio.AddMinutes(servicio.DuracionMinutos + servicio.TiempoPosteriorMinutos),
                    null, null, null, null, null));
            return FiltrarPorReservas(fecha, servicio, tiempos, contexto, resultado);
        }

        var profesionalesAConsiderar = profesionalIdFiltro.HasValue
            ? contexto.Profesionales.Where(p => p.Id == profesionalIdFiltro.Value).ToList()
            : contexto.Profesionales;
        var recursosAConsiderar = recursoIdFiltro.HasValue
            ? contexto.Recursos.Where(r => r.Id == recursoIdFiltro.Value).ToList()
            : contexto.Recursos;

        if (servicio.RequiereProfesional && !servicio.RequiereRecurso)
        {
            foreach (var profesional in profesionalesAConsiderar)
            {
                var efectivoProf = CalcularEfectivoProfesional(fecha, profesional.Id, contexto);
                var interseccion = CalculadorIntervalos.Intersectar(efectivoSede, efectivoProf);
                var slots = GeneradorSlots.Generar(interseccion, tiempos, _opciones.PasoMinutos, noAntesDe);
                foreach (var inicio in slots)
                    resultado.Add(new HorarioDisponibleDto(
                            inicio, inicio.AddMinutes(servicio.DuracionMinutos),
                            inicio.AddMinutes(servicio.DuracionMinutos + servicio.TiempoPosteriorMinutos),
                            profesional.Id, $"{profesional.Nombres} {profesional.Apellidos}".Trim(),
                            null, null, null));
            }
        }
        else if (!servicio.RequiereProfesional && servicio.RequiereRecurso)
        {
            foreach (var recurso in recursosAConsiderar)
            {
                var efectivoRec = CalcularEfectivoRecurso(fecha, recurso.Id, contexto);
                var interseccion = CalculadorIntervalos.Intersectar(efectivoSede, efectivoRec);
                var slots = GeneradorSlots.Generar(interseccion, tiempos, _opciones.PasoMinutos, noAntesDe);
                foreach (var inicio in slots)
                    resultado.Add(new HorarioDisponibleDto(
                            inicio, inicio.AddMinutes(servicio.DuracionMinutos),
                            inicio.AddMinutes(servicio.DuracionMinutos + servicio.TiempoPosteriorMinutos),
                            null, null, recurso.Id, recurso.Nombre, null));
            }
        }
        else
        {
            foreach (var profesional in profesionalesAConsiderar)
            {
                var efectivoProf = CalcularEfectivoProfesional(fecha, profesional.Id, contexto);
                var conSede = CalculadorIntervalos.Intersectar(efectivoSede, efectivoProf);
                if (conSede.Count == 0) continue;

                foreach (var recurso in recursosAConsiderar)
                {
                    var efectivoRec = CalcularEfectivoRecurso(fecha, recurso.Id, contexto);
                    var combinado = CalculadorIntervalos.Intersectar(conSede, efectivoRec);
                    if (combinado.Count == 0) continue;

                    var slots = GeneradorSlots.Generar(combinado, tiempos, _opciones.PasoMinutos, noAntesDe);
                    foreach (var inicio in slots)
                        resultado.Add(new HorarioDisponibleDto(
                                inicio, inicio.AddMinutes(servicio.DuracionMinutos),
                                inicio.AddMinutes(servicio.DuracionMinutos + servicio.TiempoPosteriorMinutos),
                                profesional.Id, $"{profesional.Nombres} {profesional.Apellidos}".Trim(),
                                recurso.Id, recurso.Nombre, null));
                }
            }
        }

        return FiltrarPorReservas(fecha, servicio, tiempos, contexto, resultado)
            .OrderBy(h => h.HoraInicio).ThenBy(h => h.ProfesionalNombre).ThenBy(h => h.RecursoNombre).ToList();
    }

    private static List<HorarioDisponibleDto> FiltrarPorReservas(
        DateOnly fecha, Servicio servicio, TiemposServicio tiempos,
        ContextoDisponibilidad contexto, IEnumerable<HorarioDisponibleDto> candidatos)
    {
        var reservas = contexto.Reservas.Where(r => r.Fecha == fecha).ToList();
        var resultado = new List<HorarioDisponibleDto>();
        foreach (var slot in candidatos)
        {
            var inicioOcupacion = slot.HoraInicio.AddMinutes(-tiempos.PreparacionMinutos);
            var finOcupacion = slot.HoraFinOcupacion;
            var mismaCombinacion = reservas.Where(r =>
                (!slot.ProfesionalId.HasValue || r.ProfesionalId == slot.ProfesionalId) &&
                (!slot.RecursoId.HasValue || r.RecursoId == slot.RecursoId) &&
                (slot.ProfesionalId.HasValue || slot.RecursoId.HasValue || r.ServicioId == servicio.Id));

            if (!servicio.EsGrupal)
            {
                var conflicto = mismaCombinacion.Any(r =>
                    inicioOcupacion < r.HoraFinOcupacion && finOcupacion > r.HoraInicioOcupacion);
                if (!conflicto) resultado.Add(slot);
                continue;
            }

            var ocupada = mismaCombinacion.Where(r => r.ServicioId == servicio.Id && r.HoraInicio == slot.HoraInicio)
                .Sum(r => r.CantidadParticipantes);
            var disponible = servicio.CapacidadMaxima - ocupada;
            if (disponible > 0) resultado.Add(slot with { CapacidadDisponible = disponible });
        }
        return resultado;
    }

    private static List<Intervalo> CalcularEfectivoProfesional(DateOnly fecha, Guid empleadoId, ContextoDisponibilidad c)
    {
        var horarios = c.HorariosProfesional.Where(h => h.EmpleadoId == empleadoId)
            .Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList();
        var excepciones = c.ExcepcionesProfesional.Where(e => e.EmpleadoId == empleadoId && e.Fecha == fecha)
            .Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList();
        return CalcularEfectivoEntidad(fecha, horarios, excepciones);
    }

    private static List<Intervalo> CalcularEfectivoRecurso(DateOnly fecha, Guid recursoId, ContextoDisponibilidad c)
    {
        var horarios = c.HorariosRecurso.Where(h => h.RecursoId == recursoId)
            .Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList();
        var excepciones = c.ExcepcionesRecurso.Where(e => e.RecursoId == recursoId && e.Fecha == fecha)
            .Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList();
        var efectivo = CalcularEfectivoEntidad(fecha, horarios, excepciones);

        var bloqueosDelDia = c.Bloqueos.Where(b => b.RecursoId == recursoId &&
                DateOnly.FromDateTime(b.FechaHoraInicio) <= fecha && DateOnly.FromDateTime(b.FechaHoraFin) >= fecha)
            .Select(b => IntervaloBloqueoEnFecha(b, fecha))
            .ToList();
        return CalculadorIntervalos.Restar(efectivo, bloqueosDelDia);
    }

    private static List<Intervalo> CalcularEfectivoEntidad(
        DateOnly fecha, IReadOnlyList<IntervaloSemanal> horarioSemanal, IReadOnlyList<ExcepcionDia> excepciones) =>
        CalculadorHorarioEfectivo.Calcular(MapearDiaSemana(fecha.DayOfWeek), horarioSemanal, excepciones);

    private static Intervalo IntervaloBloqueoEnFecha(BloqueoRecurso bloqueo, DateOnly fecha)
    {
        var fechaInicio = DateOnly.FromDateTime(bloqueo.FechaHoraInicio);
        var fechaFin = DateOnly.FromDateTime(bloqueo.FechaHoraFin);
        var inicio = fechaInicio < fecha ? TimeOnly.MinValue : TimeOnly.FromDateTime(bloqueo.FechaHoraInicio);
        var fin = fechaFin > fecha ? TimeOnly.MaxValue : TimeOnly.FromDateTime(bloqueo.FechaHoraFin);
        return new Intervalo(inicio, fin);
    }

    private static DiaSemana MapearDiaSemana(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => DiaSemana.Lunes,
        DayOfWeek.Tuesday => DiaSemana.Martes,
        DayOfWeek.Wednesday => DiaSemana.Miercoles,
        DayOfWeek.Thursday => DiaSemana.Jueves,
        DayOfWeek.Friday => DiaSemana.Viernes,
        DayOfWeek.Saturday => DiaSemana.Sabado,
        _ => DiaSemana.Domingo
    };

    private async Task<ContextoDisponibilidad> CargarContextoAsync(
        Sede sede, Servicio servicio, DateOnly desde, DateOnly hasta, Guid? reservaIdExcluir, CancellationToken ct)
    {
        var profesionales = servicio.RequiereProfesional
            ? await repository.ObtenerProfesionalesCompatiblesAsync(servicio.Id, sede.Id, ct)
            : [];
        var recursos = servicio.RequiereRecurso
            ? await repository.ObtenerRecursosCompatiblesAsync(servicio.Id, sede.Id, ct)
            : [];

        var empleadoIds = profesionales.Select(p => p.Id).ToList();
        var recursoIds = recursos.Select(r => r.Id).ToList();

        var horariosSede = await repository.ObtenerHorariosSedeAsync(sede.Id, ct);
        var excepcionesSede = await repository.ObtenerExcepcionesSedeAsync(sede.Id, desde, hasta, ct);

        var horariosProfesional = empleadoIds.Count > 0
            ? await repository.ObtenerHorariosProfesionalesAsync(empleadoIds, sede.Id, ct) : [];
        var excepcionesProfesional = empleadoIds.Count > 0
            ? await repository.ObtenerExcepcionesProfesionalesAsync(empleadoIds, desde, hasta, ct) : [];

        var horariosRecurso = recursoIds.Count > 0
            ? await repository.ObtenerHorariosRecursosAsync(recursoIds, ct) : [];
        var excepcionesRecurso = recursoIds.Count > 0
            ? await repository.ObtenerExcepcionesRecursosAsync(recursoIds, desde, hasta, ct) : [];
        var bloqueos = recursoIds.Count > 0
            ? await repository.ObtenerBloqueosAsync(recursoIds, desde, hasta, ct) : [];
        var reservas = await repository.ObtenerReservasActivasAsync(sede.Id, desde, hasta, reservaIdExcluir, ct);

        return new ContextoDisponibilidad(
            profesionales, recursos, horariosSede, excepcionesSede,
            horariosProfesional, excepcionesProfesional, horariosRecurso, excepcionesRecurso, bloqueos, reservas);
    }

    private sealed record ContextoDisponibilidad(
        IReadOnlyList<Empleado> Profesionales, IReadOnlyList<Recurso> Recursos,
        IReadOnlyList<HorarioSede> HorariosSede, IReadOnlyList<ExcepcionHorarioSede> ExcepcionesSede,
        IReadOnlyList<HorarioProfesional> HorariosProfesional, IReadOnlyList<ExcepcionHorarioProfesional> ExcepcionesProfesional,
        IReadOnlyList<HorarioRecurso> HorariosRecurso, IReadOnlyList<ExcepcionHorarioRecurso> ExcepcionesRecurso,
        IReadOnlyList<BloqueoRecurso> Bloqueos, IReadOnlyList<Reserva> Reservas);
}
