using System.ComponentModel.DataAnnotations;

namespace ReservaTuCitaYa.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; init; } = string.Empty;

    public bool Recordarme { get; init; }
}

public sealed record AuthUserResponse(
       Guid Id,
       string Email,
       string[] Roles,
       string[] Permisos,
       OrganizacionResumenDto? Organizacion,
       Guid? ClienteId,
       Guid? EmpleadoId);

public sealed record OrganizacionResumenDto(Guid Id, string Nombre);

public sealed record AntiforgeryTokenResponse(
    string RequestToken,
    string HeaderName);
