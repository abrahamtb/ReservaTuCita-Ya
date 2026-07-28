# ReservaTuCita Ya

Plataforma multisectorial de reservas, atención y gestión de servicios desarrollada con ASP.NET Core MVC, Identity, Entity Framework Core y SQL Server.

## Requisitos previos

- .NET SDK 8.
- Visual Studio 2022 con desarrollo web de ASP.NET.
- SQL Server Express, SQL Server Developer o LocalDB.
- Herramientas de Entity Framework Core (`dotnet-ef`) 8.

## Configuración local segura

El proyecto Web carga la conexión y las credenciales iniciales mediante User Secrets. No agregues contraseñas a `appsettings.json` ni crees un archivo `secrets.json` dentro del repositorio.

Desde la carpeta `ReservaTuCitaYa/ReservaTuCitaYa.Web`, configura una de estas conexiones.

SQL Server Express:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=ReservaTuCitaYaDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

LocalDB:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=ReservaTuCitaYaDb;Trusted_Connection=True;MultipleActiveResultSets=True"
```

Configura las credenciales iniciales con valores propios:

```powershell
dotnet user-secrets set "SeedAdmin:Email" "tu-correo-administrativo"
dotnet user-secrets set "SeedAdmin:Password" "tu-contraseña-segura"
dotnet user-secrets set "SeedAdmin:Nombres" "Tus nombres"
dotnet user-secrets set "SeedAdmin:Apellidos" "Tus apellidos"
```

## Migraciones y base de datos

Desde la carpeta que contiene la solución:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add InitialCreate --project ReservaTuCitaYa.Infrastructure --startup-project ReservaTuCitaYa.Web --context ApplicationDbContext --output-dir Data/Migrations
dotnet tool run dotnet-ef database update --project ReservaTuCitaYa.Infrastructure --startup-project ReservaTuCitaYa.Web --context ApplicationDbContext
```

En la Consola del Administrador de paquetes de Visual Studio, los comandos equivalentes son:

```powershell
Add-Migration InitialCreate -Project ReservaTuCitaYa.Infrastructure -StartupProject ReservaTuCitaYa.Web -Context ApplicationDbContext -OutputDir Data/Migrations
Update-Database -Project ReservaTuCitaYa.Infrastructure -StartupProject ReservaTuCitaYa.Web -Context ApplicationDbContext
```

La aplicación también aplica migraciones pendientes durante el inicio antes de crear los datos de Identity.

## Ejecución

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project ReservaTuCitaYa.Web
```

Durante el primer inicio se crean, de forma idempotente, los roles `Superadministrador`, `Administrador`, `Recepcionista`, `Profesional` y `Cliente`, además del superadministrador configurado en User Secrets.

## Comprobación en SQL Server

En SQL Server Management Studio, conéctate a tu instancia y comprueba:

- La base `ReservaTuCitaYaDb`.
- Las tablas de Identity con prefijo `AspNet`.
- Las tablas de negocio.
- La tabla `__EFMigrationsHistory`.
- Los cinco registros de `AspNetRoles`.
- El superadministrador y su relación con el rol en `AspNetUserRoles`.

No consultes ni compartas `PasswordHash`, tokens o valores almacenados en User Secrets.
