# Tasks

Do not run any git command during this change. Stop after each group, run the group's build and checks, report the actual output, and summarize the files, commands and manual checks before continuing.

## 1. API: Radio Browser client and mirror fallback

- [x] 1.1 Add `RadioBrowserOptions` with the `RadioBrowser:Servers` list and the corrected defaults `https://de1.api.radio-browser.info` and `https://all.api.radio-browser.info`; add the section to `appsettings.json`; verify with `dotnet build src/OpenWave.Api`.
- [x] 1.2 Add `IRadioBrowserClient`/`RadioBrowserClient`, the internal `RadioBrowserStation` JSON record, and the mapping to `StationDto`; send `User-Agent: OpenWave/1.0`, use a 5-second timeout per mirror, and try mirrors in order; register the typed client in `Program.cs`; verify with `dotnet build src/OpenWave.Api`.
- [x] 1.3 Replace the hard-coded list in `StationService` with a basic call to the client for the default top-station request, keeping the controller working; verify with `dotnet build src/OpenWave.Api` and Swagger `GET /api/stations` returning real station names and stream URLs.
- [x] 1.4 Verify mirror fallback by temporarily setting the first configured mirror to `https://at1.api.radio-browser.info` and the second to `https://de1.api.radio-browser.info`; confirm the API returns 200 and the log records the first mirror failure; restore the valid configuration and do not commit the temporary values.
- [x] 1.5 Run `dotnet build src/OpenWave.Api`, repeat the Swagger real-catalogue check, report the actual output, and stop before group 2.

## 2. API: query contract, 502 handling and logging

- [x] 2.1 Add `StationQuery` and pass `name`, `country`, `language`, `tag` and `limit` through `StationService`; normalize `limit` to 50 when absent or non-positive and cap it at 100; apply the case-insensitive name post-filter and de-duplicate by station id; verify with `dotnet build src/OpenWave.Api`.
- [x] 2.2 Add the query parameters to `StationsController`, catch `RadioBrowserUnavailableException`, and return `502 ProblemDetails` with detail exactly `No se pudo obtener el catálogo de emisoras.`; add the Swagger response metadata; verify with `dotnet build src/OpenWave.Api`.
- [x] 2.3 Log each mirror failure with its technical cause, log the final all-mirrors failure, and confirm every request still sends `User-Agent: OpenWave/1.0`; verify by code review and by successful Swagger requests against the live mirrors.
- [x] 2.4 Update `OpenWave.Api.http` with `name`, `limit` and optional-filter examples; confirm `appsettings.json` contains the valid mirror list; verify the HTTP file examples against a running API.
- [x] 2.5 Verify in Swagger that `?name=jazz` returns only names containing `jazz`, `?limit=5` returns at most 5, `?limit=500` returns at most 100, and `country`, `language` and `tag` are forwarded; temporarily run the API with two invalid mirror URLs and confirm the 502 detail and the logged technical cause; restore the valid configuration.
- [x] 2.6 Run `dotnet build src/OpenWave.Api`, repeat the Swagger/502 checks, report the actual output, and stop before group 3.

## 3. App: search service and Inicio states

- [x] 3.1 Replace the app's `IStationService.GetStationsAsync` with `SearchAsync(string? name, CancellationToken)`, build the API query with URL-encoded name, parse `ProblemDetails` for 502, and map unreachable/502/unexpected failures to app-specific errors; verify with `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`.
- [x] 3.2 Add `StationListItem` and update `InicioViewModel` with `SearchText`, `SearchCommand`, `ClearSearchCommand`, retry, loading, error, empty and cancellation handling; restore the selected highlight by station id and update it when `PlaybackService.CurrentStation` changes; verify with the Windows build.
- [x] 3.3 Update `InicioPage.xaml` with the search `Entry` (`ReturnType="Search"`), `Buscar` and `Limpiar` buttons, the `StationListItem` template, the selected `DataTrigger`, and the loading/error/empty overlays; verify with the Windows build.
- [x] 3.4 Add `SelectedSurfaceColor`/`SelectedSurfaceColorDark` to `Colors.xaml` and use them only through the selected-card style/trigger; verify the styles compile in the Windows build.
- [x] 3.5 Update the README search/error section with the working search, clear, empty and two error messages; verify every documented command and string against the running app.
- [x] 3.6 On Windows, with the API running, verify top stations, search `jazz`, clear, a nonsense search showing `No se encontraron emisoras.`, the API-down message `No hay conexión con el servidor.`, the 502 message `No se pudo obtener el catálogo de emisoras.`, retry, and the selected highlight; report the observed results.
- [x] 3.7 With `$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"`, build and deploy the Android app and repeat the search, clear, empty, error, retry and highlight checks on the emulator; report the observed results.
- [x] 3.8 Run the Windows and Android builds for this group, report the actual output, and stop before group 4.

## 4. App: MediaElement, playback states and Reproductor

- [x] 4.1 Add `CommunityToolkit.Maui.MediaElement` with an unpinned `dotnet add src/OpenWave.App package CommunityToolkit.Maui.MediaElement`, call `.UseMauiCommunityToolkitMediaElement()` in `MauiProgram.cs`, and update `Platforms/Android/MainActivity.cs` to `ResizeableActivity = true` and `LaunchMode = LaunchMode.SingleTask`; verify with the Windows and Android builds.
- [x] 4.2 Implement `PlaybackState`, `IAudioPlayer` and `MediaElementAudioPlayer` with attach/detach, pending playback, `StateChanged`/`MediaFailed` mapping handled on the main thread (ignoring `Paused` before `Playing`), the 15-second watchdog, and a plain `UriMediaSource` without HTTP headers; register the adapter as a singleton; verify with the Windows build.
- [x] 4.3 Rewrite `PlaybackService` around `CurrentStation`, `State`, `ErrorMessage`, `StatusText`, `SelectStationAsync`, `PlayAsync`, `StopAsync`, `RetryAsync` and `ToggleAsync`; stop the previous stream before starting a different station and keep the same station playing when it is already active; verify with the Windows build.
- [x] 4.4 Update `ReproductorViewModel` to project the new state, commands and error fields from `PlaybackService`; update `InicioViewModel` to await `SelectStationAsync` before navigating; verify with the Windows build.
- [x] 4.5 Update `ReproductorPage.xaml` with the hosted `MediaElement`, artwork/logo fallback area, Spanish state text, `Reproducir`/`Detener`, `Reintentar` and error message; update the code-behind to attach on appearing and detach on disappearing; stop playback from `App.OnSleep`; verify with the Windows build.
- [x] 4.6 Add `station_placeholder.svg` to `Resources/Images` and a `PlayerArtwork` semantic style/size token; verify the fallback appears when a station has no logo and that the page still compiles.
- [x] 4.7 Update the README with real playback, the Spanish states, the error/retry behavior, the page-scoped playback limitation, no mini-player and no background playback; verify every documented command and behavior.
- [x] 4.8 On Windows, verify a reachable station goes `Cargando…` → `Reproduciendo` with audio, `Detener` returns `Detenido`, selecting another station replaces the stream, leaving `Reproductor` stops audio, returning shows the same station as `Detenido`, the logo fallback works, and an unreachable stream shows `Error` plus `No se pudo reproducir esta emisora.`; report the observed results.
- [x] 4.9 On the Android emulator, repeat the playback, stop, switching, page-scoped stop, logo fallback and stream-error/retry checks; report the observed results.
- [x] 4.10 Run the Windows and Android builds for this group, report the actual output, and stop before group 5.

## 5. Cross-platform integration and final verification

- [x] 5.1 Verify the selected station highlight on Windows and Android after selecting, searching, clearing and returning to `Inicio`; fix any mismatch and re-run the affected platform check.
- [x] 5.2 Verify the Spanish API-down, 502, empty-results and stream-error messages on both platforms, including `Reintentar`; fix any incorrect or untranslated text and re-run the affected check.
- [x] 5.3 Run the full Windows flow with the API running: real top stations, `jazz` search, clear, play/stop, switching, page-scoped stop, logo fallback, error/retry and no crash; report the commands and observed results.
- [x] 5.4 Run the full Android emulator flow with the API running and `$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"`: the same catalogue/search/playback/error checks; report the commands and observed results.
- [x] 5.5 Run `dotnet build OpenWave.sln` with `ANDROID_HOME` set and confirm 0 warnings and 0 errors for both target frameworks; report the actual output.
- [x] 5.6 Re-read the README against the implemented behavior and the task list; correct any stale phase-1 text, commands or strings and verify the corrected commands.
- [x] 5.7 Update the `## Purpose` lines of `openspec/specs/station-catalog/spec.md` (live Radio Browser catalogue with search filters and a bounded result size) and `openspec/specs/station-browsing/spec.md` (browse, search and select real stations, including empty-search state), because delta specs cannot change a main spec's Purpose.
- [x] 5.8 Report the final files changed, commands run and manual checks performed, then stop. Do not start another change or run any git command.
