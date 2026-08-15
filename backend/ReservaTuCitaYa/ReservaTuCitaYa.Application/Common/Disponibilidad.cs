namespace ReservaTuCitaYa.Application.Common.Disponibilidad;

public sealed class DisponibilidadOptions
{
    public const string Seccion = "Disponibilidad";
    public int PasoMinutos { get; set; } = 15;
    public int RangoMaximoDias { get; set; } = 31;
}