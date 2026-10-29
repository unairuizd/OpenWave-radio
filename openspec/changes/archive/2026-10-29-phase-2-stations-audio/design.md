# Design

## Context

See `proposal.md` for motivation. The current implementation has a fixed 12-station catalogue in `src/OpenWave.Api/Services/StationService.cs` and a simulated `PlaybackService` in the app. The API is an ASP.NET Core controller API on .NET 10 with a typed `HttpClient` already used by the MAUI app. The app uses Shell with the `Inicio`, `Reproductor` and `Favoritos` destinations, CommunityToolkit.Mvvm, semantic styles in `Resources/Styles`, and a configurable API base URL. Android already allows cleartext HTTP and has the `INTERNET` permission; Windows is unpackaged.

The project context requires Radio Browser to be contacted only by the API, real MediaElement audio in the app, manual verification only, and no unit-test project.

## Goals / Non-Goals

**Goals:**

- Serve a real, filtered, limited station catalogue from Radio Browser through the existing `GET /api/stations` endpoint.
- Keep the API usable when one mirror fails, and return a clear Spanish 502 when the catalogue cannot be reached.
- Let the user search by station name, clear the search, see empty/loading/error states, and identify the selected station in the list.
- Play and stop real streams on Windows and the Android emulator, with clear Spanish buffering/playing/error states and retry.
- Keep playback state in one place so the list and the player cannot disagree.

**Non-Goals:**

- No login, registration, database, favourites persistence, preferences UI, theming settings, caching, mini-player, or background playback.
- No direct Radio Browser calls from the app.
- No stream transcoding, recording, seeking, volume controls, or a persistent notification.
- No unit-test project; verification is manual and through Swagger plus both app targets.

## Decisions

### 1. API: Radio Browser client and mirror fallback

Use a typed `HttpClient` behind `IRadioBrowserClient` in `src/OpenWave.Api`. `RadioBrowserClient` owns the external HTTP and JSON shape; `StationService` owns the catalogue query normalization and delegates to the client. The client returns `StationDto` values so there is no EF or external entity leaking beyond the API service layer.

Configuration:

```json
"RadioBrowser": {
  "Servers": [
    "https://de1.api.radio-browser.info",
    "https://all.api.radio-browser.info"
  ]
}
```

The live check on 2026-10-29 showed that `de1.api.radio-browser.info` and `all.api.radio-browser.info` respond, while `at1.api.radio-browser.info` and `nl1.api.radio-browser.info` do not resolve. The default list is therefore corrected, but remains an ordered configuration list so it can be changed without code changes. `all.api.radio-browser.info` is kept as the second entry because it is the documented DNS-round-robin entry point; `de1` is the direct fallback.

Client behavior:

- Read `RadioBrowserOptions.Servers` through `IOptions<RadioBrowserOptions>`. If the configured list is empty or contains no valid absolute HTTP(S) URL, fall back to the built-in default list and log a warning.
- Send `User-Agent: OpenWave/1.0` and `Accept: application/json` on every mirror request. Set `HttpClient.Timeout` to infinite and enforce a 5-second timeout per mirror with a linked `CancellationTokenSource`, so each mirror gets exactly one attempt.
- Try mirrors in order. A mirror fails on network error, timeout, non-success status, or invalid JSON. Log a warning with the mirror and reason, then continue.
- Use the first mirror that returns a valid JSON array, including an empty array. An empty result is a valid search result, not a 502.
- If every mirror fails, throw `RadioBrowserUnavailableException` with the last technical cause. The controller catches it, logs a concise error, and returns `ProblemDetails` with status 502, title `Bad Gateway` and detail exactly `No se pudo obtener el catálogo de emisoras.`
- Do not catch or translate caller cancellation as a 502; propagate it.

Alternatives considered: dynamic DNS discovery of `all.api.radio-browser.info` was rejected because the request explicitly requires an ordered configured list; a single hard-coded host was rejected because it gives no fallback.

### 2. API: query, mapping and result rules

`StationService` exposes a `StationQuery` record with `Name`, `Country`, `Language`, `Tag` and `Limit`. The controller binds `name`, `country`, `language`, `tag` and `limit` with `[FromQuery]`.

- `limit` null or `<= 0` becomes 50; values above 100 become 100. The normalized value is sent to Radio Browser as `limit`.
- Radio Browser is called at `/json/stations/search` with `hidebroken=true`, `order=clickcount`, `reverse=true`, the normalized `limit`, and only the optional filters that have a non-empty value.
- The external JSON is represented by an internal `RadioBrowserStation` record with `[JsonPropertyName]` attributes for `stationuuid`, `name`, `url_resolved`, `url`, `country`, `language`, `tags` and `favicon`. Unknown fields are ignored by `System.Text.Json`.
- Mapping:
  - `Id` = trimmed `stationuuid`; skip if empty.
  - `Name` = trimmed `name`; skip if empty.
  - `StreamUrl` = trimmed `url_resolved`, falling back to trimmed `url`; skip if both are empty.
  - `Country`, `Language`, `LogoUrl` = trimmed values, or `null` when empty.
  - `Tags` = comma-split, trimmed, non-empty values, or `null` when there are none.
- De-duplicate by `Id` defensively.
- When a `name` filter is present, apply a case-insensitive `Contains` check on the trimmed mapped name. This makes the search contract deterministic and prevents unrelated fuzzy upstream matches from appearing. The `country`, `language` and `tag` filters are left to Radio Browser's search semantics.
- The hard-coded station list is deleted; no production code references it.

The name post-filter is a deliberate trade-off: it can return fewer results than Radio Browser for very loose matches, but the app only needs a predictable name search in this phase.

### 3. App: station search and Inicio states

Replace the app's `IStationService.GetStationsAsync` with:

```csharp
Task<IReadOnlyList<StationDto>> SearchAsync(string? name, CancellationToken cancellationToken = default);
```

The typed client calls only the configured OpenWave API. It builds `api/stations?limit=50`, adds `name` only when the trimmed text is non-empty, and URL-encodes the value. It reads the response explicitly rather than using `GetFromJsonAsync`, so it can inspect non-success responses.

Error mapping:

- `HttpRequestException`, or a timeout/`TaskCanceledException` not caused by caller cancellation, becomes `StationRequestError.Unreachable`.
- HTTP 502 becomes `StationRequestError.CatalogueUnavailable`; the app shows the fixed Spanish message `No se pudo obtener el catálogo de emisoras.` rather than trusting arbitrary server text.
- Other non-success responses or malformed success bodies become `StationRequestError.Unexpected`, which uses the same catalogue message.
- Caller cancellation is ignored so a newer search can replace an older one.

`InicioViewModel` owns `SearchText`, `Stations`, `IsLoading`, `HasError`, `ErrorMessage` and `IsEmpty`. It cancels the previous request when a new search starts and only lets the latest request update the UI. A successful load clears the previous error and list, adds the returned stations, and sets `IsEmpty` when the list is empty. A failed load clears the list, shows the error overlay and keeps the search text so `Reintentar` can repeat the same query. `LoadIfNeeded` loads the top stations once per session; a successful search remains visible when returning to `Inicio`.

The search area uses an `Entry` with `ReturnType="Search"` and `ReturnCommand`, a visible `Buscar` button, and a visible `Limpiar` button. `Limpiar` clears `SearchText` and loads the top stations again. The empty overlay shows `No se encontraron emisoras.`

Selected-item highlight: wrap each result in a `StationListItem` with the `Station` and an observable `IsSelected`. `InicioViewModel` subscribes to `PlaybackService.CurrentStation` changes and updates every item by station `Id`; after a new load it restores the highlight by comparing against the current station. This is preferred over `CollectionView` selection because the selected station must survive a new search and must update when playback changes from another page. A `DataTrigger` on `IsSelected` changes the card to the new semantic `SelectedSurfaceColor`/`SelectedSurfaceColorDark` and the accent stroke.

### 4. App: MediaElement and playback ownership

Add the `CommunityToolkit.Maui.MediaElement` package unpinned (`dotnet add src/OpenWave.App package CommunityToolkit.Maui.MediaElement`). The current package targets `net10.0` and is compatible with the existing MAUI target; the implementation must not pin a version by hand. Call `.UseMauiCommunityToolkitMediaElement()` in `MauiProgram` with the default (no Android foreground service), because background playback is out of scope.

Update `Platforms/Android/MainActivity.cs` to the MediaElement-required activity shape: `ResizeableActivity = true` and `LaunchMode = LaunchMode.SingleTask`, keeping the existing `ConfigurationChanges`. No new Android permissions are needed; `INTERNET` and cleartext HTTP are already present.

Playback architecture:

- `PlaybackState` is the app's four-value enum: `Stopped`, `Buffering`, `Playing`, `Error`.
- `PlaybackService` is a singleton and the only owner of `CurrentStation`, `State`, `ErrorMessage` and the computed Spanish `StatusText`. It exposes async `SelectStationAsync`, `PlayAsync`, `StopAsync`, `RetryAsync` and `ToggleAsync`.
- `IAudioPlayer` is the small adapter between the service and the MediaElement. `MediaElementAudioPlayer` is a singleton that attaches to the `MediaElement` hosted by `ReproductorPage`, subscribes to `StateChanged` and `MediaFailed`, maps `MediaElementState` to `PlaybackState`, and raises state/failure events.
- If a station is selected before the page has attached, the adapter stores the pending URL. `Attach` starts it when the `MediaElement` is loaded. This is required because selecting in `Inicio` sets the station before Shell navigates to `Reproductor`.
- The adapter uses a plain `UriMediaSource` without HTTP headers. With headers, MediaElement on Windows reads the source through a HEAD request and HTTP Range requests, which never starts a live radio stream because it has no content length (found during verification of task 4.8).
- The adapter handles `MediaElement` events on the main thread. `Paused` before the stream has reached `Playing` is part of opening (Windows reports `Opening` → `Paused` before auto-play starts) and is ignored; `Paused` after `Playing` means the platform paused the stream and is treated as a stop.
- Start a 15-second watchdog when a stream starts. If the player has not reached `Playing`, stop it and raise a failure. Cancel the watchdog on `Playing`, `Stopped` or `MediaFailed`.
- `SelectStationAsync`:
  - If the same station is already buffering or playing, do nothing.
  - If the same station is stopped or in error, retry it.
  - Otherwise stop the previous stream, set the new `CurrentStation`, clear the error, set `Buffering`, and ask the adapter to play.
- `PlaybackService` maps adapter failures to `Error` and the exact Spanish message `No se pudo reproducir esta emisora.` The technical `ErrorMessage` from MediaElement is logged, not shown to the user.
- All observable property updates are marshalled to the main thread. The adapter is responsible for marshalling its events; the service does not update UI-bound state from a background thread.

`ReproductorPage` hosts a 1×1 `MediaElement` at the end of the page, outside the artwork area, so the platform player view (a `SurfaceView` on Android) can never draw over the station logo or the placeholder. It is never collapsed, so the platform player remains alive while audio-only playback runs. `OnAppearing` attaches it; `OnDisappearing` detaches it, which stops audio and clears pending playback. `App.OnSleep` also asks `PlaybackService` to stop, so backgrounding the app cannot leave audio running. This implements the page-scoped playback decision: no mini-player and no background playback.

Alternatives considered: putting MediaElement playback logic in `ReproductorViewModel` would couple the view model to a control and make it hard to keep state consistent from `Inicio`; a page-only code-behind player would make `PlaybackService` a second source of truth. The adapter keeps the state in the service and the control in the view.

### 5. App: player presentation and failures

`ReproductorViewModel` exposes read-only projections of the service: station, logo URL, country, state text, error message, `HasStation`, `HasNoStation`, `HasError`, `ShowPlayerControls` and `ToggleButtonText`. It subscribes to `PlaybackService.PropertyChanged` and raises all dependent properties. `ToggleCommand` and `RetryCommand` delegate to the service.

The page shows:

- `Selecciona una emisora en Inicio` when no station is selected.
- The station name, country (when present), logo or local fallback, and status text when a station is selected.
- `Reproducir`/`Detener` for the normal toggle, and `Reintentar` in the error state.
- `No se pudo reproducir esta emisora.` below the status in the error state.

The logo fallback is a local `station_placeholder.svg` in `Resources/Images`, with the remote logo stacked above it. If the URL is empty or the image fails to load, the placeholder remains visible; no image-failure event is required. The artwork area uses the semantic surface background of the `PlayerArtwork` style.

New semantic style keys:

- `SelectedSurfaceColor`/`SelectedSurfaceColorDark` for the selected station card.
- `PlayerArtworkSize` (or a `PlayerArtwork` style) for the artwork area, so the page does not hard-code a size.

All user-facing strings are Spanish; code identifiers and comments stay English.

### 6. Capability retirement

The change sets `retire_capabilities: true` in `.openspec.yaml`. The `simulated-playback` delta removes all three old requirements with reasons and migrations, and the new `audio-playback` delta adds the real playback requirements. At archive, the old capability is retired and the new capability is created. This avoids leaving a main spec that still claims audio is simulated.

## Risks / Trade-offs

- **Mirror hosts can change or disappear** → Keep the list in `RadioBrowser:Servers`, verify the live hosts at implementation time, log each failed mirror, and return a clear 502 when all fail. The current list uses `de1` and `all`.
- **Radio Browser rate limits or rejects the client** → Always send `User-Agent: OpenWave/1.0`, do not poll in a loop, and surface a 502 instead of crashing. No caching in this phase.
- **Streams are unreliable or use HLS** → Use a plain URI source (no custom headers, see above), test both an MP3/AAC stream and an HLS stream, and show the Spanish error with retry when playback fails.
- **MediaElement package/Android setup mismatch** → Add the package unpinned, call `UseMauiCommunityToolkitMediaElement()`, update `MainActivity` to `SingleTask` and `ResizeableActivity`, and build both target frameworks before UI verification.
- **Audio-only MediaElement layout** → Host it as a 1×1 element outside the artwork rather than collapsing it; verify audio still plays on Windows and Android and that no unwanted video/black rectangle is visible.
- **Page-scoped playback makes the “switch while playing” UI path narrow** → The service still stops the previous stream before starting a new one, and leaving `Reproductor` stops audio. The README documents this limitation; a persistent mini-player is deliberately out of scope.
- **Name post-filtering can return fewer results** → Accept the stricter, predictable search in this phase; later preferences can use the country/language/tag filters directly.
- **Search races** → Cancel the previous request and ignore stale results; only the latest request may update `IsLoading`, the list or the error.
- **No automated tests** → Manual Swagger checks and Windows/Android verification are required in the task list; the project explicitly excludes a test project.

## Migration Plan

No data migration is needed. The API has no database and the app has no persisted catalogue. Implementation proceeds in the task groups: API client and catalogue, API error handling, app search, app playback, then cross-platform verification and documentation. Rollback is a manual revert of the changed source, configuration and planning files; the previous fixed catalogue and simulated player can be restored if needed. No git commands are used.

## Open Questions

None. The remaining choices (exact timeout, fallback asset, selected-card colors) are implementation details covered above and do not change the specs or task breakdown.
