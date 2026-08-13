namespace ReservaTuCitaYa.Api.Contracts.Usuarios
{
    public sealed record CrearUsuarioRequest(
        string Email,
        string Password,
        string Nombres,
        string Apellidos,
        string NumeroDocumento,
        string Telefono,
        string Rol,
        Guid? OrganizacionId);

    public sealed record UsuarioResponse(
        Guid Id,
        string Email,
        string Nombres,
        string Apellidos,
        bool EstaActivo,
        string[] Roles,
        Guid? OrganizacionId);

    public sealed record AsignarRolRequest(string Rol);

    public sealed record AsignarOrganizacionRequest(Guid OrganizacionId, bool EsPrincipal);
}
