# Auditoría y corrección RG-020 a RG-024

Fecha: 2026-08-15
Base revisada: `origin/develop` (`922c042`)
Rama local de revisión: `Errores/auditoria-rg020-rg024`
Estado: cambios locales, sin commit, push, PR ni merge.

## A. Estado Git

- La copia principal del usuario continúa en `UI-001-sistema-diseno-react`, HEAD `fb08782`, con sus cambios frontend intactos.
- La auditoría se realizó en un worktree aislado para no mezclar UI-001 con backend.
- Se encontró la rama remota `origin/RG-020-021-022-023-024-La-mitad-de-backend`, HEAD `4d37016`.
- Se encontró la referencia GitHub `refs/pull/22/head` para ese trabajo. No existe evidencia local de un merge del PR y no fue posible consultar su estado remoto mediante la API privada.
- `origin/develop` no contenía RG-020 a RG-024; solo contenía la base hasta RG-030 y RG-018-FE. La implementación estaba únicamente en la rama remota/PR.
- La corrección no modificó esa rama remota: su diff se aplicó sin crear merge commit sobre una rama local nacida de `develop`.

## B. Estado inicial reproducido

- Build inicial de la rama RG: correcto, 0 errores y 0 advertencias.
- Unit inicial: 145 total, 144 aprobadas, 1 fallida. Falla: `EstadoReserva` no tenía valor cero.
- Integration inicial: 45 total, 44 aprobadas, 1 fallida. Falla: EF creaba la FK sombra `BloqueoRecurso.RecursoId1` y usaba `ClientSetNull`.
- Total inicial: 190, aprobadas 188, fallidas 2.
- EF inicial: migraciones RG-020 y RG-021 pendientes, cambios de modelo sin migración, relaciones con filtros globales incompatibles, FK sombra y precisión decimal sin configurar.
- RG-024 tenía entidades y DTO, pero no operaciones de servicio ni endpoints.
- RG-022 no descontaba reservas activas; la disponibilidad profesional consultaba una sede vacía (`Guid.Empty`).

## C. RG-020 — Recursos y bloqueos

Se conservó el modelo existente `Recurso`, `ServicioRecurso` y `BloqueoRecurso`, eliminando la clase duplicada `BloqueoRecursos`. Se corrigieron organización/sede/servicio, código único, estado inicial, sincronización de estado del recurso, soft delete, índices y relaciones EF. Un recurso inactivo o eliminado ya no puede bloquearse ni participar en disponibilidad.

El solapamiento usa `nuevoInicio < existenteFin && nuevoFin > existenteInicio`; detecta solapamiento parcial, contenido y contenedor, pero permite intervalos adyacentes. Crear/actualizar bloqueo valida y guarda dentro de una transacción serializable para cerrar la carrera entre comprobar e insertar.

Pruebas añadidas: solapamientos parciales/completos, contención, adyacencia y reglas de intervalos.

## D. RG-021 — Horarios y excepciones

Se mantuvo una única estructura de horarios recurrentes para sede, empleado profesional y recurso. Se corrigió la relación profesional/sede, se añadieron repositorios, servicios y endpoints que faltaban, y se validan intervalos, solapamientos, pertenencia a sede/organización, actividad y soft delete.

Las excepciones por fecha tienen precedencia: `Cerrado` elimina todo el día y `HorarioEspecial` sustituye el horario semanal. No se aceptan enums cero ni excepciones temporales solapadas. Se verificaron horarios adyacentes, cerrados y especiales.

## E. RG-022 — Disponibilidad

No se creó tabla de slots. El resultado se calcula en memoria después de cargar conjuntos acotados desde SQL:

1. horario recurrente de sede;
2. excepción de sede;
3. intersección con horario/excepción del profesional, si se requiere;
4. intersección con horario/excepción del recurso, si se requiere;
5. resta de bloqueos del recurso;
6. generación cada 15 minutos;
7. comprobación de preparación + duración + tiempo posterior;
8. resta de reservas `Confirmada` o `Reprogramada` y aplicación de capacidad grupal.

Se corrigieron los bloqueos que cruzan fechas, los slots que cruzaban medianoche, el rango máximo (31 días inclusivos), profesionales/recursos de otra organización, filtros activos/eliminados y las listas de candidatos con fecha: ahora realmente exigen al menos un slot libre. Una reserva cancelada no ocupa; una reprogramada sí.

No existe consulta SQL por cada slot. El contexto se carga por rango y el cálculo se efectúa sobre colecciones ya obtenidas.

## F. RG-023 — Crear reserva

Crear reserva vuelve a validar disponibilidad dentro de la operación de persistencia. Valida organización, cliente principal, participantes, servicio, sede, profesional, recurso, compatibilidad y capacidad. Conserva snapshots de precio, duración, preparación, post y capacidad, genera código único y crea el evento inicial de historial.

La operación usa una transacción SQL Server `Serializable`. Dentro de ella se recalculan disponibilidad, conflictos y capacidad antes de insertar. Los índices de profesional/recurso/fecha/horas permiten que los range locks serialicen reservas incompatibles. Conflictos de clave, deadlock, snapshot o concurrencia se traducen a `409`.

## G. RG-024 — Reprogramar, cancelar e historial

Se implementaron servicio y API. Reprogramar conserva `ReservaId`, código, cliente, servicio, sede y snapshots; cambia fecha/hora y, cuando corresponde, profesional/recurso compatible. Reutiliza el servicio central de disponibilidad y excluye la propia reserva mediante `reservaIdExcluir`.

La operación registra antes/después e historial en la misma transacción y deja el estado `Reprogramada`; por ello el horario anterior se libera y el nuevo queda ocupado. Cancelar solo admite `Confirmada` o `Reprogramada`, registra motivo/observación/usuario, crea historial y cambia a `Cancelada` transaccionalmente. No realiza reembolso.

## H. Base de datos

Tablas principales: `Recursos`, `ServiciosRecurso`, `BloqueosRecurso`, horarios y excepciones de sede/profesional/recurso, `Reservas`, `ReservaParticipantes`, `HistorialReservas`, `ReprogramacionesReserva` y `CancelacionesReserva`.

Se agregaron configuraciones explícitas para relaciones, índices, soft delete, `DeleteBehavior.Restrict` y precisión decimal. Se eliminó la FK sombra `RecursoId1`. El snapshot quedó actualizado.

Migraciones visibles:

- `20260813031037_RG020_RecursosYBloqueos` (recibida de la rama remota; no editada).
- `20260813073412_AgregarHorariosYExcepciones` (recibida; no editada).
- `20260815141612_CorregirRG020RG024Integracion` (nueva migración correctiva).

`dotnet ef migrations has-pending-model-changes`: **No changes have been made to the model since the last migration.**

La migración correctiva elimina la FK sombra y ajusta longitudes; EF advierte posible pérdida al eliminar esa columna/estrechar textos. Debe revisarse el contenido real antes de aplicarla en una base compartida. No se ejecutó sobre la base del usuario.

## I. Aislamiento multiempresa

Se validan combinaciones entre organización, sede, servicio, cliente, participantes, profesional y recurso. Las asociaciones cruzadas producen resultados controlados de validación/conflicto/no encontrado, no una excepción 500. Las reservas exponen `OrganizacionId` en su DTO para que la API no entregue por ID/código una reserva de otra empresa.

## J. Concurrencia

Recursos críticos usan transacciones y reservas/bloqueos usan aislamiento `Serializable`. La creación y reprogramación vuelven a consultar conflictos dentro de la transacción. La protección está implementada para SQL Server y las pruebas de integración vuelven a ejecutarse correctamente después de reparar la cadena de migraciones RG-030. No se añadió en esta auditoría una prueba HTTP dedicada que dispare dos solicitudes exactamente simultáneas; esa mejora de cobertura no bloquea RG-025.

## K. API real

Todos requieren cookie autenticada y rol `Administrador` o `Superadministrador`; POST/PUT/PATCH/DELETE también requieren `X-XSRF-TOKEN`. Respuestas de negocio: 400 validación, 404 inexistente/oculto, 409 conflicto; creación devuelve 201, lectura 200, actualización/eliminación 204.

### RG-020

| Método | Ruta | Request/Response principal |
|---|---|---|
| GET | `/api/sedes/{sedeId}/recursos` | filtros → página de `RecursoListaDto` |
| GET | `/api/recursos/{id}` | `RecursoDetalleDto` |
| POST | `/api/sedes/{sedeId}/recursos` | `CrearRecursosRequest` → 201 detalle |
| PUT | `/api/recursos/{id}` | `ActualizarRecursosRequest` → 204 |
| PATCH | `/api/recursos/{id}/estado` | `CambiarEstadoRecursosRequest` → 204 |
| DELETE | `/api/recursos/{id}` | 204 |
| GET/PUT | `/api/recursos/{id}/servicios` | lista / `ReemplazarServiciosRecursosRequest` |
| GET/POST | `/api/recursos/{recursoId}/bloqueos` | lista / `CrearBloqueoRequest` |
| PUT/DELETE | `/api/bloqueos/{id}` | `ActualizarBloqueoRequest` / 204 |

### RG-021

Para cada propietario existen GET/PUT de horario y GET/POST de excepciones:

- `/api/sedes/{sedeId}/horarios` y `/api/sedes/{sedeId}/excepciones-horario`.
- `/api/profesionales/{profesionalId}/horarios`.
- `/api/profesionales/{profesionalId}/sedes/{sedeId}/horarios`.
- `/api/profesionales/{profesionalId}/excepciones-horario`.
- `/api/profesionales/{profesionalId}/sedes/{sedeId}/excepciones-horario`.
- `/api/recursos/{recursoId}/horarios` y `/api/recursos/{recursoId}/excepciones-horario`.
- PUT/DELETE global por ID en `/api/excepciones-horario-sede/{id}`, `/api/excepciones-horario-profesional/{id}` y `/api/excepciones-horario-recurso/{id}`.

El horario usa `ActualizarHorarioSemanalSolicitud`; excepciones usan los DTO `Crear/ActualizarExcepcion...Solicitud`. Lecturas devuelven `HorarioSemanalDto`, `PaginaResultado<ExcepcionHorarioDto>` o 204.

### RG-022

- GET `/api/disponibilidad`: `sedeId`, `servicioId`, `fechaDesde`, `fechaHasta`, `profesionalId?`, `recursoId?` → `DisponibilidadRespuestaDto`.
- GET `/api/disponibilidad/profesionales`: sede, servicio, fecha opcional → candidatos realmente libres.
- GET `/api/disponibilidad/recursos`: sede, servicio, fecha opcional → candidatos realmente libres.

### RG-023/RG-024

- POST `/api/organizaciones/{organizacionId}/reservas`: `CrearReservaSolicitud` → 201 `ReservaCreadaDto`.
- GET `/api/reservas/{id}` y GET `/api/reservas/codigo/{codigo}` → `ReservaDetalleDto`.
- GET `/api/organizaciones/{organizacionId}/reservas` → página filtrada.
- PUT `/api/organizaciones/{organizacionId}/reservas/{id}/reprogramacion`: `ReprogramarReservaSolicitud` → 200.
- POST `/api/organizaciones/{organizacionId}/reservas/{id}/cancelacion`: `CancelarReservaSolicitud` → 200.

## L. ProblemDetails

Se mapean especialmente: código de recurso duplicado, sede/recurso inválido, servicio no ofrecido, bloqueo solapado, rango inválido/excesivo, horario ocupado, capacidad excedida, estado de reserva no permitido y asociaciones de otra organización. Errores conocidos no deben escapar como 500.

## M. Archivos modificados

Hay 20 archivos versionados modificados y uno eliminado. Incluyen `Program.cs`, `appsettings.json`, contratos de repositorio, entidades ya existentes, `EstadoReserva`, `ApplicationDbContext`, configuraciones EF, snapshot, DI, repositorio de servicios, la corrección RG-030 y pruebas existentes. `BloqueoRecursos.cs` se elimina por ser duplicado.

El listado literal completo está en `CAMBIOS_RG020_RG024.txt`.

## N. Archivos creados

Se crearon 81 archivos de implementación/prueba, más este informe y su anexo: controllers/contratos; abstracciones, DTO, algoritmos, interfaces y servicios Application; entidades/enums; configuraciones, repositorios y migraciones Infrastructure; y `Rg020Rg024BusinessRulesTests.cs`. El detalle literal está en el anexo.

## O. Migraciones

Sí se creó una migración correctiva porque las migraciones originales ya estaban compartidas y el modelo final tenía tablas de reserva, relaciones e índices ausentes. No se editó `InitialCreate`. Después de autorizarse la reparación RG-030, se ajustó `20260811021227_AgregarPermisosYRolePermissions`: ya no vuelve a crear permisos, roles ni vínculos de usuario existentes y conserva únicamente la creación condicional de las tablas de empleados que le corresponden. También se retiraron esas creaciones duplicadas de la migración correctiva local RG-020–024. Así queda una sola estrategia coherente para una base nueva.

## P. Tests finales

- Unit: 163 total, 163 aprobadas, 0 fallidas.
- Integration: 45 total, 45 aprobadas, 0 fallidas.
- Total: 208; aprobadas 208; fallidas 0.
- La cadena completa de pruebas unitarias y de integración pasa en SQL Server.

## Q. Build

`dotnet build`: correcto, 0 advertencias, 0 errores. Todos los proyectos conservan `TargetFramework=net8.0`; no se migró a .NET 10.

## R. Flujo end-to-end

El flujo queda implementado en una sola cadena de reglas: horario → excepción/bloqueo → disponibilidad → reserva/revalidación → reprogramación (libera anterior, ocupa nuevo) → cancelación (libera nuevo). Sus primitivas y persistencia se verificaron con las pruebas unitarias y de integración. Una base SQL Server temporal recibió correctamente las nueve migraciones desde cero; la API inició contra ella, Swagger respondió HTTP 200 y publicó 55 rutas, incluidas disponibilidad, reprogramación y cancelación.

## S. Errores no corregidos

No quedan errores bloqueantes conocidos para comenzar RG-025. La duplicación de `Permissions`/`RolePermissions` de RG-030 fue corregida y comprobada creando una base limpia. Como mejora futura de cobertura puede añadirse una prueba HTTP específicamente sincronizada para dos reservas simultáneas; la protección transaccional y las pruebas actuales ya están aprobadas.

## T. Breaking changes

No se renombraron rutas ni propiedades existentes. Se agregaron endpoints RG-024 y `OrganizacionId` a `ReservaDetalleDto` de forma aditiva. Se eliminó solo la entidad interna duplicada `BloqueoRecursos`; no era un contrato HTTP.

## U. Git final

- Rama: `Errores/auditoria-rg020-rg024`.
- HEAD base: `922c042`.
- Cambios locales: implementación y correcciones aún sin confirmar; el listado exacto está en `CAMBIOS_RG020_RG024.txt`.
- `git diff --check`: sin errores de whitespace después de la limpieza final; solo avisos de normalización LF/CRLF de Git para Windows.
- No hay commit, push, PR ni merge.
- Ubicación de revisión: `artifacts/ReservaTuCita-Ya-RG020024-Audit` dentro del repositorio principal; la carpeta está ignorada por la rama UI para preservar su trabajo.
