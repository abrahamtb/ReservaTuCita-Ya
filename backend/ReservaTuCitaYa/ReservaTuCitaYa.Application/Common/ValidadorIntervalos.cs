namespace ReservaTuCitaYa.Application.Common;

public static class ValidadorIntervalos
{
    public static bool SeSuperponen(TimeOnly inicioA, TimeOnly finA, TimeOnly inicioB, TimeOnly finB) =>
        inicioA < finB && finA > inicioB;

    public static bool EstaContenidoEn(TimeOnly inicio, TimeOnly fin, TimeOnly rangoInicio, TimeOnly rangoFin) =>
        inicio >= rangoInicio && fin <= rangoFin;

    public static string? ValidarColeccionSemana<T>(
        IReadOnlyList<T> intervalos,
        Func<T, ReservaTuCitaYa.Domain.Enums.DiaSemana> dia,
        Func<T, TimeOnly> inicio,
        Func<T, TimeOnly> fin)
    {
        foreach (var i in intervalos)
        {
            if (inicio(i) >= fin(i)) return "La hora de inicio debe ser menor a la hora de fin.";
        }
        var porDia = intervalos.GroupBy(dia);
        foreach (var grupo in porDia)
        {
            var lista = grupo.ToList();
            for (var i = 0; i < lista.Count; i++)
                for (var j = i + 1; j < lista.Count; j++)
                    if (SeSuperponen(inicio(lista[i]), fin(lista[i]), inicio(lista[j]), fin(lista[j])))
                        return $"Existen intervalos superpuestos el día {grupo.Key}.";
        }
        return null;
    }
}