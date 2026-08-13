using ReservaTuCitaYa.Domain.Common;
using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Domain.Entities;

public sealed class Empleado : BaseEntity
{
    public Guid OrganizacionId { get; set; }
    public TipoDocumento TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public bool EsProfesional { get; set; }
    public string? NumeroColegiatura { get; set; }
    public string? Observaciones { get; set; }

    public Organizacion Organizacion { get; set; } = null!;
    public ICollection<EmpleadoSede> Sedes { get; set; } = new List<EmpleadoSede>();
    public ICollection<ProfesionalServicio> ServiciosProfesionales { get; set; } =
        new List<ProfesionalServicio>();
}
