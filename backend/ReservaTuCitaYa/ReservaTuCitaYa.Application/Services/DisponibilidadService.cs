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
        if ((solicitud.FechaHasta.ToDateTime(TimeOnly.MinValue) - solicitud.FechaDesde.ToDateTime(TimeOnly.MinValue)).Days
            > _opciones.RangoMaximoDias)
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

        var contexto = await CargarContextoAsync(sede, servicio, solicitud.FechaDesde, solicitud.FechaHasta, ct);

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
        var profesionales = await repository.ObtenerProfesionalesCompatiblesAsync(servicioId, sedeId, ct);
        var lista = profesionales
            .Select(p => new ProfesionalDisponibleDto(p.Id, $"{p.Nombres} {p.Apellidos}".Trim()))
            .ToList();
        return ResultadoOperacion<IReadOnlyList<ProfesionalDisponibleDto>>.Exito(lista);
    }

    public async Task<ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>> ListarRecursosCompatiblesAsync(
        Guid sedeId, Guid servicioId, DateOnly? fecha, CancellationToken ct = default)
    {
        var recursos = await repository.ObtenerRecursosCompatiblesAsync(servicioId, sedeId, ct);
        var lista = recursos.Select(r => new RecursoDisponibleDto(r.Id, r.Nombre)).ToList();
        return ResultadoOperacion<IReadOnlyList<RecursoDisponibleDto>>.Exito(lista);
    }

    public async Task<bool> ValidarSlotEspecificoAsync(
        Guid sedeId, Guid servicioId, DateOnly fecha, TimeOnly horaInicio,
        Guid? profesionalId, Guid? recursoId, CancellationToken ct = default)
    {
        var sede = await repository.ObtenerSedeAsync(sedeId, ct);
        var servicio = await repository.ObtenerServicioAsync(servicioId, ct);
        if (sede is null || sede.EstaEliminado || !sede.EstaActivo) return false;
        if (servicio is null || servicio.EstaEliminado || !servicio.EstaActivo) return false;
        var servicioSede = await repository.ObtenerServicioSedeAsync(servicioId, sedeId, ct);
        if (servicioSede is null || !servicioSede.EstaActivo || servicioSede.EstaEliminado) return false;

        var contexto = await CargarContextoAsync(sede, servicio, fecha, fecha, ct);
        var tiempos = new TiemposServicio(
            servicio.TiempoPreparacionMinutos, servicio.DuracionMinutos, servicio.TiempoPosteriorMinutos);

        var horarios = CalcularHorariosDelDia(fecha, servicio, contexto, tiempos, null, profesionalId, recursoId);
        return horarios.Any(h => h.HoraInicio == horaInicio);
    }

    public async Task<bool> ProfesionalDisponibleAsync(
        Guid profesionalId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        CancellationToken ct = default)
    {
        var empleado = await repository.ObtenerProfesionalAsync(profesionalId, ct);
        if (empleado is null || empleado.EstaEliminado || !empleado.EstaActivo || !empleado.EsProfesional) return false;

        var horarios = await repository.ObtenerHorariosProfesionalesAsync(new[] { profesionalId }, Guid.Empty, ct);
        var todos = await ObtenerTodosLosHorariosDelProfesionalAsync(profesionalId, ct);
        var excepciones = await repository.ObtenerExcepcionesProfesionalesAsync(new[] { profesionalId }, fecha, fecha, ct);

        var efectivo = CalcularEfectivoEntidad(fecha, todos.Select(h => new IntervaloSemanal(h.DiaSemana, h.HoraInicio, h.HoraFin)).ToList(),
            excepciones.Select(e => new ExcepcionDia(e.TipoExcepcion, e.HoraInicio, e.HoraFin)).ToList());

        return CalculadorIntervalos.CabeCompleto(efectivo, inicioOcupacion, finOcupacion);
    }

    public async Task<Guid?> ObtenerProfesionalDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        CancellationToken ct = default)
    {
        var candidatos = await repository.ObtenerProfesionalesCompatiblesAsync(servicioId, sedeId, ct);
        foreach (var candidato in candidatos)
            if (await ProfesionalDisponibleAsync(candidato.Id, fecha, inicioOcupacion, finOcupacion, ct))
                return candidato.Id;
        return null;
    }

    public async Task<bool> RecursoDisponibleAsync(
        Guid recursoId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        CancellationToken ct = default)
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
            .Select(b => new Intervalo(TimeOnly.FromDateTime(b.FechaHoraInicio), TimeOnly.FromDateTime(b.FechaHoraFin)))
            .ToList();
        var final = CalculadorIntervalos.Restar(efectivo, bloqueosDelDia);

        return CalculadorIntervalos.CabeCompleto(final, inicioOcupacion, finOcupacion);
    }

    public async Task<Guid?> ObtenerRecursoDisponibleAsync(
        Guid servicioId, Guid sedeId, DateOnly fecha, TimeOnly inicioOcupacion, TimeOnly finOcupacion,
        CancellationToken ct = default)
    {
        var candidatos = await repository.ObtenerRecursosCompatiblesAsync(servicioId, sedeId, ct);
        foreach (var candidato in candidatos)
            if (await RecursoDisponibleAsync(candidato.Id, fecha, inicioOcupacion, finOcupacion, ct))
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
            return resultado;
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
                    if (resultado.All(h => h.HoraInicio != inicio))
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
                    if (resultado.All(h => h.HoraInicio != inicio))
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
                        if (resultado.All(h => h.HoraInicio != inicio))
                            resultado.Add(new HorarioDisponibleDto(
                                inicio, inicio.AddMinutes(servicio.DuracionMinutos),
                                inicio.AddMinutes(servicio.DuracionMinutos + servicio.TiempoPosteriorMinutos),
                                profesional.Id, $"{profesional.Nombres} {profesional.Apellidos}".Trim(),
                                recurso.Id, recurso.Nombre, null));
                }
            }
        }

        return resultado.OrderBy(h => h.HoraInicio).ToList();
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
            .Select(b => new Intervalo(TimeOnly.FromDateTime(b.FechaHoraInicio), TimeOnly.FromDateTime(b.FechaHoraFin)))
            .ToList();
        return CalculadorIntervalos.Restar(efectivo, bloqueosDelDia);
    }

    private static List<Intervalo> CalcularEfectivoEntidad(
        DateOnly fecha, IReadOnlyList<IntervaloSemanal> horarioSemanal, IReadOnlyList<ExcepcionDia> excepciones) =>
        CalculadorHorarioEfectivo.Calcular(MapearDiaSemana(fecha.DayOfWeek), horarioSemanal, excepciones);

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
        Sede sede, Servicio servicio, DateOnly desde, DateOnly hasta, CancellationToken ct)
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

        return new ContextoDisponibilidad(
            profesionales, recursos, horariosSede, excepcionesSede,
            horariosProfesional, excepcionesProfesional, horariosRecurso, excepcionesRecurso, bloqueos);
    }

    /// Variante sin filtro de sede, usada para revalidar un profesional específico en cualquier sede donde trabaje.
    private async Task<IReadOnlyList<HorarioProfesional>> ObtenerTodosLosHorariosDelProfesionalAsync(
        Guid empleadoId, CancellationToken ct)
    {
        // Reutiliza el mismo repositorio; si prefieres, agrega un método dedicado sin filtro de sede.
        var sedesConocidas = await repository.ObtenerProfesionalesCompatiblesAsync(Guid.Empty, Guid.Empty, ct);
        return await repository.ObtenerHorariosProfesionalesAsync(new[] { empleadoId }, Guid.Empty, ct);
    }

    private sealed record ContextoDisponibilidad(
        IReadOnlyList<Empleado> Profesionales, IReadOnlyList<Recurso> Recursos,
        IReadOnlyList<HorarioSede> HorariosSede, IReadOnlyList<ExcepcionHorarioSede> ExcepcionesSede,
        IReadOnlyList<HorarioProfesional> HorariosProfesional, IReadOnlyList<ExcepcionHorarioProfesional> ExcepcionesProfesional,
        IReadOnlyList<HorarioRecurso> HorariosRecurso, IReadOnlyList<ExcepcionHorarioRecurso> ExcepcionesRecurso,
        IReadOnlyList<BloqueoRecurso> Bloqueos);
}