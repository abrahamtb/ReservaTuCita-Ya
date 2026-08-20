# Reserva tu Cita Ya

Plataforma académica de reservas con backend ASP.NET Core 8, frontend React/TypeScript, Entity Framework Core 8, SQL Server Express y ASP.NET Core Identity.

## Estructura del repositorio

```text
backend/
└── ReservaTuCitaYa/
    ├── ReservaTuCitaYa.sln
    ├── ReservaTuCitaYa.Api/
    ├── ReservaTuCitaYa.Application/
    ├── ReservaTuCitaYa.Domain/
    ├── ReservaTuCitaYa.Infrastructure/
    ├── ReservaTuCitaYa.UnitTests/
    └── ReservaTuCitaYa.IntegrationTests/

frontend/
└── ReservaTuCitaYa/
    ├── package.json
    └── src/
```

## Arquitectura

```text
React + TypeScript (Vite)
        │ HTTPS/JSON
        ▼
ASP.NET Core Web API
        │
        ├── Application (servicios y contratos)
        ├── Infrastructure (EF Core, Identity y repositorios)
        └── Domain (entidades y enumeraciones)
                │
                ▼
          SQL Server Express
```

La presentación MVC/Razor fue retirada. `ReservaTuCitaYa.Api` es el único host del backend y React nunca se conecta directamente a SQL Server.

## Requisitos

- .NET SDK 8.
- Node.js 20 o posterior y npm.
- Visual Studio 2022 con desarrollo web ASP.NET.
- SQL Server Express.
- Certificado HTTPS de desarrollo confiable: `dotnet dev-certs https --trust`.
- Herramienta `dotnet-ef` 8.

## Configurar User Secrets

La API mantiene el `UserSecretsId` utilizado durante el desarrollo anterior, por lo que los secretos locales existentes siguen funcionando. Desde `backend/ReservaTuCitaYa/ReservaTuCitaYa.Api`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=ReservaTuCitaYaDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "SeedAdmin:Email" "tu-correo-administrativo"
dotnet user-secrets set "SeedAdmin:Password" "tu-contraseña-segura"
dotnet user-secrets set "SeedAdmin:Nombres" "Tus nombres"
dotnet user-secrets set "SeedAdmin:Apellidos" "Tus apellidos"
```

No agregues conexiones, contraseñas o secretos al frontend ni a `appsettings.json`.

## Ejecutar el backend

Desde `backend/ReservaTuCitaYa` (la carpeta que contiene la solución):

```powershell
dotnet restore
dotnet build
dotnet run --project ReservaTuCitaYa.Api --launch-profile https
```

- API HTTPS: `https://localhost:7264`.
- Swagger: `https://localhost:7264/swagger`.
- API HTTP auxiliar: `http://localhost:5264`.

La API aplica las migraciones existentes y ejecuta los seeders idempotentes de roles y superadministrador al iniciar.

## Ejecutar React

Desde `frontend/ReservaTuCitaYa`:

```powershell
Copy-Item .env.example .env
npm install
npm run dev
```

El frontend abre en `https://localhost:5173` y obtiene la API mediante:

```text
VITE_API_URL=https://localhost:7264
```

`.env`, `node_modules` y `dist` están ignorados por Git. Si Vite utiliza otro puerto, actualiza `Frontend:Url` en la configuración de API y reinicia ambos hosts; el origen debe coincidir exactamente.

## Sesión, CORS y antiforgery

- Identity conserva autenticación mediante cookie `HttpOnly` y `Secure`; React nunca lee ni guarda la cookie.
- Todas las solicitudes usan `credentials: "include"`.
- La política CORS `ReactFrontend` permite únicamente el origen de `Frontend:Url`, junto con credenciales.
- Antes de un POST, PUT, PATCH o DELETE, el cliente solicita `GET /api/antiforgery/token` y envía el token en `X-XSRF-TOKEN`.
- Después del login se solicita un token nuevo porque cambió la identidad asociada a la sesión.
- 401 y 403 se devuelven como ProblemDetails JSON, sin redirecciones HTML.

Para probar escrituras desde Swagger:

1. Ejecuta `GET /api/antiforgery/token`.
2. Copia `requestToken`.
3. Ejecuta `POST /api/auth/login` agregando `X-XSRF-TOKEN` con ese valor.
4. Vuelve a solicitar un token después del login.
5. Usa el token renovado en cada operación mutable. El navegador conserva la cookie de sesión.

## Endpoints principales

```text
GET  /api/antiforgery/token
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me

GET|POST       /api/organizaciones
GET|PUT|DELETE /api/organizaciones/{id}
PATCH          /api/organizaciones/{id}/estado
GET            /api/organizaciones/tipos

GET|POST       /api/organizaciones/{organizacionId}/sedes
GET|PUT|DELETE /api/sedes/{id}
PATCH          /api/sedes/{id}/estado

GET|POST       /api/organizaciones/{organizacionId}/categorias
GET|PUT|DELETE /api/categorias/{id}
PATCH          /api/categorias/{id}/estado

GET|POST       /api/organizaciones/{organizacionId}/servicios
GET|PUT|DELETE /api/servicios/{id}
PATCH          /api/servicios/{id}/estado
```

Swagger documenta también los endpoints auxiliares para opciones de categorías y sedes.

## Migraciones existentes

Para aplicar las migraciones existentes, ejecuta desde `backend/ReservaTuCitaYa`:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project ReservaTuCitaYa.Infrastructure --startup-project ReservaTuCitaYa.Api --context ApplicationDbContext
```

No ejecutes `migrations add` si no realizaste cambios nuevos en el modelo.

### Estado de RG-014

RG-014 utiliza una sola migración incremental: `20260806231410_AddRecursosHorariosDisponibilidad`. La configuración explícita de `HorarioRecurso`, los filtros globales relacionados y el snapshot de EF Core están sincronizados. La migración duplicada `20260807022144_AgregarHorariosYRecursos` no forma parte de la versión consolidada.

## Validación

Desde `backend/ReservaTuCitaYa`:

```powershell
dotnet build --no-restore
dotnet test ReservaTuCitaYa.UnitTests --no-build
dotnet test ReservaTuCitaYa.IntegrationTests --no-build
```

Desde el frontend:

```powershell
npm run lint
npm run build
```

Las pruebas API utilizan `WebApplicationFactory` y una base SQL Server Express aislada y desechable. No usan EF InMemory.

## Solución de problemas

- **CORS:** confirma que Vite use exactamente el mismo esquema, host y puerto configurados en `Frontend:Url`.
- **Cookie no enviada:** usa `localhost` en ambos proyectos, confía el certificado HTTPS y no mezcles `localhost` con `127.0.0.1`.
- **400 antiforgery:** recarga la página o vuelve a solicitar el token; después del login siempre se renueva.
- **401:** inicia sesión nuevamente; la sesión pudo vencer o el usuario pudo desactivarse.
- **403:** el usuario necesita temporalmente el rol `Administrador` o `Superadministrador`.
- **SQL Server:** comprueba que la instancia `SQLEXPRESS` esté iniciada y que User Secrets contenga `DefaultConnection`.

## Trabajo posterior

- RG-018 debe comenzar solo después de aprobar esta separación y resolver el riesgo de persistencia de RG-014.
- RG-030 deberá reemplazar la autorización temporal por políticas, contexto de organización, permisos detallados y menús definitivos.
