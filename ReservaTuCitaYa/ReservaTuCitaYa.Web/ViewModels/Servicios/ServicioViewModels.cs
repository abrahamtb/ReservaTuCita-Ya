using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaTuCitaYa.Application.DTOs.Common;
using ReservaTuCitaYa.Application.DTOs.Servicios;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Web.ViewModels.Servicios;

public sealed class ServicioIndexViewModel
{
    public Guid OrganizacionId { get; init; }
    public string Organizacion { get; init; } = string.Empty;
    public string? Busqueda { get; init; }
    public Guid? CategoriaServicioId { get; init; }
    public ModalidadServicio? Modalidad { get; init; }
    public EstadoFiltro Estado { get; init; }
    public IReadOnlyList<SelectListItem> Categorias { get; init; } = [];
    public PaginaResultado<ServicioListaDto> Resultado { get; init; } = new([], 1, 10, 0);
}

public sealed class SedeSeleccionViewModel
{
    public Guid SedeId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Seleccionada { get; set; }

    [Display(Name = "Precio especial")]
    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "El precio especial no puede ser negativo.")]
    public decimal? PrecioEspecial { get; set; }
}

public sealed class ServicioFormularioViewModel
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Organizacion { get; set; } = string.Empty;

    [Display(Name = "Categoría")]
    [Required(ErrorMessage = "Seleccione una categoría.")]
    public Guid? CategoriaServicioId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción admite hasta 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Duración (minutos)")]
    [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor que cero.")]
    public int DuracionMinutos { get; set; } = 30;

    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Display(Name = "Monto de adelanto")]
    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "El adelanto no puede ser negativo.")]
    public decimal MontoAdelanto { get; set; }

    [Required(ErrorMessage = "Seleccione una modalidad.")]
    public ModalidadServicio? Modalidad { get; set; }

    [Display(Name = "Servicio grupal")]
    public bool EsGrupal { get; set; }

    [Display(Name = "Capacidad máxima")]
    [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor que cero.")]
    public int CapacidadMaxima { get; set; } = 1;

    [Display(Name = "Requiere profesional")]
    public bool RequiereProfesional { get; set; }
    [Display(Name = "Requiere recurso")]
    public bool RequiereRecurso { get; set; }
    [Display(Name = "Permite cancelación")]
    public bool PermiteCancelacion { get; set; }
    [Display(Name = "Permite reprogramación")]
    public bool PermiteReprogramacion { get; set; }

    [Display(Name = "Horas límite de cancelación")]
    [Range(0, int.MaxValue, ErrorMessage = "El valor no puede ser negativo.")]
    public int HorasLimiteCancelacion { get; set; }

    [Display(Name = "Preparación (minutos)")]
    [Range(0, int.MaxValue, ErrorMessage = "El valor no puede ser negativo.")]
    public int TiempoPreparacionMinutos { get; set; }

    [Display(Name = "Tiempo posterior (minutos)")]
    [Range(0, int.MaxValue, ErrorMessage = "El valor no puede ser negativo.")]
    public int TiempoPosteriorMinutos { get; set; }

    public IReadOnlyList<SelectListItem> Categorias { get; set; } = [];
    public List<SedeSeleccionViewModel> Sedes { get; set; } = [];
}
