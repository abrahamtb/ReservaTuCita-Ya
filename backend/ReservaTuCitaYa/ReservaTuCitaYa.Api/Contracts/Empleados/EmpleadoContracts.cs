using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Api.Contracts.Empleados;

public class GuardarEmpleadoRequest
{
    public TipoDocumento TipoDocumento { get; init; }
    public string NumeroDocumento { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public DateOnly? FechaNacimiento { get; init; }
    public string Cargo { get; init; } = string.Empty;
    public string? Especialidad { get; init; }
    public bool EsProfesional { get; init; }
    public string? NumeroColegiatura { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearEmpleadoRequest : GuardarEmpleadoRequest
{
    public IReadOnlyList<Guid> SedeIds { get; init; } = [];
    public IReadOnlyList<Guid> ServicioIds { get; init; } = [];
}

public sealed class ActualizarEmpleadoRequest : GuardarEmpleadoRequest;

public sealed class CambiarEstadoEmpleadoRequest
{
    public bool EstaActivo { get; init; }
}

public sealed class ReemplazarSedesEmpleadoRequest
{
    public IReadOnlyList<Guid> SedeIds { get; init; } = [];
}

public sealed class ReemplazarServiciosProfesionalRequest
{
    public IReadOnlyList<Guid> ServicioIds { get; init; } = [];
}
