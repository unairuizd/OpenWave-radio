# Proposal

## Why

Phase 1 proved the navigation and screen structure with a fixed test catalogue and simulated playback. OpenWave is not yet a radio app: the stations are not real, search does nothing and no audio is streamed. This change connects the API to the live Radio Browser catalogue and replaces the simulation with real audio, so the core product can be demonstrated and the later favourites and preferences work has real station data to build on.

## What Changes

- Add a typed `RadioBrowserClient` behind `IRadioBrowserClient`. It sends `User-Agent: OpenWave/1.0`, reads an ordered mirror list from `RadioBrowser:Servers`, and tries each mirror in order with a 5-second timeout.
- Correct the default mirror list after checking the official documentation and the live hosts: `de1.api.radio-browser.info` and `all.api.radio-browser.info` respond; `at1.api.radio-browser.info` and `nl1.api.radio-browser.info` no longer resolve. The list stays configurable.
- Replace the hard-coded catalogue in `StationService`. `GET /api/stations` accepts `name`, `country`, `language`, `tag` and `limit` (default 50, maximum 100), calls Radio Browser's `/json/stations/search` with `hidebroken=true`, `order=clickcount`, `reverse=true`, and maps the external fields to `StationDto`.
- Return `502 ProblemDetails` with the detail `No se pudo obtener el catálogo de emisoras.` when every mirror fails, and log the technical cause on the server.
- Replace the app's station request with `IStationService.SearchAsync(string? name, CancellationToken)`. `Inicio` gets a working search submitted from the keyboard, a clear button, loading, empty and error states, and highlights the selected/playing station.
- Add real streaming with `CommunityToolkit.Maui.MediaElement`. `PlaybackService` becomes the single source of truth for the current station, playback state (`Stopped`, `Buffering`, `Playing`, `Error`), the Spanish error message and change notifications.
- Update `Reproductor` to show the station name, country, logo with a local fallback, Spanish state text (`Detenido`, `Cargando…`, `Reproduciendo`, `Error`), a play/stop control and a retry path when a stream fails.
- Selecting another station stops the previous stream and starts the new one. Playback is scoped to the `Reproductor` page in this phase: leaving the page stops audio, and there is no mini-player.
- **BREAKING** (development phase): the fixed test catalogue and the simulated player are removed; the API now depends on Radio Browser availability and the app now produces real audio.
- Retire the `simulated-playback` capability and replace it with a new `audio-playback` capability.

Out of scope: login, registration, database, favourites logic, mini-player, preferences UI, theming settings, caching, background playback.

## Capabilities

### New Capabilities

- `audio-playback`: real streaming playback, the `PlaybackService` state machine, play/stop/retry, Spanish failure handling, and page-scoped playback.

### Modified Capabilities

- `station-catalog`: the endpoint now serves the live Radio Browser catalogue with filters, a limit, field mapping and a Spanish 502 failure response.
- `station-browsing`: search by name, clear/search states, empty results, selection starts playback, and the selected station is highlighted.
- `api-connectivity`: all catalogue access, including search, goes through the configured OpenWave API; the app never calls Radio Browser directly.
- `simulated-playback`: retired and replaced by `audio-playback`; its simulated requirements are removed.

## Impact

- `src/OpenWave.Api`: new Radio Browser client, options/configuration, DI registration, `StationService` replacement, controller query parameters and 502 handling, logging, `appsettings.json`.
- `src/OpenWave.App`: new MediaElement package and initialization, Android activity configuration, station search client, Inicio and Reproductor view models/views, playback service and audio adapter, fallback image, semantic selected-item style.
- `src/OpenWave.Contracts`: no DTO shape change is expected; `StationDto` remains the API/app contract.
- `README.md`: new run notes, mirror configuration, search/playback behavior, page-scoped playback limitation and manual checks.
- External dependency: Radio Browser mirrors and station streams must be reachable from the API host and the app host respectively.
- OpenSpec: three modified capability specs, one new capability spec and one retired capability spec.
