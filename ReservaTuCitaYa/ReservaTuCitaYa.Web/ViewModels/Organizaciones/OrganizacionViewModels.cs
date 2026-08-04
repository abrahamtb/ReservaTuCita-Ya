using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Organizaciones;

namespace ReservaTuCitaYa.Web.ViewModels.Organizaciones;

public sealed class OrganizacionIndexViewModel
{
    public string? Busqueda { get; init; }
    public EstadoFiltro Estado { get; init; }
    public IReadOnlyList<OrganizacionListaDto> Organizaciones { get; init; } = [];
}

public sealed class OrganizacionFormularioViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Tipo de organización")]
    [Required(ErrorMessage = "Seleccione el tipo de organización.")]
    public Guid? TipoOrganizacionId { get; set; }

    [Display(Name = "Nombre comercial")]
    [Required(ErrorMessage = "El nombre comercial es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre comercial admite hasta 150 caracteres.")]
    public string NombreComercial { get; set; } = string.Empty;

    [Display(Name = "Razón social")]
    [StringLength(200, ErrorMessage = "La razón social admite hasta 200 caracteres.")]
    public string? RazonSocial { get; set; }

    [Display(Name = "Número de documento")]
    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número de documento admite hasta 20 caracteres.")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [StringLength(30, ErrorMessage = "El teléfono admite hasta 30 caracteres.")]
    public string? Telefono { get; set; }

    [Display(Name = "Correo electrónico")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(256, ErrorMessage = "El correo admite hasta 256 caracteres.")]
    public string? Correo { get; set; }

    [Display(Name = "Dirección principal")]
    [StringLength(250, ErrorMessage = "La dirección admite hasta 250 caracteres.")]
    public string? DireccionPrincipal { get; set; }

    [Display(Name = "URL del logo")]
    [Url(ErrorMessage = "Ingrese una URL válida.")]
    [StringLength(500, ErrorMessage = "La URL admite hasta 500 caracteres.")]
    public string? LogoUrl { get; set; }

    public IReadOnlyList<SelectListItem> TiposOrganizacion { get; set; } = [];
}
