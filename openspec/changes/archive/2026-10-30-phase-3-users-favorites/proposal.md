# Proposal

## Why

OpenWave can browse, search and play real stations, but nothing is remembered: there are no users and `Favoritos` is still a placeholder. This phase adds accounts and per-user favourite stations stored in the remote MySQL database, so a user can log in on any run of the app and find the same favourites, which is the last core feature of the course project.

## What Changes

- **Database**: a new `database/schema.sql`, run manually by the developer in MySQL Workbench against the existing cloud database `2DAM_alumno19` (provided by the teacher; the account cannot create databases). It selects that database and creates `users` and `favorite_stations` (utf8mb4), and ends with an optional, commented-out block that creates a restricted `openwave_app` user with only SELECT, INSERT, UPDATE and DELETE rights. The API never runs DDL: no EF migrations and no `EnsureCreated`.
- **API**:
  - EF Core with Oracle's `MySql.EntityFrameworkCore` provider (EF Core 10; Pomelo has no EF Core 10 release). Entities in `Data/Entities`, a `DbContext` with fluent mapping to the snake_case tables, repositories, and the connection string read from `ConnectionStrings:Default`. The real credentials live only in `dotnet user-secrets`; the repository only contains placeholders.
  - `POST /api/auth/register` and `POST /api/auth/login`, with passwords hashed by `PasswordHasher<T>`, emails trimmed and lower-cased, and Spanish ProblemDetails for 400, 401 and 409.
  - `GET`, `POST` and `DELETE` under `/api/users/{userId}/favorites`, storing a copy of the station data (including language) so favourites survive catalogue changes, with 404, 409 and 204 handling.
  - A database-unreachable error becomes 503 with `No se pudo acceder a la base de datos.`
- **Contracts**: new `RegisterRequest`, `LoginRequest` and `UserDto`; favourites reuse `StationDto`.
- **App**:
  - An in-memory `SessionService`, `Iniciar sesión` and `Crear cuenta` pages (registering logs the user in), and a visible `Cerrar sesión` action with the logged-in email.
  - A `FavoritesService` that keeps the set of favourite station ids in memory while logged in.
  - A heart toggle on every station in `Inicio` and in `Reproductor`. When logged out it opens `Iniciar sesión` and, after a successful login, adds the station and returns to the previous screen.
  - `Favoritos` becomes a real list with play and remove, loading, error with `Reintentar`, and empty states. When logged out it shows an invitation to log in instead of redirecting.
- **Project config**: fix `openspec/config.yaml` so OpenSpec can parse it (the project text moves under `context: |`), and update its database name to `2DAM_alumno19`.
- **README**: database setup, user-secrets, and how to run.

Out of scope: JWT or tokens, remembering the session after closing the app, password reset, preferences, theming settings, mini-player and statistics. As already accepted for this project, the favourites endpoints are not authorised; the session is only held in the app.

## Capabilities

### New Capabilities
- `user-accounts`: API registration and login. Unique, normalised emails, hashed passwords, input validation and the Spanish error responses, with users persisted in MySQL.
- `favorite-stations`: API storage of favourite stations per user. Listing newest first, adding without duplicates, removing, persistence in MySQL, and the database-unavailable response.
- `user-session`: the app's in-memory session. The `Iniciar sesión` and `Crear cuenta` pages, logging out, and where logged-out users are sent.
- `favorites-browsing`: the app's favourites experience. The heart toggle in `Inicio` and `Reproductor`, the `Favoritos` list with its states, playing and removing favourites, and keeping the heart state consistent across screens.

### Modified Capabilities
- `app-navigation`: the `Favoritos placeholder` requirement is removed; `Favoritos` is now defined by `favorites-browsing`.
- `api-connectivity`: a new requirement states that account and favourite data go only through the OpenWave API and that the app never connects to MySQL. The existing requirements are unchanged.

## Impact

- **New files**: `database/schema.sql`; API `Data/` (DbContext and entities), `Repositories/`, auth and favourites services and controllers; Contracts DTOs; app `SessionService`, `FavoritesService`, `IAuthService`/`AuthService`, login, register and favourites view models and pages.
- **Changed files**:
  - API: `Program.cs`, `OpenWave.Api.csproj` (EF Core MySQL provider package, `UserSecretsId`), `appsettings.json` (placeholder connection string), `OpenWave.Api.http`.
  - App: `MauiProgram.cs`, `AppShell` (login and register routes), `InicioPage`/`InicioViewModel`, `ReproductorPage`/`ReproductorViewModel`, `FavoritosPage`/`FavoritosViewModel`, `StationListItem`, `Resources/Styles`.
  - Other: `README.md`, `openspec/config.yaml`.
- **New dependencies**: `MySql.EntityFrameworkCore` (10.0.x), added unpinned with `dotnet add package`. `PasswordHasher<T>` already ships with the ASP.NET Core shared framework, so it needs no package.
- **External system**: the teacher-provided cloud MySQL database `2DAM_alumno19`. The developer runs the schema script, and the API connects with credentials stored only in user-secrets.
- **Known limitation**: any client that knows a user id can read or change that user's favourites, because there are no tokens.
