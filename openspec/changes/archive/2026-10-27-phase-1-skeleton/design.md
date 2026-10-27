# Design

## Context

The repository contains only the OpenSpec root: no solution, no projects, no specs. This phase creates the skeleton that later phases extend. Constraints that shape the approach:

- .NET SDK 10.0.402, nullable reference types enabled; MAUI targets only `net10.0-android` and `net10.0-windows10.0.19041.0`; API uses controllers.
- Strict layering from the project context: app -> API over HTTP; API controllers -> services -> repositories. Phase 1 has no database, so the repository layer is absent and the station service returns in-memory data.
- Manual verification only, on Windows and the Android emulator; no test projects.
- No git commands are run by the agent.
- The project context's `openspec/config.yaml` currently fails YAML parsing (Markdown headings inside the file), so the OpenSpec CLI ignores it. Its content was read directly and is treated as the project context for this plan. Fixing that file is out of scope for this change.

## Goals / Non-Goals

**Goals:**

- A buildable solution with the three projects and the exact folder layout from the project context.
- One contract type shared by API and app, so the JSON shape cannot drift.
- An API that runs on a fixed local URL and can be inspected manually in Swagger.
- A MAUI shell whose three tabs and simulated playback work on both Windows and the Android emulator.
- A single, replaceable seam for playback (`PlaybackService`) and for the backend URL (`ApiSettings`).

**Non-Goals:**

- Audio playback, Radio Browser, MySQL, EF Core, repositories, authentication, functional search and favourites, preferences UI, theming settings.
- Any abstraction beyond what this phase uses (no mediator, no generic repository, no test projects).
- Production hardening: CORS policy, HTTPS, rate limiting, restricted cleartext traffic.

## Decisions

### Solution and project layout

`OpenWave.sln` at the repository root, with `src/OpenWave.Contracts`, `src/OpenWave.Api` and `src/OpenWave.App`, matching the project context. `OpenWave.sln` is created with `dotnet new sln`, projects are added with `dotnet new classlib` / `dotnet new webapi --use-controllers` / `dotnet new maui`, and referenced with `dotnet add reference`.

- API references Contracts; App references Contracts. Contracts references nothing.
- `database/` is not created in this phase because there is no schema yet.
- Alternative considered: `dotnet new maui` multi-targeting iOS/macOS and trimming targets afterwards. Rejected: targets are set to the two required frameworks directly to keep the build honest and avoid dead configuration.

### Contract shape

`StationDto` is a sealed record with init-only properties: `Id`, `Name`, `StreamUrl` as non-nullable strings; `Country`, `Language`, `LogoUrl` as nullable strings; `Tags` as `string[]?`. `Id` stays a string because later phases will map external catalogue identifiers.

- The API serializes with ASP.NET Core's default camelCase JSON; the app deserializes with `System.Text.Json` web defaults through `GetFromJsonAsync`, which is case-insensitive.
- Alternative considered: separate request/response DTOs. Rejected as unnecessary for a read-only list.

### API: controllers, health and stations

- `HealthController` handles `GET /api/health` and returns an anonymous object or a small record with `status = "ok"` and `timeUtc = DateTime.UtcNow` (serialized as ISO 8601 with `Z`).
- `StationsController` handles `GET /api/stations`, depends on `IStationService`, and returns the DTO list. `StationService` owns the 12 hard-coded stations and returns them as `IReadOnlyList<StationDto>`. No repository is introduced yet: the service is the seam that phase 2/3 replaces with the Radio Browser-backed implementation.
- Registration: `builder.Services.AddScoped<IStationService, StationService>()`; controllers are registered by `AddControllers()`.
- Swagger: Swashbuckle (`AddEndpointsApiExplorer`, `AddSwaggerGen`, `UseSwagger`/`UseSwaggerUI`) wrapped in `if (app.Environment.IsDevelopment())`. Swagger UI stays at `/swagger`.
- Fixed URL: `Properties/launchSettings.json` http profile with `applicationUrl: "http://localhost:5080"` and `launchBrowser: true`. No HTTPS profile is used, because the Android emulator reaches the API over cleartext HTTP and the app has no certificate trust for a dev certificate.
- Alternative considered: .NET 10's built-in `AddOpenApi`/`MapOpenApi`. Rejected because it does not ship a Swagger UI, and the task explicitly asks to test through Swagger. If the unpinned Swashbuckle package does not support `net10.0`, stop and ask before changing approach (ground rule: never invent or downgrade versions).

### App: shell, routes and navigation

- `AppShell.xaml` uses a single `TabBar` with three `ShellContent` entries in order: `Inicio` (`inicio`), `Reproductor` (`reproductor`), `Favoritos` (`favoritos`). MAUI Shell renders this as a bottom tab bar on Android and as a top tab strip on Windows; that is the accepted "equivalent" on Windows.
- Pages are resolved from DI through Shell's data templates (`ContentTemplate="{DataTemplate views:InicioPage}"`) with constructor-injected view models.
- Selecting a station does not pass data through query strings. The page calls `Shell.Current.GoToAsync("//reproductor")`, and the player reads the station from the shared `PlaybackService`. This keeps navigation simple and survives tab switching.
- Alternative considered: `Shell.Current.CurrentItem = ...` index switching. Rejected: route-based navigation is the pattern later phases will need (favourites detail, etc.).

### App: API base URL and HTTP

- `ApiSettings` is a small class with a `BaseUrl` property resolved once at startup: `Preferences.Get("api_base_url", platformDefault)`, with `#if ANDROID` -> `http://10.0.2.2:5080` and `#if WINDOWS` -> `http://localhost:5080`. A malformed override falls back to the platform default instead of throwing.
- `MauiProgram` creates the `ApiSettings` instance, registers it as a singleton, and registers a typed client: `AddHttpClient<IStationService, StationService>(client => client.BaseAddress = new Uri(apiSettings.BaseUrl))` with a 10-second timeout so the error state appears quickly.
- The app's `IStationService` exposes `GetStationsAsync(CancellationToken)` and uses `GetFromJsonAsync<List<StationDto>>("api/stations", ...)`. `BaseAddress` ends with `/` and the relative path has no leading slash, so the path composes correctly.
- Alternative considered: a static `HttpClient` created in the service. Rejected: typed clients give DI-managed lifetimes and one obvious place for the base address.

### App: MVVM and screen states

- CommunityToolkit.Mvvm `ObservableObject`, `[ObservableProperty]` and `[RelayCommand]`. Both CommunityToolkit.Mvvm and CommunityToolkit.Maui are added now because the project context requires them; `UseMauiCommunityToolkit()` is initialised in `MauiProgram`, but no MediaElement is used until the audio phase.
- `InicioViewModel` holds `ObservableCollection<StationDto> Stations`, `IsLoading`, `HasError`, `ErrorMessage`, a `LoadStationsCommand` and a `SelectStationCommand(StationDto)`. The first appearance triggers the load through a guard flag; `Reintentar` calls the same command. Errors are caught in the view model and turned into the Spanish message `No se pudo conectar con el servidor. Comprueba que la API está en marcha.` The view never catches exceptions.
- `ReproductorViewModel` exposes the singleton `PlaybackService` plus a `TogglePlaybackCommand` and `CanControlPlayback`. The page shows the "Selecciona una emisora en Inicio" hint when `CurrentStation` is null.
- `FavoritosViewModel` exists only to keep the page constructor-injected and consistent; it holds no data.
- Views contain only XAML and lifecycle wiring (`OnAppearing` forwarding to a command). No business logic, no direct HttpClient use.
- Alternative considered: putting the state machine in the page code-behind. Rejected: it breaks the project's MVVM rule and makes phase 2 harder.

### App: simulated playback

- `PlaybackService` is a singleton deriving from `ObservableObject`, with `[ObservableProperty]` `CurrentStation` and `IsPlaying`, plus a computed `StatusText` (`Reproduciendo` / `Detenido`) kept in sync with `[NotifyPropertyChangedFor]`. Methods: `SelectStation(StationDto)` (sets the station and forces `IsPlaying = false`), `Play()`, `Stop()`, `Toggle()`.
- Selecting a different station while playing stops playback. This is not specified by the user; it is the least surprising behaviour for a simulated player and keeps phase 2's "replace the simulation" seam small.
- The service performs no I/O and never touches `StreamUrl`. Phase 2 replaces its method bodies with MediaElement calls while keeping the same properties.
- Alternative considered: a service with an event that the view model subscribes to. Rejected: the singleton would hold a reference to a transient view model, and the observable service is simpler for a course project.

### Styles and resources

- `Resources/Styles/Colors.xaml` defines semantic keys with `AppThemeBinding` light/dark values: `PageBackgroundColor`, `PrimaryTextColor`, `SecondaryTextColor`, `AccentColor`, `ErrorColor`, `SurfaceColor`, `DividerColor`.
- `Resources/Styles/Styles.xaml` defines `AppFontFamily`, `TitleFontSize`, `BodyFontSize`, `CaptionFontSize` and implicit styles for `Page`, `Label`, `Button`, `Entry`, `CollectionView` items. Pages reference them with `DynamicResource`; no page hard-codes a colour, font family or size.
- Platform default fonts are used (`AppFontFamily` maps to the system font) so no font file is added in this phase.

### Android platform configuration

- `Platforms/Android/AndroidManifest.xml`: keep `<uses-permission android:name="android.permission.INTERNET" />` and set `android:usesCleartextTraffic="true"` on `<application>` so `http://10.0.2.2:5080` works.
- Alternative considered: a network security config limited to `10.0.2.2`. Rejected for now as extra configuration; the project is development-only and the risk is recorded below.

### Documentation and repository hygiene

- `.gitignore` covers `bin/`, `obj/`, `.vs/`, `.vscode/`, `*.user`, `appsettings.Development.json`, `secrets.json`, `*.keystore`, and OS/editor noise. Editing `.gitignore` is explicitly allowed by the project context; no git command is run.
- `README.md` documents prerequisites (.NET SDK, MAUI workload, Android emulator), the API URL and Swagger URL, how to run the API, the app on Windows and on the Android emulator, and the `api_base_url` override.

## Risks / Trade-offs

- **MAUI workload or Android SDK missing** -> the first app build fails for environment reasons, not code. Mitigation: the first task group verifies the workload and reports the exact command output; installing workloads is a developer action, not something to guess around.
- **Swashbuckle does not support `net10.0`** -> Swagger UI cannot be enabled as designed. Mitigation: add the package unpinned, run the build, and stop to ask before choosing another UI or approach.
- **Package version drift** -> builds may pick versions with breaking changes. Mitigation: no pinned versions, restore and build after each task group, and report real output.
- **Windows tab presentation differs from Android** -> Shell shows tabs at the top on Windows. Accepted as the platform equivalent; the requirement is reachability and order, not pixel parity.
- **Cleartext HTTP enabled app-wide on Android** -> acceptable for a development-only build, but it must be narrowed before any release. Mitigation: recorded here and in the README.
- **10-second timeout on a slow emulator** -> the error state may appear on a slow first request. Mitigation: the retry button is the designed recovery path; the timeout is a constant in one place.
- **Playback state is in memory only** -> closing the app loses the selection. Accepted: no persistence is in scope.
- **No automated tests** -> regressions are caught only by the manual checks in `tasks.md`. Accepted by the project context.
- **Stale list after returning to Inicio** -> the list is loaded once per app session unless the user retries. Accepted for a fixed catalogue; phase 2 can add refresh.

## Migration Plan

Greenfield: no data, schema or deployment migration. Rollout is local only. Rollback is deleting the added projects and files; git history is controlled by the developer and no git commands are run here.

## Open Questions

None blocking. The exact 12 station names, countries, languages and tags are chosen during implementation from plausible real-world examples; the specs only constrain count, required fields, uniqueness and plausibility.
