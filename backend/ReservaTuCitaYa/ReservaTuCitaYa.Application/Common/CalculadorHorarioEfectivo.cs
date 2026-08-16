using ReservaTuCitaYa.Domain.Enums;
namespace ReservaTuCitaYa.Application.Common.Disponibilidad;

public sealed record IntervaloSemanal(DiaSemana DiaSemana, TimeOnly Inicio, TimeOnly Fin);
public sealed record ExcepcionDia(TipoExcepcionHorario Tipo, TimeOnly? Inicio, TimeOnly? Fin);

public static class CalculadorHorarioEfectivo
{
    public static List<Intervalo> Calcular(
        DiaSemana dia,
        IReadOnlyList<IntervaloSemanal> horarioSemanal,
        IReadOnlyList<ExcepcionDia> excepcionesDelDia)
    {
        if (excepcionesDelDia.Any(e => e.Tipo == TipoExcepcionHorario.CerradoTodoElDia))
            return [];

        var especiales = excepcionesDelDia
            .Where(e => e.Tipo == TipoExcepcionHorario.HorarioEspecial)
            .Select(e => new Intervalo(e.Inicio!.Value, e.Fin!.Value))
            .ToList();

        var baseIntervalos = especiales.Count > 0
            ? CalculadorIntervalos.Normalizar(especiales)
            : CalculadorIntervalos.Normalizar(horarioSemanal
                .Where(h => h.DiaSemana == dia)
                .Select(h => new Intervalo(h.Inicio, h.Fin))
                .ToList());

        var parciales = excepcionesDelDia
            .Where(e => e.Tipo == TipoExcepcionHorario.NoDisponibleParcial)
            .Select(e => new Intervalo(e.Inicio!.Value, e.Fin!.Value))
            .ToList();

        return parciales.Count > 0
            ? CalculadorIntervalos.Restar(baseIntervalos, parciales)
            : baseIntervalos;
    }
}