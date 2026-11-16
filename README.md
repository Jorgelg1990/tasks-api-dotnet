# Tasks API (.NET 10)

REST API for task management with Clean Architecture, JWT authentication, refresh token rotation, SQLite persistence, EF Core migrations, validation, filtering, pagination, automated tests, and Swagger UI.

## Architecture

The solution is organized into layers with dependencies pointing inward:

- `src/TasksApi.Domain`: entities and enums (`TaskItem`, `ApplicationUser`, `RefreshToken`, `TaskStatus`) with no infrastructure dependencies.
- `src/TasksApi.Application`: use cases, DTOs, validation-oriented results, and abstractions (`ITaskRepository`, `IUnitOfWork`, `ITokenService`, `IRefreshTokenStore`).
- `src/TasksApi.Infrastructure`: EF Core/SQLite (`AppDbContext`, migrations), Identity, JWT token service, refresh token store, repositories, and global exception handling.
- `src/TasksApi` (API): controllers, middleware pipeline, Swagger, and composition root (`Program.cs`).
- `tests/TasksApi.Tests`: integration tests with `WebApplicationFactory` and in-memory SQLite.

## Requirements

- .NET SDK 10.0+

## How to run

1. Restore and build:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Run the API:

   ```bash
   dotnet run --project src/TasksApi/TasksApi.csproj
   ```

   The app applies EF Core migrations automatically on startup (`Database.Migrate()`), creating/updating `tasks.db` (SQLite) in the API project execution directory.

3. Open Swagger UI in Development:

   - `https://localhost:<port>/swagger` (or the HTTP URL shown by `dotnet run`)

### Authentication (JWT + refresh token)

- `POST /api/auth/register` — register a user (201). Returns access token and refresh token.
- `POST /api/auth/login` — login (200). Returns access token and refresh token.
- `POST /api/auth/refresh` — rotate refresh token (200). The used refresh token is revoked and a new pair is issued.
- `POST /api/auth/logout` — revoke refresh token (204).

Security behavior:
- Access tokens are short-lived JWTs (default 15 minutes) with strict validation (issuer, audience, lifetime, signature, short clock skew).
- Refresh tokens are opaque, stored hashed in the database, rotated on use, and revoked on logout. Reusing a revoked refresh token revokes all refresh tokens for that user.
- `/login` and `/register` are rate-limited.
- Login returns a generic 401 for invalid credentials and does not reveal whether the email exists. Duplicate registration returns 409.
- In Development, a demo user is seeded for quick testing: `demo@tasks.local` / `Demo123!`.

JWT configuration is read from `Jwt` settings. In Development, a local key is provided in `appsettings.Development.json` for convenience; for real environments configure `Jwt:Key` via user-secrets or environment variables (never commit production secrets).

### Testing with Swagger

1. Call `POST /api/auth/login` (or `/register`) and copy the `accessToken`.
2. Click **Authorize** in Swagger UI, enter `Bearer <accessToken>`, and confirm.
3. Call `/api/tasks` endpoints as an authenticated user.

### Tasks (private per user)

All `/api/tasks` endpoints require a valid Bearer token. Tasks are private: each user can only access their own tasks. Accessing another user's task returns 404.

- `POST /api/tasks` — create task
- `GET /api/tasks` — list tasks (filters and pagination)
- `GET /api/tasks/{id}` — get task by id
- `PUT /api/tasks/{id}` — update task
- `DELETE /api/tasks/{id}` — delete task

Task fields: `titulo`, `descripcion`, `estado` (`"pendiente"`, `"en curso"`, `"hecha"`), `responsable`, `fechaLimite` (`YYYY-MM-DD`). The owner (`UserId`) is taken from the JWT, never from the request body.

### Filters and pagination

`GET /api/tasks?estado=pendiente&responsable=Ana&page=1&pageSize=10`

- `estado`: `pendiente`, `en curso`, or `hecha`
- `responsable`: exact value
- `page`: >= 1 (default 1)
- `pageSize`: 1..100 (default 10)

Paginated response: `items`, `page`, `pageSize`, `totalItems`, `totalPages`.

### HTTP status codes and errors

- `201 Created` on create/register, `200 OK` on reads/updates/login/refresh, `204 No Content` on delete/logout.
- `400 Bad Request` for invalid input or invalid query parameters (ProblemDetails).
- `401 Unauthorized` for missing/invalid credentials or tokens.
- `404 Not Found` when a task does not exist for the authenticated user.
- `409 Conflict` for duplicate email registration.
- Unhandled exceptions are mapped to ProblemDetails by a global exception handler.

## Migrations

Migrations are in `src/TasksApi.Infrastructure/Persistence/Migrations/`.

Useful commands:

```bash
dotnet ef migrations add <Name> --project src/TasksApi.Infrastructure/TasksApi.Infrastructure.csproj --startup-project src/TasksApi/TasksApi.csproj -o Persistence/Migrations
dotnet ef database update --project src/TasksApi.Infrastructure/TasksApi.Infrastructure.csproj --startup-project src/TasksApi/TasksApi.csproj
```

## Tests

Run:

```bash
dotnet test
```

The suite includes 11 integration tests (`WebApplicationFactory` + in-memory SQLite) covering:
- Task CRUD, filters, pagination, validation (400), and not-found behavior.
- Authentication flow (register/login), protected endpoint access (401 without token), refresh token rotation, reuse detection (revoked token reuse revokes all sessions), and logout.
- Per-user task isolation (user A cannot read/edit/delete user B's task; receives 404).
- Duplicate registration conflict (409) and invalid login credentials (401).

## Decisions

- **Clean Architecture:** separated Domain/Application/Infrastructure/API to keep business rules independent from EF Core and HTTP concerns, with abstractions implemented in Infrastructure.
- **SQLite + EF Core + migrations:** lightweight and portable; migrations are applied on startup for easy local execution.
- **ASP.NET Core Identity + JWT:** Identity handles user storage and password hashing; JWT provides stateless access tokens and refresh tokens are stored hashed with rotation and reuse detection.
- **Per-user task ownership:** `UserId` is derived from the authenticated token and all task queries are scoped by owner; foreign-task access returns 404 to avoid leaking existence.
- **Swagger (Swashbuckle):** configured with Bearer security so the evaluator can authorize and test endpoints from the UI.
- **ProblemDetails-based errors:** consistent error responses, including global exception handling.
- **Integration-first tests:** validate real endpoint behavior, auth, and persistence against an isolated in-memory SQLite database per factory.

## What I would do with more time

- Docker/compose and CI pipeline.
- Structured logging, tracing/metrics, and health checks.
- Access token revocation/blacklisting strategy and password recovery flow.
- Cursor-based pagination and richer sorting/search.
- More unit tests around edge cases (token expiry boundaries, concurrency) and stronger secret management for all environments.
- OpenAPI examples and API versioning.

## How I used AI

- **What I asked for:** plan and implement a professional evolution of the existing Tasks API for a technical assessment: clean architecture, JWT authentication with refresh tokens, Swagger for testing, private tasks per user, plus tests and updated documentation.
- **What I accepted:** the layered project structure, Identity/JWT/refresh-token design, per-user task scoping, Swagger Bearer configuration, global ProblemDetails handling, and integration-test coverage for auth and isolation flows.
- **What I corrected:** kept the task HTTP contract stable during the initial refactor and verified with existing tests before changing behavior; adapted tests to authenticate after protecting `/api/tasks`; fixed a Swashbuckle 7.2.0 incompatibility with .NET 10/OpenAPI by upgrading to a compatible Swashbuckle version; adjusted the test host configuration so JWT signing/validation keys and rate limiting behaved correctly in integration tests; and recorded progress in `odd/tasks/tasks-api-auth.md` (Engram mirror saved for the feature state).
