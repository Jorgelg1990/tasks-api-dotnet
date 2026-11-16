# Tasks API (.NET 10)

API REST para gestionar tareas (CRUD) con SQLite, Entity Framework Core, migraciones, validaciones, filtros/paginación y tests automatizados.

## Requisitos

- .NET SDK 10.0+

## Cómo ejecutarlo

1. Restaurar dependencias y compilar:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Ejecutar la API:

   ```bash
   dotnet run --project src/TasksApi/TasksApi.csproj
   ```

   La API aplica las migraciones automáticamente al iniciar (`Database.Migrate()`), por lo que crea/actualiza el archivo `tasks.db` (SQLite) en el directorio de ejecución del proyecto.

3. Endpoints principales (`/api/tasks`):

   - `POST /api/tasks` — crear tarea
   - `GET /api/tasks` — listar tareas (con filtros y paginación)
   - `GET /api/tasks/{id}` — ver tarea por id
   - `PUT /api/tasks/{id}` — editar tarea
   - `DELETE /api/tasks/{id}` — borrar tarea

### Modelo de tarea

Campos: `titulo`, `descripcion`, `estado` (`"pendiente"`, `"en curso"` o `"hecha"`), `responsable`, `fechaLimite` (`YYYY-MM-DD`).

Ejemplo de creación:

```bash
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Preparar demo",
    "descripcion": "Armar endpoints y pruebas",
    "estado": "pendiente",
    "responsable": "Ana",
    "fechaLimite": "2026-12-31"
  }'
```

### Filtros y paginación

`GET /api/tasks?estado=pendiente&responsable=Ana&page=1&pageSize=10`

- `estado`: `pendiente`, `en curso` o `hecha`
- `responsable`: valor exacto
- `page`: >= 1 (default 1)
- `pageSize`: 1..100 (default 10)

Respuesta paginada: `items`, `page`, `pageSize`, `totalItems`, `totalPages`.

### Validaciones y códigos HTTP

- `201 Created` al crear, `200 OK` en consultas/actualizaciones, `204 No Content` al borrar.
- `400 Bad Request` para datos inválidos (DataAnnotations/`[ApiController]`) o parámetros de filtro/paginación inválidos.
- `404 Not Found` cuando la tarea no existe.

## Migraciones

La migración inicial está en `src/TasksApi/Data/Migrations/`.

Comandos útiles:

```bash
dotnet ef migrations add <Nombre> --project src/TasksApi/TasksApi.csproj --startup-project src/TasksApi/TasksApi.csproj -o Data/Migrations
dotnet ef database update --project src/TasksApi/TasksApi.csproj --startup-project src/TasksApi/TasksApi.csproj
```

## Tests

Ejecutar:

```bash
dotnet test
```

Incluye 4 tests de integración (WebApplicationFactory + SQLite en memoria) que cubren creación/consulta, filtros y paginación, validación (400) y actualización/borrado (404 al consultar eliminado).

## Decisiones que tomé

- **.NET 10 + ASP.NET Core Web API (controllers):** estructura clara para CRUD, validación con DataAnnotations y respuestas HTTP estándar.
- **SQLite + EF Core + migraciones:** cumple el requisito con una base liviana y portable; se incluye migración inicial y se aplican migraciones al iniciar para facilitar la ejecución.
- **Estados en español:** el dominio expone/acepta `pendiente`, `en curso`, `hecha` (enum con conversión JSON y conversión de valor en EF) alineado al enunciado.
- **DTOs separados:** desacoplan el contrato HTTP del modelo de persistencia y permiten controlar validaciones/campos requeridos.
- **Paginación y filtros explícitos:** parámetros `page/pageSize/estado/responsable` con validación y metadatos de paginación en la respuesta.
- **Tests de integración:** validan el comportamiento real de los endpoints, códigos HTTP y persistencia con una base SQLite en memoria aislada por fábrica.

## Qué haría con más tiempo

- Agregar autenticación/autorización y auditoría (quién creó/modificó).
- Ordenamiento configurable, búsqueda por texto y paginación con cursores para grandes volúmenes.
- Manejo de errores más uniforme (middleware de excepciones con `ProblemDetails` consistente), logging estructurado y observabilidad.
- Docker/compose, CI, y más cobertura de tests (casos límite de fechas, concurrencia, filtros combinados, etc.).
- Documentación OpenAPI enriquecida (ejemplos y esquemas detallados) y versionado de API.

## Cómo usé la IA

- **Qué le pedí:** implementar la solución completa solicitada (CRUD de tareas, filtros por estado/responsable con paginación, validaciones y códigos HTTP correctos, base SQLite con migraciones, al menos 3 tests y este README) usando .NET 10 en `C:\Users\User\TasksApi` con SQLite.
- **Qué acepté:** la estructura de solución (API + proyecto de tests), el modelado con EF Core/SQLite y migración inicial, los DTOs/controlador con validaciones y respuestas paginadas, los tests de integración y la aplicación automática de migraciones al iniciar.
- **Qué corregí:** ajusté las versiones de paquetes NuGet para que fueran compatibles con `net10.0` (evitando referencias 11.x incompatibles), hice los tests independientes entre sí (datos de filtro con identificadores propios) para evitar flakiness por estado compartido, y documenté que el espejo en Engram quedó pendiente por ambigüedad del contexto de proyecto en el entorno (el avance local y el documento `odd/tasks/tasks-api.md` sí se preservaron).
