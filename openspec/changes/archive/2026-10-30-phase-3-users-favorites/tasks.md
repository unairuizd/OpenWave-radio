# Tasks

Do not run any git command during this change. Never run `database/schema.sql`, never open a database client, and never use EF migrations or `EnsureCreated`. Database credentials go only into `dotnet user-secrets` and are never written to any file in the repository, printed, or repeated in a report. Stop after each group, run the group's build and checks, report the actual output, and summarize the files, commands and manual checks before continuing.

## 1. Database script, data layer and configuration

- [x] 1.1 Fix `openspec/config.yaml`: move the free project text under a `context: |` block and change the database name from `openwave` to `2DAM_alumno19`, leaving all other rule text unchanged; verify that `openspec context --json` no longer prints the "could not parse" warning.
- [x] 1.2 Create `database/schema.sql`: a header comment, `USE \`2DAM_alumno19\`;`, then `users` and `favorite_stations` as in design decision 1 (including `language`, the unique indexes, the cascading foreign key, InnoDB and utf8mb4 without a collation), and a final commented-out `openwave_app` block with a password placeholder. Verify by reading the file against the design and the `user-accounts`/`favorite-stations` specs; do not run it.
- [x] 1.3 Add `RegisterRequest`, `LoginRequest` and `UserDto` records to `OpenWave.Contracts`; verify with `dotnet build src/OpenWave.Contracts`.
- [x] 1.4 Add `MySql.EntityFrameworkCore` with an unpinned `dotnet add src/OpenWave.Api package MySql.EntityFrameworkCore`, confirm the restored version targets `net10.0`, and verify with `dotnet build src/OpenWave.Api`.
- [x] 1.5 Add `Data/Entities/UserEntity`, `Data/Entities/FavoriteStationEntity` and `Data/OpenWaveDbContext` with fluent snake_case mapping, unique indexes and the relationship (no migrations, no `EnsureCreated`); register it in `Program.cs` with `UseMySQL` and `ConnectionStrings:Default`; verify with `dotnet build src/OpenWave.Api` and a search showing no `Migrate`/`EnsureCreated` calls.
- [x] 1.6 Add `IUserRepository`/`UserRepository` and `IFavoriteRepository`/`FavoriteRepository` under `Repositories/`, async with `CancellationToken`, covering only the queries the specs need; verify with `dotnet build src/OpenWave.Api`.
- [x] 1.7 Run `dotnet user-secrets init` in `src/OpenWave.Api` and add the placeholder `ConnectionStrings:Default` from design decision 3 to `appsettings.json`; verify that the csproj has a `UserSecretsId`, that the repository contains no real host, user or password, and that `dotnet build src/OpenWave.Api` succeeds.
- [x] 1.8 Add a README "Database setup" section: running `database/schema.sql` in MySQL Workbench against `2DAM_alumno19`, the optional `openwave_app` block, and storing the connection string with `dotnet user-secrets set "ConnectionStrings:Default" "<cadena>"` (placeholders only); verify the commands match the files.
- [x] 1.9 Run `dotnet build src/OpenWave.Api`, report the actual output, and stop. Ask the developer to run `database/schema.sql` in MySQL Workbench and to provide the credentials. Then store them with a single `dotnet user-secrets set` command run from `src/OpenWave.Api`, without echoing them back, and confirm only that the secret was saved.

## 2. Auth: registration and login

- [x] 2.1 Add `ServiceResult<T>`/`ServiceError` and `IAuthService`/`AuthService`: email normalisation and validation, password length check, `PasswordHasher<UserEntity>` hashing and verification (with rehash on `SuccessRehashNeeded`), and the duplicate-email check; verify with `dotnet build src/OpenWave.Api`.
- [x] 2.2 Add `AuthController` with `POST /api/auth/register` (201, 400, 409) and `POST /api/auth/login` (200, 400, 401), the Swagger response metadata, and the Spanish `InvalidModelStateResponseFactory`; verify with `dotnet build src/OpenWave.Api`.
- [x] 2.3 Add `DatabaseExceptionFilter` mapping MySQL failures to 503 `No se pudo acceder a la base de datos.` and logging only the exception type and message; register it globally; verify with `dotnet build src/OpenWave.Api` and a code review showing that no password, request body or connection string is logged.
- [x] 2.4 With the API running on the stored secret, verify in Swagger: register → 201; the same email in another case and with spaces → 409; invalid email → 400 `El correo no es válido.`; short password → 400; correct login (other case) → 200; wrong password and unknown email → identical 401. Then confirm in Workbench (developer) that `password_hash` is not the plain password. Report the observed results.
- [x] 2.5 Verify 503 by running the API with the environment variable `ConnectionStrings__Default` set to the placeholder connection string from `appsettings.json` (no real credentials), so the database is unreachable: login returns 503 with the Spanish detail, the API keeps running, and the log shows the cause without secrets. Remove the variable afterwards.
- [x] 2.6 Add register/login examples (fake data only) to `OpenWave.Api.http` and an "Authentication" section to the README listing the endpoints and status codes; verify the examples against the running API.
- [x] 2.7 Run `dotnet build src/OpenWave.Api`, report the actual output, and stop before group 3.

## 3. Favourites endpoints

- [x] 3.1 Add `IFavoriteService`/`FavoriteService`: user-exists check, station validation, trimming to the column limits (name 255, country and language 100, tags whole-tag up to 500), the existing-favourite check, duplicate-key 1062 → Conflict, foreign-key 1452 → NotFound, newest-first listing, and removal; verify with `dotnet build src/OpenWave.Api`.
- [x] 3.2 Add `FavoritesController` at `api/users/{userId:long}/favorites` with GET (200, 404), POST (201, 400, 404, 409) and DELETE `{stationId}` (204, 404), with Swagger metadata; verify with `dotnet build src/OpenWave.Api`.
- [x] 3.3 Verify in Swagger with a registered user:
  - POST a station → 201, POST it again → 409 `La emisora ya está en tus favoritas.`, and the developer confirms in Workbench that one row exists.
  - POST without `streamUrl` → 400; POST and GET for an unknown user → 404.
  - GET returns newest first; a station with a name over 255 characters is stored shortened.
  - DELETE → 204, DELETE again → 404.

  Report the observed results.
- [x] 3.4 Verify persistence: add two favourites, stop and restart the API, log in again, and confirm `GET` returns the same favourites in the same order. Verify 503 for `GET` with the unreachable placeholder connection string as in 2.5.
- [x] 3.5 Add favourites examples to `OpenWave.Api.http` and a "Favourites API" README section, including the known limitation that these endpoints are not authorised; verify the examples against the running API.
- [x] 3.6 Run `dotnet build src/OpenWave.Api`, report the actual output, and stop before group 4.

## 4. App: session, login and registration

- [x] 4.1 Add `ApiRequestException`/`ApiErrorKind` and `IAuthService`/`AuthService` (typed `HttpClient`, same base URL and timeout as stations) that maps unreachable, 400, 401, 409, 503 and unexpected responses and keeps the server's Spanish `detail`; register it in `MauiProgram.cs`; verify with the Windows build.
- [x] 4.2 Add `SessionService` (singleton `ObservableObject`: `CurrentUser`, `IsLoggedIn`, `LogIn`, `LogOut`, nothing saved to `Preferences`) and register it; verify with the Windows build.
- [x] 4.3 Add `LoginViewModel`/`LoginPage` and `RegisterViewModel`/`RegisterPage` with the client-side checks, `IsBusy` loading state, Spanish error text, and navigation back to the screen that opened the flow (`..` after login, `../..` after registration). Register the `login` and `register` routes. Use only semantic style keys. Verify with the Windows build.
- [x] 4.4 Replace the `Favoritos` placeholder with the logged-out state (`Inicia sesión para ver tus favoritas.` and `Iniciar sesión`) and the logged-in header (email and `Cerrar sesión`); verify with the Windows build.
- [x] 4.5 On Windows with the API running, verify:
  - Anonymous browsing and playback still work.
  - Login error messages (wrong credentials, invalid email, API stopped).
  - Registration: passwords not matching, duplicate email, and success logging in straight away.
  - Returning to `Favoritos` after login.
  - Logging out keeps the selected station in `Reproductor`.
  - Restarting the app starts logged out.

  Report the observed results.
- [x] 4.6 Add a README "Account in the app" section describing login, registration and logout; verify every string against the running app.
- [x] 4.7 Run the Windows and Android builds, report the actual output, and stop before group 5.

## 5. App: favourites, heart toggle and Favoritos list

- [x] 5.1 Add `FavoritesApiClient` (list, add, remove through the API) and `FavoritesService` (singleton: id set, last list, `FavoritesChanged`, `PendingStation`, load on login, clear on logout, optimistic add/remove with revert, 409/404 treated as success); register both; verify with the Windows build.
- [x] 5.2 Add the `FavoriteToggleButton` semantic style and font-size token, add `IsFavorite` to `StationListItem`, and add the heart and `ToggleFavoriteCommand` to `InicioPage`/`InicioViewModel`. The command adds or removes when logged in, and sets `PendingStation` and opens `login` when logged out. Show `No se pudo actualizar tus favoritas.` on failure. Verify with the Windows build.
- [x] 5.3 Add the heart to `ReproductorPage`/`ReproductorViewModel` for the current station with the same behaviour; verify with the Windows build.
- [x] 5.4 Complete `FavoritosViewModel`/`FavoritosPage` for logged-in users: reload on appearing, loading indicator, error with `Reintentar` (unreachable and 503 messages), empty text, list with tap-to-play (`SelectStationAsync` then `//reproductor`), and an immediate `Quitar`. Verify with the Windows build.
- [x] 5.5 On Windows with the API running, verify:
  - Hearts after login, and adding from `Inicio` and from `Reproductor` without changing playback.
  - A heart while logged out → login → station added and the app back on the same screen; cancelling the login adds nothing.
  - Removing in `Favoritos` updates `Inicio`; removing the last favourite shows the empty text.
  - Playing from `Favoritos`.
  - Save failure with the API stopped (message and reverted heart); 503 and `Reintentar` in `Favoritos`.
  - Logout clears every heart, and the `♡`/`♥` glyphs render.

  Report the observed results.
- [x] 5.6 Add a README "Favourites in the app" section and update the manual check list for hearts and `Favoritos`; verify every string against the running app.
- [x] 5.7 Run the Windows and Android builds, report the actual output, and stop before group 6.

## 6. Cross-platform integration and final verification

- [x] 6.1 Run the full flow on Windows with the API using the stored secret: register, log in, add favourites from `Inicio` and `Reproductor`, list, play and remove in `Favoritos`, log out, restart the API, log in again and see the same favourites; report the commands and observed results.
- [x] 6.2 With `$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"`, run the same flow on the Android emulator, including the heart glyphs, the logged-out `Favoritos` state and the error messages; report the observed results.
- [x] 6.3 Verify that neither the app source nor its settings contain any database host, user or password, and that the repository contains no real credentials (search for the host and user name given by the developer without printing them); report only whether anything was found.
- [x] 6.4 Run `dotnet build OpenWave.sln` with `ANDROID_HOME` set and confirm 0 warnings and 0 errors for every project; report the actual output.
- [x] 6.5 Re-read the README against the implemented behaviour and correct any stale text (including the old "no database" note); verify the corrected commands.
- [x] 6.6 Report the final files changed, commands run and manual checks performed, and tell the developer exactly where the credentials are stored (the user-secrets `secrets.json` path for this project's `UserSecretsId`) and how to change them; then stop without running any git command.

## Workflow follow-up

- Archive the change with `/opsx:archive` after the developer has reviewed it.
