# Tasks API (.NET 10)

REST API for personal task management. Built with ASP.NET Core on .NET 10, with Clean Architecture, JWT authentication and refresh token rotation, SQLite persistence managed by EF Core migrations, input validation, filtering, pagination, integration tests, and Swagger UI.

## Features

- Create, list, get, update, and delete tasks.
- Each task has a title, description, status, assignee (`responsable`), and due date (`fechaLimite`).
- Status values: `pendiente`, `en curso`, `hecha`.
- Filter by status and assignee, with pagination.
- Tasks are private: each user only sees and manages their own tasks.
- Consistent error responses using ProblemDetails.

## Architecture

The solution follows Clean Architecture, with dependencies pointing inward:

- `src/TasksApi.Domain`: entities and enums (`TaskItem`, `ApplicationUser`, `RefreshToken`, `TaskStatus`), with no infrastructure dependencies.
- `src/TasksApi.Application`: use cases, DTOs, service results, and abstractions (`ITaskRepository`, `IUnitOfWork`, `ITokenService`, `IRefreshTokenStore`).
- `src/TasksApi.Infrastructure`: EF Core with SQLite (`AppDbContext`, migrations), ASP.NET Core Identity, JWT token service, refresh token store, repositories, and global exception handling.
- `src/TasksApi`: API layer with controllers, middleware pipeline, Swagger, and the composition root (`Program.cs`).
- `tests/TasksApi.Tests`: integration tests using `WebApplicationFactory` and in-memory SQLite.

## Requirements

- .NET SDK 10.0 or later.

## Getting started

1. Restore and build:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Run the API:

   ```bash
   dotnet run --project src/TasksApi/TasksApi.csproj
   ```

   On startup the app applies EF Core migrations automatically (`Database.Migrate()`), creating or updating `tasks.db` (SQLite) in the API execution directory.

3. Open Swagger UI in Development at `https://localhost:<port>/swagger` (or the HTTP URL shown by `dotnet run`).

## Authentication

- `POST /api/auth/register`: register a user (201). Returns an access token and a refresh token.
- `POST /api/auth/login`: log in (200). Returns an access token and a refresh token.
- `POST /api/auth/refresh`: rotate the refresh token (200). The used token is revoked and a new pair is issued.
- `POST /api/auth/logout`: revoke a refresh token (204).

Security behavior:

- Access tokens are short-lived JWTs (15 minutes by default), validated for issuer, audience, lifetime, signature, and with a short clock skew.
- Refresh tokens are opaque, stored hashed, rotated on every use, and revoked on logout. Reusing a revoked refresh token revokes all refresh tokens for that user.
- `/login` and `/register` are rate-limited.
- Invalid credentials return a generic 401, so the response does not reveal whether an email exists. Duplicate registration returns 409.
- In Development only, a demo user is seeded: `demo@tasks.local` / `Demo123!`.

JWT settings are read from the `Jwt` section. The Development file includes a local key for convenience. Other environments must set `Jwt:Key` through user secrets or environment variables.

### Using Swagger

1. Call `POST /api/auth/login` (or `/api/auth/register`) and copy the `accessToken`.
2. Click **Authorize**, enter `Bearer <accessToken>`, and confirm.
3. Call the `/api/tasks` endpoints.

## Tasks API

All `/api/tasks` endpoints require a Bearer token.

- `POST /api/tasks`: create a task.
- `GET /api/tasks`: list tasks, with filters and pagination.
- `GET /api/tasks/{id}`: get a task by id.
- `PUT /api/tasks/{id}`: update a task.
- `DELETE /api/tasks/{id}`: delete a task.

Task fields: `titulo`, `descripcion`, `estado` (`"pendiente"`, `"en curso"`, `"hecha"`), `responsable`, and `fechaLimite` (`YYYY-MM-DD`). The owner is taken from the JWT, never from the request body.

### Filters and pagination

`GET /api/tasks?estado=pendiente&responsable=Ana&page=1&pageSize=10`

- `estado`: `pendiente`, `en curso`, or `hecha`.
- `responsable`: exact match.
- `page`: 1 or greater (default 1).
- `pageSize`: between 1 and 100 (default 10).

Paginated response: `items`, `page`, `pageSize`, `totalItems`, `totalPages`.

### HTTP status codes

- `201 Created`: create and register.
- `200 OK`: reads, updates, login, and refresh.
- `204 No Content`: delete and logout.
- `400 Bad Request`: invalid input or query parameters (ProblemDetails).
- `401 Unauthorized`: missing or invalid credentials or token.
- `404 Not Found`: the task does not exist for the authenticated user.
- `409 Conflict`: duplicate email on registration.
- Unhandled exceptions return ProblemDetails through the global exception handler.

## Migrations

Migrations live in `src/TasksApi.Infrastructure/Persistence/Migrations/`.

```bash
dotnet ef migrations add <Name> --project src/TasksApi.Infrastructure/TasksApi.Infrastructure.csproj --startup-project src/TasksApi/TasksApi.csproj -o Persistence/Migrations
dotnet ef database update --project src/TasksApi.Infrastructure/TasksApi.Infrastructure.csproj --startup-project src/TasksApi/TasksApi.csproj
```

## Tests

```bash
dotnet test
```

The suite has 11 integration tests (`WebApplicationFactory` with in-memory SQLite) covering:

- Task create, get, update, delete, filters, pagination, validation (400), and not-found (404).
- Authentication: register, login, protected endpoint access (401 without a token), refresh token rotation, reuse detection, and logout.
- Per-user isolation: one user cannot read, update, or delete another user's task (404).
- Duplicate registration (409) and invalid login (401).

## Design decisions

- **Clean Architecture:** Domain, Application, Infrastructure, and API are separated so business rules do not depend on EF Core or HTTP.
- **SQLite with EF Core migrations:** lightweight and portable. Migrations are applied on startup for simple local execution.
- **ASP.NET Core Identity with JWT:** Identity handles user storage and password hashing. JWTs are stateless; refresh tokens are hashed, rotated, and protected by reuse detection.
- **Per-user ownership:** `UserId` comes from the authenticated token, and every task query is scoped to the owner. Access to another user's task returns 404 so existence is not leaked.
- **Swagger with Bearer auth:** endpoints can be tested directly from the UI.
- **ProblemDetails errors:** consistent error responses across the API.
- **Integration-first tests:** they exercise real endpoints, authentication, and persistence against an isolated in-memory SQLite database per test host.

## What I would do with more time

- Docker/Compose and a CI pipeline.
- Structured logging, tracing, metrics, and health checks.
- Access token revocation and a password recovery flow.
- Cursor-based pagination and richer sorting and search.
- Unit tests for edge cases such as token expiry boundaries and concurrency, and stronger secret management across environments.
- OpenAPI examples and API versioning.

## How I used AI

- **What I asked for:** help planning and implementing the API: clean architecture, JWT authentication with refresh tokens, Swagger for manual testing, per-user task privacy, and tests and documentation.
- **What I accepted:** the layered structure, the Identity/JWT/refresh token design, per-user task scoping, the Swagger Bearer configuration, global ProblemDetails handling, and the integration test coverage for auth and isolation.
- **What I corrected:** kept the task HTTP contract stable during the refactor and checked it against the existing tests before changing behavior; adapted the tests to authenticate once `/api/tasks` was protected; upgraded Swashbuckle to a version compatible with .NET 10 and OpenAPI after an incompatibility with 7.2.0; and adjusted the test host so JWT keys and rate limiting behave correctly in integration tests.
