# Tasks API (.NET 10)

Objetivo: Implementar una API de tareas en .NET 10 con SQLite, migraciones, validaciones, filtros, paginación, tests y README.
Rama: `feat/tasks-api`. Estrategia de entrega: `ask-on-risk`. Test runner: `dotnet test`.

## Specs

- **S1.** "1. Crear, listar, ver, editar y borrar tareas. Cada tarea tiene título, descripción, estado (pendiente, en curso o hecha), responsable y fecha límite."
  - Alcance: CRUD completo de tareas con campos título, descripción, estado (`pendiente`, `en curso`, `hecha`), responsable y fecha límite.
  - Criterios: crear, listar, ver (detalle), editar y borrar disponibles vía HTTP.
- **S2.** "2. Filtros por estado y por responsable, con paginación."
  - Alcance: listado con filtros por estado y responsable, y paginación.
  - Criterios: parámetros de filtro y paginación funcionan y se validan razonablemente.
- **S3.** "3. Validación de los datos y errores con los códigos HTTP correctos."
  - Alcance: validación de entrada y respuestas de error con códigos HTTP apropiados (400, 404, etc.).
- **S4.** "4. Base de datos SQL Server, PostgreSQL o SQLite con migraciones."
  - Alcance: usar SQLite con Entity Framework Core y migraciones.
  - Criterios: esquema gestionado por migraciones; base persistente local.
- **S5.** "5. Al menos 3 tests."
  - Alcance: mínimo 3 tests automatizados ejecutables con el test runner.
- **S6.** "6. Un README con cómo ejecutarlo, las decisiones que tomaste, qué harías con más tiempo y cómo usaste la IA (qué le pediste, qué aceptaste y qué corregiste)."
  - Alcance: README completo con ejecución, decisiones, mejoras futuras y uso de IA (qué se pidió, qué se aceptó y qué se corrigió).

## Tasks

| ID | Specs | Ruta | Estado | Evidencia (commit/RED-GREEN) |
|---|---|---|---|---|
| T1 | S4 | inline | completada | `57f81eb` — `dotnet build`: OK; migración `InitialCreate` creada |
| T2 | S1,S3 | inline | completada | `57f81eb` — CRUD + validaciones/HTTP en `TasksController` |
| T3 | S2,S3 | inline | completada | `57f81eb` — filtros `estado`/`responsable` y paginación en `GET /api/tasks` |
| T4 | S5 | inline | completada | `c6f4cf5` — `dotnet test`: 4/4 superados |
| T5 | S6 | inline | completada | `57f81eb` (README) + commit de documentación final (este cambio) |

## Log

- **L1 (solicitud original, verbatim):**
  1. Crear, listar, ver, editar y borrar tareas. Cada tarea tiene título, descripción, estado (pendiente, en curso o hecha), responsable y fecha límite.
  2. Filtros por estado y por responsable, con paginación.
  3. Validación de los datos y errores con los códigos HTTP correctos.
  4. Base de datos SQL Server, PostgreSQL o SQLite con migraciones.
  5. Al menos 3 tests.
  6. Un README con cómo ejecutarlo, las decisiones que tomaste, qué harías con más tiempo y cómo usaste la IA (qué le pediste, qué aceptaste y qué corregiste).
- **L2 (aclaraciones usuario):** ".net 10" / "si estoy de acuerdo" (acepta carpeta `C:\Users\User\TasksApi` y SQLite por defecto).
- **L3:** Implementación completada en rama `feat/tasks-api`. `dotnet build` OK (advertencias de vulnerabilidades en dependencias transitivas/paquetes); `dotnet test`: 4 superados, 0 fallidos.
- **L4:** Espejo Engram `odd/tasks-api/tasks` pendiente por contexto ambiguo del servidor MCP (no aceptó `project=tasksapi` con token de recuperación); avance local preservado en `odd/tasks/tasks-api.md`.
