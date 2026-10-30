# Design

## Context

See proposal.md (Why) for motivation and the specs for the required behaviour. Current state that shapes the approach:

- **API:** controllers call services, which call the Radio Browser client. There is no database code yet. The existing controller translates a service exception into ProblemDetails (`StationsController` returns 502). `appsettings.Development.json` is gitignored and the project has no `UserSecretsId`.
- **App:** MVVM with singleton view models and pages, a Shell `TabBar` with `inicio`, `reproductor` and `favoritos`, and a typed `HttpClient` for stations with a 10-second timeout. `StationListItem` already wraps each station with an observable `IsSelected`. `PlaybackService` is the single owner of the selected station, and playback is tied to `Reproductor` (leaving that page stops audio). `FavoritosPage` is a static placeholder.
- **Database:** the teacher provided an existing cloud MySQL database named `2DAM_alumno19`. The server version and host are unknown, and the account most likely cannot create databases or users. The developer runs SQL in MySQL Workbench. Credentials will be given to the agent once, after group 1, and must be stored only in `dotnet user-secrets`.
- **Provider:** Pomelo's newest release (9.0.0) supports EF Core 9 only. Oracle's `MySql.EntityFrameworkCore` 10.0.x targets `net10.0` and EF Core 10. The developer chose Oracle's provider.

## Goals / Non-Goals

**Goals:**
- Keep the existing layering: Controllers (HTTP only), then Services (validation and rules), then Repositories (EF Core), with entities never crossing the HTTP boundary.
- The database schema is owned by `database/schema.sql`. The API only reads and writes rows.
- One in-memory source of truth in the app for the session and one for the favourite ids, so every heart and list agrees.
- Every failure path ends in a Spanish message and never in a crash.

**Non-Goals:**
- Authorisation of the favourites endpoints, tokens, or a persisted session.
- Offline caching of favourites.
- Changing the page-scoped playback from phase 2.
- Refactoring the existing station HTTP client.

## Decisions

### 1. Schema script for a pre-existing, shared database

`database/schema.sql` starts with `USE \`2DAM_alumno19\`;` instead of `CREATE DATABASE`, because the database already exists and a student account typically lacks the CREATE privilege (MySQL checks it even with `IF NOT EXISTS`). It then creates:

- `users`: `id`, `email`, `password_hash`, and `created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)`. The `email` column is UNIQUE.
- `favorite_stations`: the columns requested, plus `language VARCHAR(100) NULL` (the developer's choice). It has `created_at DATETIME(6) NOT NULL` set by the API in UTC, `UNIQUE (user_id, station_uuid)`, and a foreign key to `users(id)` with `ON DELETE CASCADE`.

Both tables use `CREATE TABLE IF NOT EXISTS`, `ENGINE=InnoDB` and `DEFAULT CHARSET=utf8mb4` with no explicit collation. The server's default utf8mb4 collation is case-insensitive on both MySQL 5.7 and 8.x, and naming `utf8mb4_0900_ai_ci` would fail on 5.7.

The script ends with a commented-out block that creates `openwave_app` with SELECT, INSERT, UPDATE and DELETE on `2DAM_alumno19.*`, using a password placeholder. A header comment explains that it may be rejected on a shared server, and that the teacher's account is then used instead.

Alternatives: `CREATE DATABASE IF NOT EXISTS openwave` was rejected because it would fail or target the wrong database. EF migrations and `EnsureCreated` are forbidden by the project rules.

### 2. EF Core with Oracle's provider, no schema management

The API adds `MySql.EntityFrameworkCore` unpinned. `OpenWaveDbContext` is registered with `AddDbContext` and `UseMySQL(connectionString)` from `ConnectionStrings:Default`, and contains `Data/Entities/UserEntity` and `Data/Entities/FavoriteStationEntity`.

`OnModelCreating` maps both entities to the snake_case tables and columns, the unique indexes and the relationship, so EF matches the script. Nothing calls `Database.Migrate`, `EnsureCreated`, or any raw DDL. Sensitive-data logging stays off, so parameter values (password hashes, emails) never reach the logs.

### 3. Credentials only in user-secrets

- `dotnet user-secrets init` adds a `UserSecretsId` to `OpenWave.Api.csproj`; this id is not a secret.
- `appsettings.json` gets a placeholder value, for example `Server=<HOST>;Database=2DAM_alumno19;User ID=<USER>;Password=<PASSWORD>;SslMode=Preferred;Connect Timeout=5;`. This lets the API start and answer 503 before the real values exist.
- When the developer gives the credentials after group 1, the agent stores them with one `dotnet user-secrets set "ConnectionStrings:Default" "…"` command, run from `src/OpenWave.Api`. The value goes to the user profile's `UserSecrets/<id>/secrets.json`, outside the repository. The credentials are never written to any repository file, README, planning artifact, `.http` file, or report, and `dotnet user-secrets list` is never run.
- `SslMode=Preferred` works whether or not the unknown server offers TLS. `Connect Timeout=5` keeps a database failure shorter than the app's 10-second HTTP timeout, so the app receives a 503 rather than its own timeout.

### 4. Service results instead of exceptions for expected outcomes

Services return a small `ServiceResult<T>`, holding either a value or a `ServiceError` (`Invalid`, `NotFound`, `Conflict`, `Unauthorized`), plus Spanish messages. Controllers translate it with one `switch` into 201/200/204 or ProblemDetails (400 with a `ValidationProblemDetails` errors dictionary, then 401, 404 and 409).

Expected business outcomes therefore don't depend on exceptions. Only infrastructure failures throw.

Alternative: one custom exception per outcome, as `RadioBrowserUnavailableException` does. Rejected because five exception types for normal outcomes are harder to follow than one enum.

### 5. Database failures become 503 in one place

A global exception filter (`DatabaseExceptionFilter`) catches `MySqlException`, and `DbUpdateException` or `InvalidOperationException` whose inner exception is a `MySqlException`. It logs the exception type and message, never the request body, and returns 503 with `No se pudo acceder a la base de datos.`

The filter keeps the controllers free of repeated try/catch blocks. Duplicate-key errors (`MySqlException.Number == 1062`) are handled earlier, in `FavoriteService`, so they never reach the filter.

### 6. Validation and normalisation in the services

- **Email:** trimmed and lower-cased, at most 255 characters, and valid when `MailAddress.TryCreate` succeeds and the parsed address equals the input (this rejects display-name forms).
- **Password:** at least 8 characters for registration. On login an empty password is simply a failed login (401).
- **Spanish messages:** `El correo no es válido.` and `La contraseña debe tener al menos 8 caracteres.`
- **Malformed or missing JSON bodies:** `[ApiController]` would answer them in English, so `InvalidModelStateResponseFactory` is configured to return a 400 ProblemDetails with `La solicitud no es válida.`

### 7. Password hashing

`PasswordHasher<UserEntity>` comes from the ASP.NET Core shared framework, so no package is needed. It produces salted PBKDF2 hashes, which makes two equal passwords hash differently.

Login uses `VerifyHashedPassword`. When it returns `SuccessRehashNeeded`, the hash is updated, which is why the API needs UPDATE rights. Unknown email and wrong password follow the same path and return the same 401.

### 8. Favourites: copy, limits and duplicates

- **Valid station:** `id` (1–64 characters, matching `station_uuid`), `name` and `streamUrl` are non-empty, and both URLs are at most 2048 characters. Anything else is rejected with 400 `Los datos de la emisora no son válidos.`
- **Long catalogue values are trimmed to fit, not rejected:** Radio Browser names and tag lists can exceed the column sizes (a real top station has a name of about 330 characters). The name is cut to 255 characters, country and language to 100, and tags are joined with `,` and whole tags are dropped from the end until the text fits in 500.
- **Reading back:** tags are split on `,`.
- **Adding:** the service checks that the user exists (404), then that the favourite doesn't exist yet (409), then inserts with `created_at = UtcNow`. If a concurrent insert wins, the unique index raises error 1062, which the service also maps to 409. Foreign-key error 1452 (user deleted meanwhile) maps to 404.
- **Order:** the list is ordered by `created_at` descending, then `id` descending.
- **Route:** the delete route segment is named `{stationId}`, matching `StationDto.Id`. The URL shape is the one requested.

### 9. App services and session flow

- **Contracts:** `RegisterRequest`, `LoginRequest` and `UserDto` are added as records in `OpenWave.Contracts`.
- **HTTP clients:** `AuthService` and `FavoritesApiClient` are typed `HttpClient`s with the same base URL and timeout as stations. They throw one `ApiRequestException` with a `Kind` and the server's Spanish `detail`.
  - `Kind` is one of: `Unreachable`, `DatabaseUnavailable`, `Invalid`, `Unauthorized`, `Conflict`, `NotFound`, `Unexpected`.
  - The app shows the server's `detail` for 400, 401 and 409.
  - It uses fixed messages for `Unreachable` (`No hay conexión con el servidor.`) and `DatabaseUnavailable` (`No se pudo acceder a la base de datos.`).
- **`SessionService`:** a singleton `ObservableObject` with `CurrentUser` (`UserDto?`), `IsLoggedIn`, `LogIn(UserDto)` and `LogOut()`. Nothing is written to `Preferences`.
- **`FavoritesService`:** a singleton `ObservableObject` that is the single source of truth for favourite ids. It holds a `HashSet<string>` of ids and the last loaded list, and raises `FavoritesChanged`.
  - **Logging in:** when `SessionService` logs in, it loads the favourites. If there is a pending station (see below), it adds it.
  - **Logging out:** it clears the ids, the list and any pending station.
  - **Add and remove:** these update the set optimistically and revert it on failure. A 409 on add or a 404 on remove means the server already has the wanted state, so it is kept as success.
- **Login flow:** Shell routes `login` and `register` are registered with `Routing.RegisterRoute` and pushed onto the current tab's stack. Each entry into the flow (a heart while logged out, or the `Iniciar sesión` button in `Favoritos`) first sets `FavoritesService.PendingStation`, possibly to null, so a cancelled flow can never add a stale station later.
  - A successful login pops one page (`..`).
  - A successful registration pops both pages (`../..`), returning the user to where the flow started.
- **Client-side checks:** the same rules as the API, plus `Las contraseñas no coinciden.` While a request runs, `IsBusy` disables the button and shows an `ActivityIndicator`.

Alternative: redirect automatically when the `Favoritos` tab is selected. The developer rejected it, because it can loop when going back.

### 10. Heart toggle and Favoritos UI

- **Inicio:** `StationListItem` gains an observable `IsFavorite`. `InicioViewModel` sets it after each load and on every `FavoritesChanged`, the same way it already restores `IsSelected`.
- **Reproductor:** `ReproductorViewModel` projects `IsFavorite` for `CurrentStation`.
- **The heart control:** a `Button` with the text `♡` or `♥`, styled by a new semantic `FavoriteToggleButton` style (transparent background, `AccentColor` text via `AppThemeBinding`, size from a font-size token). Being a `Button`, it consumes its own tap, so tapping it doesn't trigger the card's select gesture.
- **Save errors:** shown with `Shell.Current.DisplayAlertAsync("Favoritas", "No se pudo actualizar tus favoritas.", "Aceptar")`, following the existing pattern of view models using `Shell.Current` for navigation.
- **`Favoritos` states:**
  - Logged out: `Inicia sesión para ver tus favoritas.` and an `Iniciar sesión` button.
  - Logged in: the email, a `Cerrar sesión` button, and a list with tap to play and a `Quitar` button.
  - Loading, error with `Reintentar`, and empty `Aún no tienes emisoras favoritas.`
  - The page reloads from the API each time it appears while logged in, so it always shows stored data.
- **Playing a favourite:** calls `PlaybackService.SelectStationAsync` and then `Shell.Current.GoToAsync("//reproductor")`, the same as `Inicio`.

### 11. Project config fix

The free text above the YAML keys in `openspec/config.yaml` moves under a `context: |` block, so the file parses and OpenSpec loads the project context again. The database name line is updated from `openwave` to `2DAM_alumno19`. No other rule text changes.

## Risks / Trade-offs

- **Shared server forbids the `openwave_app` user** → The block is commented out and optional. The README explains that the teacher's account is then used and that the API still never runs DDL.
- **Unknown server version or TLS policy** → No explicit collation, `SslMode=Preferred`, and the developer verifies the script in Workbench before any test.
- **Table names `users`/`favorite_stations` already exist in the shared database** → `CREATE TABLE IF NOT EXISTS` won't overwrite them. Group 1 stops so the developer can check that the created tables match the script before data is written.
- **Credentials leaking through tooling** → They are written only by `dotnet user-secrets set`, never echoed, listed or logged. EF sensitive-data logging stays off, and the exception filter logs only the exception type and message.
- **Unauthenticated favourites endpoints** → Accepted course limitation, documented in the README.
- **Heart glyph missing in Open Sans** → The platform's fallback font draws `♡`/`♥` on Windows and Android. This is verified in groups 5 and 6, and an SVG icon is the fallback plan if it renders badly.
- **Optimistic heart updates can briefly disagree with the server** → The heart reverts on failure and `Favoritos` always reloads from the API.
- **Long catalogue values are shortened when stored** → Only the stored copy is shortened, never the live catalogue; the names stay recognisable.

## Migration Plan

1. The developer runs `database/schema.sql` once in MySQL Workbench, connected to `2DAM_alumno19`.
2. The developer gives the credentials, and they are stored with `dotnet user-secrets`.
3. The API and app are built and run as before; no existing data is migrated.

**Rollback:** drop the two tables in Workbench (developer-run) and revert the source. No git commands are used by the agent.
