namespace ReservaTuCitaYa.Application.Common.Disponibilidad;

public static class CalculadorIntervalos
{
    public static List<Intervalo> Intersectar(IReadOnlyList<Intervalo> a, IReadOnlyList<Intervalo> b)
    {
        var resultado = new List<Intervalo>();
        foreach (var ia in a)
            foreach (var ib in b)
            {
                var inicio = ia.Inicio > ib.Inicio ? ia.Inicio : ib.Inicio;
                var fin = ia.Fin < ib.Fin ? ia.Fin : ib.Fin;
                if (inicio < fin) resultado.Add(new Intervalo(inicio, fin));
            }
        return Normalizar(resultado);
    }

    public static List<Intervalo> Restar(IReadOnlyList<Intervalo> baseIntervalos, IReadOnlyList<Intervalo> aRestar)
    {
        var resultado = new List<Intervalo>(baseIntervalos);
        foreach (var quitar in aRestar)
        {
            var siguiente = new List<Intervalo>();
            foreach (var actual in resultado)
            {
                if (quitar.Fin <= actual.Inicio || quitar.Inicio >= actual.Fin)
                {
                    siguiente.Add(actual); // no se tocan
                    continue;
                }
                if (quitar.Inicio > actual.Inicio)
                    siguiente.Add(new Intervalo(actual.Inicio, quitar.Inicio));
                if (quitar.Fin < actual.Fin)
                    siguiente.Add(new Intervalo(quitar.Fin, actual.Fin));
            }
            resultado = siguiente;
        }
        return Normalizar(resultado);
    }

    public static List<Intervalo> Normalizar(IReadOnlyList<Intervalo> intervalos)
    {
        var validos = intervalos.Where(i => i.EsValido).OrderBy(i => i.Inicio).ToList();
        var resultado = new List<Intervalo>();
        foreach (var actual in validos)
        {
            if (resultado.Count > 0 && actual.Inicio <= resultado[^1].Fin)
            {
                var ultimo = resultado[^1];
                if (actual.Fin > ultimo.Fin)
                    resultado[^1] = new Intervalo(ultimo.Inicio, actual.Fin);
            }
            else
            {
                resultado.Add(actual);
            }
        }
        return resultado;
    }

    public static bool CabeCompleto(IReadOnlyList<Intervalo> disponibles, TimeOnly inicio, TimeOnly fin) =>
        disponibles.Any(d => inicio >= d.Inicio && fin <= d.Fin);
}