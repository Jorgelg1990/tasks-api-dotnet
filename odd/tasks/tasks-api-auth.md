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
| T8 | S8, S11 | inline | pendiente | — |
| T9 | S8, S11 | delegado (riesgo alto: seguridad) | pendiente | — |
| T10 | S10 | inline | pendiente | — |
| T11 | S9 | inline | pendiente | — |
| T12 | S12 | inline | pendiente | — |
| T13 | S13 | inline + verificador independiente (riesgo alto) | pendiente | — |
| T14 | S13 | inline | pendiente | — |

## Log

- **L1 (solicitud original, verbatim):** "este proycto es para una prueba tecnica , lo necesto lo mas profecional posible, que use arquitectura limpia, atenticacion con jwt y refrech toke y para provar swaagger po ahora solo planifica y no lo implementes"
- **L2 (aprobación del plan, verbatim):** "si privada por usuarios que otra cosa recomiendas ?" / "si estoy de acuerdo"
- **L3:** Plan aprobado con las recomendaciones de S10 y S11. Se inicia con T6.
- **L4 (T6):** Refactor puro, sin RED nuevo: la red de seguridad son los 4 tests de integración existentes. Trade-off aceptado: TaskStatus conserva atributos JSON en Domain para no cambiar el contrato; se documentará en el README.
- **L5 (T7):** Se agregó `ApplicationUser`, `RefreshToken` y `TaskItem.UserId` (nullable hasta T10 para no romper el contrato actual) con migración `AddIdentityAndRefreshTokens`. Se añadieron paquetes de Identity/JWT Bearer en Infrastructure; los 4 tests existentes siguen pasando.
