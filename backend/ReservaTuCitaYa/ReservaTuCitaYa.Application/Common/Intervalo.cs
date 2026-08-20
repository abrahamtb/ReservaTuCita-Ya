namespace ReservaTuCitaYa.Application.Common.Disponibilidad;

public readonly record struct Intervalo(TimeOnly Inicio, TimeOnly Fin)
{
    public bool EsValido => Inicio < Fin;
}