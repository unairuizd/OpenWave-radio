# Tasks

Stop after each group, run the builds listed for that group, and report the real output before continuing. No git commands at any point.

## 1. Solution, projects and repository hygiene

- [x] 1.1 Verify the environment: `dotnet --version` reports 10.0.402 and `dotnet workload list` shows the MAUI workload. Report both outputs; if the workload is missing, stop and report instead of improvising.
- [x] 1.2 Create `OpenWave.sln` and `src/OpenWave.Contracts` (class library, `net10.0`, nullable enabled), add the project to the solution, and verify `dotnet build src/OpenWave.Contracts` succeeds.
- [x] 1.3 Create `src/OpenWave.Api` (ASP.NET Core Web API with controllers, `net10.0`), add it to the solution, reference Contracts, and verify `dotnet build src/OpenWave.Api` succeeds.
- [x] 1.4 Create `src/OpenWave.App` (MAUI), set `TargetFrameworks` to only `net10.0-android;net10.0-windows10.0.19041.0`, add it to the solution, reference Contracts, and verify both `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0` and `dotnet build src/OpenWave.App -f net10.0-android` succeed.
- [x] 1.5 Add `.gitignore` covering `bin/`, `obj/`, `.vs/`, `.vscode/`, `*.user`, `appsettings.Development.json`, `secrets.json`, `*.keystore` and OS/editor files; verify by reading the file that each required pattern is present.
- [x] 1.6 Write `README.md` with prerequisites, the API URL (`http://localhost:5080`), the Swagger URL, run commands for the API, the app on Windows and the app on the Android emulator, and the `api_base_url` override; verify every command matches the projects that exist.
- [x] 1.7 Run `dotnet build OpenWave.sln` and verify it succeeds for both app target frameworks, reporting any workload or SDK error verbatim.

## 2. API health and station stub

- [x] 2.1 Add `StationDto` to Contracts with `Id`, `Name`, `StreamUrl` non-nullable and `Country`, `Language`, `Tags`, `LogoUrl` optional; verify `dotnet build src/OpenWave.Contracts` succeeds.
- [x] 2.2 Add `IStationService` and `StationService` under `src/OpenWave.Api/Services` returning 12 hard-coded plausible stations with unique ids, and register the service in `Program.cs`; verify `dotnet build src/OpenWave.Api` succeeds.
- [x] 2.3 Add `StationsController` exposing `GET /api/stations` and returning the DTO list; verify with the API running that `Invoke-RestMethod http://localhost:5080/api/stations` returns 12 items with unique non-empty `id`, `name` and `streamUrl`.
- [x] 2.4 Add `HealthController` exposing `GET /api/health`; verify with the API running that `Invoke-RestMethod http://localhost:5080/api/health` returns `status` equal to `ok` and a parseable ISO 8601 `timeUtc`.
- [x] 2.5 Enable Swagger UI in Development only; verify `http://localhost:5080/swagger` loads and lists both endpoints while the registration is wrapped in an `IsDevelopment()` check.
- [x] 2.6 Configure the `http` launch profile in `Properties/launchSettings.json` with `applicationUrl` `http://localhost:5080`; verify `dotnet run --project src/OpenWave.Api` starts on port 5080 and the health URL responds.
- [x] 2.7 Verify the station catalogue has no external dependency: with the API running and no database configured, request `/api/stations` twice and confirm the full list and ids are identical both times.
- [x] 2.8 Confirm the README API and Swagger commands work exactly as written; correct the README if the real startup command or URLs differ.

## 3. App shell, tabs and styles

- [x] 3.1 Add semantic resources in `src/OpenWave.App/Resources/Styles`: `PageBackgroundColor`, `PrimaryTextColor`, `SecondaryTextColor`, `AccentColor`, `ErrorColor`, `SurfaceColor`, `DividerColor`, `AppFontFamily`, `TitleFontSize`, `BodyFontSize`, `CaptionFontSize`, with implicit styles for the controls in use; verify no page hard-codes a colour, font family or font size by searching the XAML for hex colours and `FontSize=`.
- [x] 3.2 Build `AppShell.xaml` with one `TabBar` containing `Inicio`, `Reproductor` and `Favoritos` in that order, plus the three placeholder pages under `Views/`; verify `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0` succeeds.
- [x] 3.3 Register the pages, view models and services in `MauiProgram.cs` (including `UseMauiCommunityToolkit()`), with pages resolved through Shell data templates; verify the Windows build succeeds and the app starts without DI resolution errors.
- [x] 3.4 Run the app on Windows and verify all three tabs appear in the order `Inicio`, `Reproductor`, `Favoritos`, the app opens on `Inicio`, and switching tabs works without a restart.
- [x] 3.5 Confirm the README Windows run instructions match how the app was actually launched; correct the README if needed.

## 4. API access and Inicio list states

- [x] 4.1 Add `ApiSettings` resolving `Preferences["api_base_url"]` with platform defaults (`http://10.0.2.2:5080` on Android, `http://localhost:5080` on Windows) and falling back on a malformed override; register it and a typed `HttpClient` with a 10-second timeout in `MauiProgram.cs`; verify the Windows build succeeds.
- [x] 4.2 Add the app-side `IStationService` and implementation calling `GET /api/stations` through the typed client; verify the Windows build succeeds.
- [x] 4.3 Add `InicioViewModel` with `Stations`, `IsLoading`, `HasError`, `ErrorMessage`, a load command and a retry command using CommunityToolkit.Mvvm, catching request failures and producing the Spanish message; verify the Windows build succeeds.
- [x] 4.4 Build `InicioPage` with a visible non-functional search placeholder, a `CollectionView` of station names, a loading indicator bound to `IsLoading`, and an error panel with the Spanish message and a `Reintentar` button bound to the retry command; verify with the API running that Inicio lists 12 stations and typing in the search box does not change the list.
- [x] 4.5 Verify the error path on Windows: stop the API, restart the app (or retry), confirm Inicio shows the Spanish error and `Reintentar`, the app does not crash, and the other tabs remain reachable; then start the API, press `Reintentar`, and confirm the list replaces the error.
- [x] 4.6 Confirm the Windows app sends requests only to `http://localhost:5080` by checking the API console log while Inicio loads; report any request to another host as a defect.

## 5. Simulated player, selection and Favoritos

- [x] 5.1 Add `PlaybackService` as a singleton `ObservableObject` with `CurrentStation`, `IsPlaying`, computed `StatusText` (`Reproduciendo` / `Detenido`) and `SelectStation`/`Play`/`Stop`/`Toggle`, registered in DI; verify the Windows build succeeds.
- [x] 5.2 Add `ReproductorViewModel` and `ReproductorPage` showing the selected station name and country, the status text, and a play/stop control bound to the toggle; verify the Windows build succeeds.
- [x] 5.3 Wire station selection in Inicio: tapping a station calls `PlaybackService.SelectStation` and navigates to `Reproductor`; verify on Windows that tapping a station opens the player with that station's name and country.
- [x] 5.4 Verify on Windows that Play changes the status to `Reproduciendo`, Stop changes it to `Detenido`, no audio is heard, and a placeholder stream URL does not produce a network request.
- [x] 5.5 Verify the no-selection path: open `Reproductor` before selecting anything and confirm it shows `Selecciona una emisora en Inicio`.
- [x] 5.6 Verify state persistence: start playback, switch to `Favoritos` and back, confirm the station and `Reproduciendo` remain; select a different station while playing and confirm the player shows the new station with `Detenido`.
- [x] 5.7 Build `FavoritosPage` with `AÃºn no tienes emisoras favoritas.` and a short Spanish note that it is not connected yet; verify on Windows that the text appears and no network request is made when opening the tab.
- [x] 5.8 Update the README with a short manual check list for the player and the Favoritos placeholder, and verify each step matches the running app.

## 6. Android emulator and cleartext configuration

- [x] 6.1 Set `android:usesCleartextTraffic="true"` on the application element and keep the `INTERNET` permission in `Platforms/Android/AndroidManifest.xml`; verify by reading the manifest that both entries are present.
- [x] 6.2 Build the Android target with `dotnet build src/OpenWave.App -f net10.0-android` and verify it succeeds.
- [x] 6.3 Run the app on the Android emulator with the API running and verify the three tabs, 12 stations in Inicio, station selection, the play/stop toggle, and the Favoritos text.
- [x] 6.4 Verify the Android app reaches the API through `http://10.0.2.2:5080` by confirming the station list loads and the API log shows the emulator requests.
- [x] 6.5 Verify the Android error path: stop the API, retry in Inicio, confirm the Spanish error and `Reintentar` appear and the app does not crash; restart the API and confirm retry recovers.
- [x] 6.6 Verify the documented Android run command and the cleartext note in the README by following them from a clean terminal; correct any mismatch.
