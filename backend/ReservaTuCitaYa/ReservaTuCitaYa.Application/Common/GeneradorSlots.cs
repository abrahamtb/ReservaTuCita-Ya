namespace ReservaTuCitaYa.Application.Common.Disponibilidad;

public sealed record TiemposServicio(int PreparacionMinutos, int DuracionMinutos, int PosteriorMinutos)
{
    public int TotalMinutos => PreparacionMinutos + DuracionMinutos + PosteriorMinutos;
}

public static class GeneradorSlots
{
    public static List<TimeOnly> Generar(
        IReadOnlyList<Intervalo> disponibles, TiemposServicio tiempos, int pasoMinutos, TimeOnly? noAntesDe = null)
    {
        var resultado = new List<TimeOnly>();
        foreach (var intervalo in disponibles)
        {
            var candidato = intervalo.Inicio.AddMinutes(tiempos.PreparacionMinutos);
            if (noAntesDe.HasValue && candidato < noAntesDe.Value)
            {
                var minutosDesdeInicio = (noAntesDe.Value - candidato).TotalMinutes;
                var pasos = Math.Ceiling(minutosDesdeInicio / pasoMinutos);
                candidato = candidato.AddMinutes(pasos * pasoMinutos);
            }

            while (true)
            {
                var inicioOcupacion = candidato.AddMinutes(-tiempos.PreparacionMinutos);
                var finOcupacion = candidato.AddMinutes(tiempos.DuracionMinutos + tiempos.PosteriorMinutos);
                if ((candidato - TimeOnly.MinValue).TotalMinutes +
                    tiempos.DuracionMinutos + tiempos.PosteriorMinutos >= 24 * 60) break;
                if (inicioOcupacion < intervalo.Inicio) { candidato = candidato.AddMinutes(pasoMinutos); continue; }
                if (finOcupacion > intervalo.Fin) break;
                resultado.Add(candidato);
                candidato = candidato.AddMinutes(pasoMinutos);
            }
        }
        return resultado;
    }
}
