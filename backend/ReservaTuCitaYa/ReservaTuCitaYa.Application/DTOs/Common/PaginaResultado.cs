namespace ReservaTuCitaYa.Application.DTOs.Common;

public sealed record PaginaResultado<T>(
    IReadOnlyList<T> Elementos,
    int PaginaActual,
    int TamanoPagina,
    int TotalElementos)
{
    public int TotalPaginas => TotalElementos == 0
        ? 1
        : (int)Math.Ceiling(TotalElementos / (double)TamanoPagina);

    public bool TieneAnterior => PaginaActual > 1;
    public bool TieneSiguiente => PaginaActual < TotalPaginas;
}
