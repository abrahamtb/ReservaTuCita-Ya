using ReservaTuCitaYa.Domain.Enums;

namespace ReservaTuCitaYa.Api.Contracts.Clientes;

public class GuardarClienteRequest
{
    public TipoDocumento TipoDocumento { get; init; }
    public string NumeroDocumento { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public DateOnly? FechaNacimiento { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class CrearClienteRequest : GuardarClienteRequest;
public sealed class ActualizarClienteRequest : GuardarClienteRequest;

public sealed class CambiarEstadoClienteRequest
{
    public bool EstaActivo { get; init; }
}
