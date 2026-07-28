namespace ReservaTuCitaYa.Infrastructure.Identity
{
    public static class RoleNames
    {
        public const string Superadministrador = "Superadministrador";
        public const string Administrador = "Administrador";
        public const string Recepcionista = "Recepcionista";
        public const string Profesional = "Profesional";
        public const string Cliente = "Cliente";

        public static IReadOnlyCollection<string> Todos { get; } =
        [
            Superadministrador,
            Administrador,
            Recepcionista,
            Profesional,
            Cliente
        ];
    }
}
