using System.ComponentModel.DataAnnotations;
using ReservaTuCitaYa.Application.DTOs.CategoriasServicio;
using ReservaTuCitaYa.Application.DTOs.Common;

namespace ReservaTuCitaYa.Web.ViewModels.CategoriasServicio;

public sealed class CategoriaServicioIndexViewModel
{
    public Guid OrganizacionId { get; init; }
    public string Organizacion { get; init; } = string.Empty;
    public string? Busqueda { get; init; }
    public EstadoFiltro Estado { get; init; }
    public PaginaResultado<CategoriaServicioListaDto> Resultado { get; init; } =
        new([], 1, 10, 0);
}

public sealed class CategoriaServicioFormularioViewModel
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Organizacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción admite hasta 500 caracteres.")]
    public string? Descripcion { get; set; }
}
