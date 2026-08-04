using System.ComponentModel.DataAnnotations;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Sedes;

namespace ReservaTuCitaYa.Web.ViewModels.Sedes;

public sealed class SedeIndexViewModel
{
    public Guid OrganizacionId { get; init; }
    public string Organizacion { get; init; } = string.Empty;
    public string? Busqueda { get; init; }
    public EstadoFiltro Estado { get; init; }
    public IReadOnlyList<SedeListaDto> Sedes { get; init; } = [];
}

public sealed class SedeFormularioViewModel
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Organizacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250, ErrorMessage = "La dirección admite hasta 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [StringLength(30, ErrorMessage = "El teléfono admite hasta 30 caracteres.")]
    public string? Telefono { get; set; }

    [Display(Name = "Correo electrónico")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(256, ErrorMessage = "El correo admite hasta 256 caracteres.")]
    public string? Correo { get; set; }

    [StringLength(500, ErrorMessage = "La referencia admite hasta 500 caracteres.")]
    public string? Referencia { get; set; }
}
