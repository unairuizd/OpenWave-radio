# Proposal

## Why

The repository currently contains no application code: only the OpenSpec planning root. Phase 1 must produce a first, demonstrable vertical slice so the student can run the API and the MAUI app, see data coming from the backend, and move through the three main screens. It also fixes the project skeleton, layering and conventions that later phases (real audio, Radio Browser, database, authentication) will build on, so those phases do not have to restructure the solution.

## What Changes

- Create the .NET solution skeleton: `OpenWave.sln` with three projects under `src/` — `OpenWave.Contracts` (net10.0 class library), `OpenWave.Api` (ASP.NET Core Web API with controllers), `OpenWave.App` (.NET MAUI targeting Android and Windows).
- Add a `.gitignore` covering .NET, Visual Studio Code and secret/config files, plus a short `README.md` with run instructions for Windows and the Android emulator.
- Add `StationDto` to Contracts as the shared station shape used at the HTTP boundary.
- Add a minimal API: `GET /api/health` returning status and UTC time, and `GET /api/stations` returning 12 hard-coded test stations produced by a service behind an interface, with Swagger enabled in Development only and a fixed HTTP URL of `http://localhost:5080`.
- Add a MAUI app shell with three bottom tabs in order: Inicio, Reproductor, Favoritos.
- Add a configurable API base URL (`ApiSettings`, per-platform default, overridable through `Preferences`) and a typed `HttpClient` station service; all backend calls go through it.
- Add the Inicio page: non-functional search placeholder, station list, loading state, and a Spanish error state with a "Reintentar" button. Selecting a station navigates to Reproductor.
- Add the Reproductor page with a simulated `PlaybackService` singleton: shows the selected station, toggles between "Reproduciendo" and "Detenido", and shows "Selecciona una emisora en Inicio" when nothing is selected.
- Add the Favoritos placeholder page with its Spanish empty-state text and a note that it is not connected yet.
- Enable cleartext HTTP and the internet permission on Android.

Explicitly not included: real audio playback, Radio Browser integration, MySQL database, login/registration, functional search, functional favourites, preferences and theme settings.

## Capabilities

### New Capabilities

- `backend-health`: the API exposes a health endpoint reporting service status and current UTC time.
- `station-catalog`: the API exposes a station list endpoint backed by a service, returning a fixed set of test stations in this phase.
- `api-connectivity`: the app reaches the backend only through a single configurable base URL, with per-platform defaults.
- `app-navigation`: the app shell provides the Inicio, Reproductor and Favoritos destinations and navigation between them.
- `station-browsing`: Inicio lists stations with loading and error states and lets the user select a station.
- `simulated-playback`: the player shows the selected station and toggles a simulated playing/stopped state.

### Modified Capabilities

None. The project has no existing specs.

## Impact

- New files: solution and three projects, `.gitignore`, `README.md`, `database/` is not touched in this phase.
- New API endpoints: `GET /api/health`, `GET /api/stations`.
- New app configuration surface: `ApiSettings` base URL and the `api_base_url` Preferences override.
- New Android platform configuration: internet permission and cleartext HTTP.
- No existing code, specs or data are modified. No database objects, external APIs or secrets are involved in this phase.
