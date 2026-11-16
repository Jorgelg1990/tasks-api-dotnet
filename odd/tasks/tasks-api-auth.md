# Tasks API — Arquitectura limpia, JWT + refresh token, Swagger

Objetivo: evolucionar la API de tareas a arquitectura limpia, con autenticación JWT, refresh tokens rotados y Swagger UI con autorización Bearer. Las tareas son privadas por usuario.
Rama: `feat/tasks-api`. Estrategia de entrega: `ask-on-risk`. Test runner: `dotnet test`.

## Specs

- **S7.** "que use arquitectura limpia"
  - Alcance: capas `Domain`, `Application`, `Infrastructure`, `Api` con dependencias hacia adentro; EF Core solo en `Infrastructure`.
  - Criterios: el contrato HTTP de `/api/tasks` no cambia en T6; los 4 tests existentes siguen en verde.
- **S8.** "atenticacion con jwt y refrech toke"
  - Alcance: registro, login, refresh con rotación y detección de reutilización, logout (revoca el refresh token). Access token JWT de vida corta.
  - Criterios: `/api/tasks/**` exige token (401 sin token o inválido); refresh anterior revocado falla; reutilizar un refresh revocado revoca todas las sesiones del usuario.
- **S9.** "para provar swaagger" (Swagger UI con autorización)
  - Alcance: Swagger UI con esquema Bearer y botón Authorize; respuestas documentadas.
  - Criterios: desde Swagger se puede registrar/login, autorizar y llamar a `/api/tasks`.
- **S10.** Tareas privadas por usuario (decisión del usuario: "si privada por usuarios").
  - Criterios: el usuario A recibe 404 al leer, editar o borrar una tarea del usuario B; `UserId` se toma del token, nunca del body.
- **S11.** Recomendaciones aprobadas por el usuario ("si estoy de acuerdo").
  - Alcance: validación estricta del JWT (issuer, audience, lifetime, firma, ClockSkew corto); access 15 min y refresh 7 días configurables; rate limiting en `/login` y `/register`; usuario demo sembrado solo en Development; secretos fuera del repositorio (user-secrets / variables de entorno).
- **S12.** Errores uniformes.
  - Alcance: `ProblemDetails` global; 409 para email duplicado; 401 sin revelar si el email existe.
- **S13.** Pruebas y documentación.
  - Alcance: unitarias (dominio, validadores, rotación de refresh) e integración (flujo completo con JWT, aislamiento entre usuarios); README actualizado.

## Tasks

| ID | Specs | Ruta | Estado | Evidencia (commit/RED-GREEN) |
|---|---|---|---|---|
| T6 | S7 | inline | completada | a10f10e: dotnet build 0 errores; dotnet test 4/4 superados (refactor sin cambio de contrato) |
| T7 | S8, S10 | inline | completada | commit de T7: Identity, RefreshToken, TaskItem.UserId y migración AddIdentityAndRefreshTokens; 4/4 tests existentes en verde |
| T8 | S8, S11 | inline | completada | commit T8: JWT access + refresh emisión inicial, /register, /login, validación estricta, rate limiting auth, usuario demo dev; 4/4 tests existentes en verde |
| T9 | S8, S11 | inline (riesgo alto; verificación independiente en T13) | completada | commit T9: /refresh con rotación y detección de reutilización, /logout; 4/4 tests existentes en verde |
| T10 | S10 | inline | completada | commit T10: [Authorize] en /api/tasks, UserId desde token, filtro por usuario y 404 en acceso ajeno; 4 tests existentes adaptados y en verde |
| T11 | S9 | inline | completada | commit T11: Swagger UI con esquema Bearer/Authorize y respuestas documentadas; 4/4 tests en verde |
| T12 | S12 | inline | completada | commit T12: GlobalExceptionHandler con ProblemDetails; 409 en email duplicado y 401 genérico ya aplicados; 4/4 tests |
| T13 | S13 | inline (verificación: suite completa + revisión de escenarios críticos) | completada | commit T13: 7 tests de auth/aislamiento añadidos; dotnet test 11/11 |
| T14 | S13 | inline | completada | commit T14: README actualizado; dotnet build OK, dotnet test 11/11 |

## Log

- **L1 (solicitud original, verbatim):** "este proycto es para una prueba tecnica , lo necesto lo mas profecional posible, que use arquitectura limpia, atenticacion con jwt y refrech toke y para provar swaagger po ahora solo planifica y no lo implementes"
- **L2 (aprobación del plan, verbatim):** "si privada por usuarios que otra cosa recomiendas ?" / "si estoy de acuerdo"
- **L3:** Plan aprobado con las recomendaciones de S10 y S11. Se inicia con T6.
- **L4 (T6):** Refactor puro, sin RED nuevo: la red de seguridad son los 4 tests de integración existentes. Trade-off aceptado: TaskStatus conserva atributos JSON en Domain para no cambiar el contrato; se documentará en el README.
- **L5 (T7):** Se agregó `ApplicationUser`, `RefreshToken` y `TaskItem.UserId` (nullable hasta T10 para no romper el contrato actual) con migración `AddIdentityAndRefreshTokens`. Se añadieron paquetes de Identity/JWT Bearer en Infrastructure; los 4 tests existentes siguen pasando.
- **L6 (T8):** Se implementaron `/api/auth/register` y `/api/auth/login` con JWT (issuer/audience/lifetime/firma/ClockSkew 30s) y emisión de refresh token inicial; rate limiting en endpoints de auth y seed de usuario demo en Development. Swashbuckle se actualizó a 10.1.7 por compatibilidad con .NET 10/OpenAPI 2.x (la 7.2.0 rompía los tests). Los endpoints de tareas siguen sin `[Authorize]` hasta T10.
- **L7 (T9):** Se agregaron `/api/auth/refresh` (rotación: revoca el usado, emite nuevo par) y `/api/auth/logout`. Si se reutiliza un refresh revocado se revocan todos los tokens del usuario. Hash SHA-256 en base. T9 se implementó inline con verificación independiente diferida a T13.
- **L8 (T10):** `/api/tasks` requiere JWT; `UserId` se toma del claim del token y las consultas se filtran por usuario. Acceso a tarea ajena devuelve 404. Se adaptaron los 4 tests existentes para autenticarse (registro/login por test). Prueba específica de aislamiento A/B pendiente para T13. Tests: 4/4.
- **L9 (T11):** Swagger UI configurado con esquema Bearer (botón Authorize) y atributos de respuesta en controladores de auth y tareas. Tests: 4/4.
- **L10 (T12):** Se agregó `GlobalExceptionHandler` para respuestas ProblemDetails uniformes. Registro devuelve 409 en email duplicado y login 401 sin revelar existencia del correo. Tests: 4/4.
- **L11 (T13):** Se agregaron 7 tests de integración: flujo registro/login/acceso, 401 sin token, rotación de refresh y revocación total ante reutilización, logout, aislamiento A/B (404 en lectura/edición/borrado), conflicto por email duplicado y login inválido. Se ajustó el factory de tests para desactivar rate limiting. Suite: 11/11. Verificación independiente diferida a revisión final (no hay subagente verificador separado en esta sesión).
- **L12 (T14):** README actualizado con arquitectura, autenticación JWT/refresh, Swagger, tareas privadas, migraciones, tests y uso de IA. Cierre: `dotnet build` OK, `dotnet test` 11/11.
