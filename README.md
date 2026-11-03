# OpenWave

Cross-platform online radio player for the DEIN course project. An ASP.NET Core API serves the live Radio Browser catalogue and stores user accounts and favourite stations in a remote MySQL database. A .NET MAUI app (Windows and Android) lets you browse, search and listen to stations, create an account, log in and keep your favourites. In `Ajustes` (the gear in every page's header) you can save default station filters (country, language, genre and content type) for Inicio, and change the theme, its variant, the font and the text size; both are stored on the device.

## Requirements

- .NET SDK 10.0.402 (or a compatible 10.0.x SDK)
- .NET MAUI workload: `dotnet workload install maui`
- Android SDK with platform **API 36** and an emulator (only for the Android target). The app needs Android 8.0 (API 26) or later, because the audio player (`CommunityToolkit.Maui.MediaElement`) requires it.
- Visual Studio Code with the C# and .NET MAUI extensions (optional but recommended)

If the Android build cannot find the SDK, point the build at it:

```powershell
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
```

## Solution layout

```
OpenWave.sln
database/schema.sql       MySQL script (run manually; source of truth for the schema)
src/OpenWave.Contracts/   Shared DTOs (net10.0)
src/OpenWave.Api/         ASP.NET Core Web API (controllers)
src/OpenWave.App/         .NET MAUI app (Android + Windows)
```

## Database setup

Users and favourites are stored in the remote MySQL database `2DAM_alumno19` (provided by the teacher). Only the API connects to it; the app never does.

1. Open MySQL Workbench, connect with your account, and run `database/schema.sql`. It selects `2DAM_alumno19` and creates the `users` and `favorite_stations` tables. It never drops anything, so running it again is safe.
2. Optional: the end of the script has a commented-out block that creates a restricted `openwave_app` account with only SELECT, INSERT, UPDATE and DELETE rights. On a shared server your account may not be allowed to create users; in that case skip it and use your own account.
3. Store the connection string with .NET user-secrets, from the API folder. The values stay in your user profile, outside the repository:

   ```powershell
   cd src/OpenWave.Api
   dotnet user-secrets set "ConnectionStrings:Default" "Server=<HOST>;Port=<PORT>;Database=2DAM_alumno19;User ID=<USER>;Password=<PASSWORD>;SslMode=Preferred;Connect Timeout=5;"
   ```

   Replace `<HOST>`, `<PORT>`, `<USER>` and `<PASSWORD>` with your real values (omit `Port=<PORT>;` if the server uses the default port 3306). `appsettings.json` only contains a placeholder; never put real credentials in any file of the repository.

   The value is saved in `%APPDATA%\Microsoft\UserSecrets\bdfcf5f9-ed90-45fc-ba9d-9cd82bdcde78\secrets.json` (the folder name is the `UserSecretsId` in `OpenWave.Api.csproj`). To change the credentials, run the same `dotnet user-secrets set` command again with the new values, or edit that file. User-secrets are read only when the API runs in the `Development` environment, which `dotnet run` uses by default.

The API never creates or changes tables: there are no EF migrations and no `EnsureCreated`. With the placeholder connection string, database requests fail with `503` and `No se pudo acceder a la base de datos.`

## Run the API

```powershell
dotnet run --project src/OpenWave.Api
```

- Base URL: `http://localhost:5080`
- Health check: `http://localhost:5080/api/health`
- Stations: `http://localhost:5080/api/stations`
- Swagger UI (Development only): `http://localhost:5080/swagger`

Quick check:

```powershell
Invoke-RestMethod http://localhost:5080/api/health
Invoke-RestMethod http://localhost:5080/api/stations
Invoke-RestMethod "http://localhost:5080/api/stations?name=jazz&limit=5"
```

### Station catalogue

`GET /api/stations` returns live stations from [Radio Browser](https://www.radio-browser.info), most popular first. The API needs internet access; the app never calls Radio Browser directly.

| Query parameter | Meaning |
|---|---|
| `name` | Text the station name must contain (case-insensitive) |
| `country` | Exact country name, ignoring case (for example `Spain`) |
| `language` | Exact language, ignoring case; matches when it is one of the station's languages (for example `spanish`) |
| `tag` | Exact genre tag, ignoring case (for example `jazz`; `pop` does not match `k-pop`) |
| `contentType` | `music`, `news`, `talk` or `sports`; any other value gives `400` `Tipo de contenido no válido.` |
| `limit` | Maximum number of stations: 50 by default, at most 100 |

Every filter given must match. When both `tag` and `contentType` are given, each station has both tags. The values for `country`, `language` and `tag` come from the catalogue option lists below; the API looks them up ignoring case and sends Radio Browser its own spelling, because Radio Browser's exact matching is case-sensitive.

The mirrors are tried in the order listed in `RadioBrowser:Servers` in `appsettings.json`, with a 5-second timeout each. If every mirror fails, the API returns `502` with a ProblemDetails body whose `detail` is `No se pudo obtener el catálogo de emisoras.` and logs the technical cause. `OpenWave.Api.http` has ready-made example requests.

### Catalogue options

These lists feed the app's `Preferencias` page. Each option is `{"value": "...", "label": "...", "stationCount": 123}`: `value` is what you send to `/api/stations`, `label` is what the app shows.

| Endpoint | Contents |
|---|---|
| `GET /api/catalog/countries` | Every country with working stations; `value` and `label` are the country name |
| `GET /api/catalog/languages` | The 100 languages with the most stations; `label` is the name with a capital first letter (`spanish` → `Spanish`) |
| `GET /api/catalog/genres` | The 100 tags with the most stations, without single-character tags; `label` capitalised like languages |
| `GET /api/catalog/content-types` | Always `music`/`Música`, `news`/`Noticias`, `talk`/`Charlas`, `sports`/`Deportes`; never contacts Radio Browser |

The lists are sorted by `label`, ignoring case and accents. Countries, languages and genres are kept in memory for 60 minutes after the first successful load, and a failed load is not kept, so the next request tries Radio Browser again. If every mirror fails they return `502` with `No se pudo obtener el catálogo de emisoras.`

### Authentication

Accounts are stored in the database (see [Database setup](#database-setup)). Both endpoints take a JSON body `{"email": "...", "password": "..."}` and return `{"id": 1, "email": "ana@example.com"}` on success. Emails are trimmed and lower-cased before they are stored or compared; passwords are stored only as salted hashes.

| Endpoint | Success | Errors (`detail`) |
|---|---|---|
| `POST /api/auth/register` | `201` | `400` `El correo no es válido.` or `La contraseña debe tener al menos 8 caracteres.`; `409` `El correo ya está registrado.` |
| `POST /api/auth/login` | `200` | `400` `El correo no es válido.`; `401` `Correo o contraseña incorrectos.` (same for an unknown email and a wrong password) |

A malformed JSON body gives `400` `La solicitud no es válida.` When the database cannot be reached, both endpoints return `503` `No se pudo acceder a la base de datos.` and the API keeps running. `OpenWave.Api.http` has example requests with fake data.

### Favourites API

Each user's favourite stations are stored in the database as a copy of the station data (the same JSON format as `/api/stations`), so they keep working when the catalogue changes or Radio Browser is down. `{userId}` is the `id` returned by register or login.

| Endpoint | Success | Errors (`detail`) |
|---|---|---|
| `GET /api/users/{userId}/favorites` | `200` list, newest first | `404` `El usuario no existe.` |
| `POST /api/users/{userId}/favorites` (station body) | `201` the stored station | `400` `Los datos de la emisora no son válidos.`; `404` `El usuario no existe.`; `409` `La emisora ya está en tus favoritas.` |
| `DELETE /api/users/{userId}/favorites/{stationId}` | `204` | `404` `La emisora no está en tus favoritas.` or `El usuario no existe.` |

- A station needs `id`, `name` and `streamUrl`.
- Values longer than the database columns are shortened when stored: the name to 255 characters, country and language to 100, and whole tags are dropped from the end until the tag list fits in 500 characters.
- A station can be a favourite only once per user, even when two identical requests arrive at the same time.
- When the database cannot be reached, these endpoints return `503` `No se pudo acceder a la base de datos.`

**Known limitation:** these endpoints are not authorised. Anyone who knows a user id can read or change that user's favourites; the session exists only in the app. This is accepted for this course project.

## Run the app on Windows

With the API running:

```powershell
dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0
& "src/OpenWave.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/OpenWave.App.exe"
```

The app uses `http://localhost:5080` on Windows. The `dotnet build -t:Run` shortcut is avoided because the SDK fails to quote this repository path, which contains spaces.

## Run the app on the Android emulator

1. Start an emulator, for example from Visual Studio or with the Android SDK tools:

   ```powershell
   $env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
   & "$env:ANDROID_HOME\emulator\emulator.exe" -avd Medium_Phone
   ```

2. Wait until the emulator has booted, then deploy and run:

   ```powershell
   $env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
   dotnet build src/OpenWave.App -f net10.0-android -t:Run
   ```

   Always deploy with `-t:Run` or `-t:Install`. A Debug build uses Fast Deployment: the app's assemblies are copied to the device separately from the APK, so installing only the APK file (for example with `adb install`) keeps running the previously deployed code.

The app uses `http://10.0.2.2:5080` on Android, which maps to the host machine's `localhost`.

Cleartext HTTP is allowed in `Platforms/Android/AndroidManifest.xml` for development. This must be restricted before any release build.

## Changing the API base URL

The app reads the base URL from the MAUI `Preferences` key `api_base_url`. With no override it uses the platform default (`http://localhost:5080` on Windows, `http://10.0.2.2:5080` on Android). `Ajustes` has no option for it; set the value programmatically when needed:

```csharp
Preferences.Default.Set("api_base_url", "http://192.168.1.10:5080");
```

## Search and error states in Inicio

- Inicio shows the stations as a grid of cards (see [Navigation and layout](#navigation-and-layout)).
- Type a name in the search box and press Enter or the magnifier (`Buscar`): Inicio lists the stations whose name contains that text. The cross (`Limpiar búsqueda`) empties the box and shows the top stations again.
- While a request is running, Inicio shows `Cargando emisoras...`.
- When there are no results and no preference filter is applied, Inicio shows `No se encontraron emisoras.` The search box stays available.
- When the API cannot be reached, Inicio shows `No hay conexión con el servidor.`
- When the API answers `502` (Radio Browser unavailable) or something unexpected, Inicio shows `No se pudo obtener el catálogo de emisoras.`
- Both errors show `Reintentar`, which repeats the last request, including the search text.
- The station selected in the app is highlighted in the list, and the highlight is restored after a new search or when returning to Inicio.

### Preferences in Inicio

The saved `Preferencias` (see below) are default filters for Inicio. Every request, including name searches and `Limpiar`, sends the saved country, language, genre and content type together with the typed name. After the preferences change, Inicio reloads its list the next time it is shown. Favoritos and Reproductor are not affected.

- **Bar:** when at least one preference is saved, a bar above the list reads `Filtros activos` with a summary of the saved values (for example `Spain · Spanish · Jazz · Música`) and a `Desactivar` button. With no preference saved there is no bar.
- **`Desactivar`:** stops applying the preferences until the app is closed and reloads the unfiltered list. The saved values stay in `Preferencias`, and the bar reads `Preferencias desactivadas` with an `Activar` button that applies them again. The next time the app starts, the preferences are applied again.
- **No matches:** when the applied preferences match no station, Inicio shows `No se encontraron emisoras con tus preferencias.` with its own `Desactivar` button.

## Playback in Reproductor

Selecting a station in Inicio or Favoritos (the card or its play button) opens Reproductor and starts the station's stream with `CommunityToolkit.Maui.MediaElement`. The page shows, from top to bottom:

- a large logo (a local placeholder when there is no logo or it cannot be loaded);
- the station name and its details (country · language · up to three tags) on the left, and the heart on the right;
- three controls: previous favourite, play/stop in the centre, next favourite;
- a status line with the state, any notice, and the error with `Reintentar`.

Before any station is selected the page shows `Selecciona una emisora en Inicio` instead of the logo and details, and play/stop is disabled.

| Status | Meaning | Centre control |
|---|---|---|
| `Detenido` | Nothing is playing | Play icon (`Reproducir`) |
| `Cargando…` | The stream is opening or buffering | Stop icon (`Detener`) |
| `Reproduciendo` | Audio is playing | Stop icon (`Detener`) |
| `Error` | The stream could not be opened, did not start within 15 seconds, or stopped unexpectedly | Play icon (`Reproducir`) |

In the `Error` state the status line also shows `No se pudo reproducir esta emisora.` and `Reintentar`, which tries the same station again; the play icon does the same. Selecting another station stops the previous stream first; selecting the station that is already playing keeps it playing.

### Previous and next favourite

The buttons on either side of play/stop (`Emisora favorita anterior` and `Emisora favorita siguiente`) move through your favourites in the `Favoritos` order (newest first) and play the chosen station:

- They wrap: next on the last favourite plays the first, previous on the first plays the last.
- When the current station is not a favourite (or none is selected), next plays the first favourite and previous the last one.
- When your only favourite is the current station, nothing changes.
- With no favourites the status line shows `Aún no tienes emisoras favoritas.`
- Logged out, they open `Iniciar sesión`; nothing is added to your favourites.

Limitations:

- **Playback continues across pages.** The app has one audio player that belongs to the window, not to a page or a theme, so the audio keeps playing while you go to Inicio, Favoritos or Ajustes, and while you change the theme. It stops only when you stop it, choose another station, the stream fails or the app goes to the background.
- **No mini-player:** Moderno shows the controls only in Reproductor; Tocadiscos also plays and stops from the sleeves in Inicio and Favoritos.
- **No background playback.** Minimising the app or sending it to the background stops the audio.

## Account in the app

You can browse, search and listen without an account. An account is needed only for favourites.

- **Logging in:** open `Favoritos` and press `Iniciar sesión`. Enter your email and password and press `Iniciar sesión`. While the request runs the button is disabled and a loading indicator is shown. On success the app returns to `Favoritos`, which shows your email and `Cerrar sesión`.
- **Creating an account:** on the `Iniciar sesión` page press `Crear cuenta`, enter your email, a password of at least 8 characters and the same password again, and press `Crear cuenta`. You are logged in straight away and returned to `Favoritos`.
- **Logging out:** press `Cerrar sesión` in `Favoritos`. The station selected in `Reproductor` stays selected and can still be played.
- **The session is kept only while the app is open.** Every app start begins logged out.

Messages on the `Iniciar sesión` and `Crear cuenta` pages:

| Message | When |
|---|---|
| `El correo no es válido.` | The email is empty or not a valid address (checked before contacting the API) |
| `Introduce la contraseña.` | Login with an empty password |
| `La contraseña debe tener al menos 8 caracteres.` | Registration with a shorter password |
| `Las contraseñas no coinciden.` | Registration with different passwords |
| `Correo o contraseña incorrectos.` | Wrong password or unknown email |
| `El correo ya está registrado.` | Registration with an email that already has an account |
| `No hay conexión con el servidor.` | The API is not running or cannot be reached |
| `No se pudo acceder a la base de datos.` | The API cannot reach the database |
| `Ha ocurrido un error inesperado.` | Any other failure |

## Favourites in the app

Each station card and the station in `Reproductor` show a heart: filled (`Quitar de favoritas`) when it is one of your favourites, outline (`Añadir a favoritas`) when it is not or when nobody is logged in.

- **Adding and removing:** while logged in, press the outline heart to add the station and the filled heart to remove it. The heart changes at once; it does not select the station, change the page or stop playback. If the change cannot be saved, the heart goes back and the app shows `No se pudo actualizar tus favoritas.`
- **Heart while logged out:** the app opens `Iniciar sesión`. After logging in (or creating an account) the station is added and you return to the screen where you pressed the heart. Going back without logging in adds nothing. In `Reproductor` this stops the audio, because playback is tied to that page.
- **`Favoritos`:** shows your email and `Cerrar sesión` in a compact row under the header, then your favourites, newest first, as the same cards as Inicio. Tap a card or its play button to open `Reproductor` and play it, or press its filled heart to remove it at once (no confirmation). The station selected in the app is highlighted. The list reloads from the API every time the page opens.
  - While loading it shows `Cargando favoritas...`; with no favourites, `Aún no tienes emisoras favoritas.`
  - If the list cannot be loaded it shows `No hay conexión con el servidor.` or `No se pudo acceder a la base de datos.` with `Reintentar`.
- **Logging out** empties every heart and the list.

## Navigation and layout

Every page except `Iniciar sesión` and `Crear cuenta` has the same frame, drawn by the active theme:

- **Header:** the page title in upper case, centred, and a gear (`Ajustes`) on the right. The gear opens `Ajustes`; from `Personalización` or `Preferencias` it returns to `Ajustes` (never opening a second one); on `Ajustes` it does nothing.
- **Bottom bar:** `Inicio`, `Reproductor` and `Favoritos`, each an icon with a label. The current section is marked (screen readers announce, for example, "Inicio, seleccionado"); on `Ajustes`, `Personalización` and `Preferencias` nothing is marked. A button leaves any page, settings included; pressing the current section's button does nothing. The app opens on `Inicio`.
- Only the content between the header and the bar scrolls.

`Iniciar sesión` and `Crear cuenta` keep the platform header with its back button. The platform's own tab bar is never shown. On Windows, pushed pages (Ajustes and its sub-pages, the login pages) also get the window's back arrow.

**Station cards** (Inicio and Favoritos) are taller than wide: logo, name (bold, up to two lines), details (country · language, then up to three tags, up to two lines; `Desconocido` for a missing country or language), and a row with the play button on the left and the heart on the right. The grid has one column for about every 170 units of width, and never fewer than two. Tapping the card or its play button plays the station; the heart never selects it.

**App icon:** the OpenWave logo (the play button with a cable and the "W" with radio waves) on white, in `src/OpenWave.App/Resources/AppIcon/appiconfg.png` over the white `appicon.svg`. The `MauiIcon` item scales it to 70% (`ForegroundScale="0.7"`) so Android's round and squircle icon shapes never cut it; Windows uses the same image for the `.exe`, the taskbar and Alt+Tab. After changing it, uninstall the app on Android (launchers cache icons) before deploying again.

**Icons** are original 24×24 single-colour drawings made for this project (`Resources/Images/ic_*.svg`, see `Icons-LICENSE.txt`, also shipped in the app's `Licences/` folder). They are tinted at runtime with the palette colours, and their size follows the title text size.

**Known limitation (Windows):** after a text size change in `Personalización`, icons on pages that were already open keep their previous size; the text resizes at once. Pages opened afterwards, and every page after restarting the app, show the icons at the new size. The CommunityToolkit tint draws each icon at the size it had when it was first drawn. Android resizes every icon immediately.

## Ajustes and Personalización

`Ajustes` opens from the gear. It shows two full-width cards, `PERSONALIZACIÓN` and `PREFERENCIAS`; each opens its page, and the back button (or the gear) returns to `Ajustes`.

`Personalización` works without an account; logging in or out does not change it.

- **Selectors:** `Tema` (every registered theme: `Moderno` and `Tocadiscos`), `Variante` (`Claro`, `Oscuro`; shown only for a theme with more than one), `Tipo de letra` (each font name is written in its own font) and `Tamaño del texto` (`Pequeño`, `Normal`, `Grande`, `Muy grande`, `Enorme`). The current choices are selected when the page opens.
- **No save button:** as the page says ("Los cambios se aplican y se guardan al momento."), every change applies at once to every page, the header and the bottom bar, and is stored on the device.
- **`Vista previa`:** a card with a sample title, body text, secondary text and button in the current theme, font and size.
- **`Restablecer valores por defecto`:** returns to `Moderno` `Claro`, `Ubuntu` and `Normal`.

## Preferencias

`Preferencias` stores default station filters on the device ("Se aplican como filtros en Inicio. «Todos» significa sin filtro."). It works without an account; logging in or out does not change it, and nothing is stored in the database.

- **Pickers:** `País`, `Idioma`, `Género` and `Tipo de contenido`. Each starts with `Todos` (no filter), followed by the options from the API (`/api/catalog/...`). The saved values are selected when the page opens. If a saved value is no longer in the list, it is still shown as selected and kept.
- **`Guardar`:** stores the four choices and shows `Preferencias guardadas.` It is available only once the lists have loaded.
- **`Restablecer`:** sets all four to `Todos` and stores that immediately. It works even when the lists cannot be loaded.
- **While loading:** `Cargando opciones...`
- **If the lists cannot be loaded:** `No hay conexión con el servidor.` (API not reachable) or `No se pudo obtener el catálogo de emisoras.` (Radio Browser unavailable), with `Reintentar`. The saved preferences are never changed by a failed load.

| Preference key | Value |
|---|---|
| `pref_country` | Country name, for example `Spain` |
| `pref_language` | Language, for example `spanish` |
| `pref_genre` | Genre tag, for example `jazz` |
| `pref_content_type` | `music`, `news`, `talk` or `sports` |

A key is removed when its picker is `Todos`.

## Themes and fonts

The app's look (theme, variant, font and text size) is set by `AppearanceService` when the app starts, before the first page appears, and it changes the whole app immediately. The choices are stored on the device with MAUI `Preferences`, never in the database, and logging in or out does not change them.

| Preference key | Default | Values |
|---|---|---|
| `appearance_theme` | `moderno` | `moderno` (Moderno), `tocadiscos` (Tocadiscos), `valvulas` (Radio de válvulas), `casete` (Casete portátil), `boombox` (Boombox), `mesadj` (Mesa de DJ), `mp3` (MP3), `rueda` (Clásico de rueda) |
| `appearance_theme_variant` | `light` | the palette id: `light` (Claro), `dark` (Oscuro), `baquelita`, `nogal`, `blanco`, `negro`. Kept while a one-palette theme (Tocadiscos, Casete portátil, Boombox, Mesa de DJ, MP3) is selected. A theme without the stored palette uses its palette of the same light or dark kind, so Moderno `Oscuro` → Radio de válvulas gives `Nogal`, and back gives `Oscuro` |
| `appearance_font` | `Ubuntu` | `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive`, `Pixelify Sans` |
| `appearance_font_scale` | `1.0` | `0.85` Pequeño, `1.0` Normal, `1.2` Grande, `1.4` Muy grande, `1.6` Enorme |

The previous version stored `white` (Blanco) or `black` (Negro) in `appearance_theme`. On the first start they become `moderno` with `light` or `dark`, and the new values are written back. Any other missing, unknown or corrupt value falls back to its default (`Moderno` `Claro`, `Ubuntu`, `Normal`). The app does not follow the operating system's light or dark setting.

### Themes

A theme is three things in `Resources/Themes/`:

- **Palettes** (one per variant): the colours. Every palette defines the same 14 keys: `PageBackgroundColor`, `SurfaceColor`, `InputBackgroundColor`, `DividerColor`, `SelectedItemBackgroundColor`, `PrimaryTextColor`, `SecondaryTextColor`, `ErrorTextColor`, `AccentColor`, `AccentTextColor`, `FavoriteIconColor`, `TabBarBackgroundColor`, `TabBarSelectedColor`, `TabBarUnselectedColor`.
- **A layout:** the page templates and the shared pieces: `HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`, `AjustesTemplate` and `SettingsPageTemplate`. Pages pick their template with `DynamicResource`, so switching theme swaps the layout.
- **An entry in `ThemeCatalog`.**

Pages, layouts and styles never contain literal colours, fonts or sizes: colours come only from the palettes, and the font family and sizes only from `Resources/Styles/Styles.xaml` (replaced at runtime by the appearance settings). Themes never set font families or text sizes. The single audio player (`MediaElement`) is created once with the window by `MediaElementAudioPlayer` (on Windows its 1×1 view sits in the window, outside every page; `Platforms/Windows/AudioHost.Windows.cs`), so no page or layout owns it and a theme change can never stop, duplicate or restart it.

`Personalización` does not use `SettingsPageTemplate`: the theme is chosen on that page, and moving its radio buttons into a new frame while it is shown makes them re-select the previous theme. It takes only `HeaderTemplate`, `BottomBarTemplate` and the layout's optional `SettingsPanelStyle`, so it still changes at once.

**Moderno** (`ModernoLayout.xaml`) has two variants:

- **Claro** (`ModernoLightPalette.xaml`): white background (`#FFFFFF`), near-black text (the colours of the former Blanco).
- **Oscuro** (`ModernoDarkPalette.xaml`): true black background (`#000000`), near-white text and a lighter accent (the colours of the former Negro).

Each palette also sets the light or dark mode of the native controls (text inputs, pickers and dialogs).

**Tocadiscos** (`TocadiscosLayout.xaml`, one palette `TocadiscosPalette.xaml`, dark) is a 1960s–70s record player: walnut wood, cream record sleeves, brass and vinyl black. Light text always sits on wood or on brass; the cream only carries logos and dark text.

- **Inicio and Favoritos:** a stack of record sleeves on the left (the focused station big, the next two smaller and dimmed) and an information panel on the right (name, details, heart, and the play state when it is the current station). Swipe, the mouse wheel or the Up/Down keys move one station at a time; pressing a smaller sleeve brings it to the front. Pressing the big sleeve plays the station without leaving the page (pressing it again stops it). The current station's sleeve has a brass outline and its record peeks out.
- **Reproductor:** a house button (Inicio) and the gear in the header; a turntable whose record label is the station logo and whose tonearm is the play/stop control; the information panel with the status line and `Reintentar`; and a record crate with your favourites (two compartments of three, six per page, arrows and swipe when there are more). There is no bottom bar and no previous/next buttons: the crate replaces them.
- **Ajustes:** the two cards drawn as cream sleeves with a peeking vinyl; Personalización and Preferencias sit on a framed wood panel.

The custom controls are UI only and live in `Views/Controls/`: `VinylDisc`, `Tonearm`, `SleeveStack`, `RecordCrate` (plus `PageAwareView` and `MotionSettings`). They get everything through bindable properties and never call a service. **Animations only observe `PlaybackService.State`** (through the view models): the tonearm lands in about 600 ms with a short settle and lifts in about 500 ms, and the record speeds up over 1.2 s and slows down over 1.8 s from its current angle. They stop while the page is hidden, and with reduced motion (Windows animations off, Android animator duration scale 0) the tonearm moves at once and the record stays still. All the Tocadiscos art is original and drawn with shapes in XAML (`Resources/Images/Tocadiscos/LICENSE.txt`, also shipped as `Licences/Tocadiscos-LICENSE.txt`).

**Radio de válvulas** (`ValvulasLayout.xaml`) is a 1940s–50s valve table radio, with two variants: **Baquelita** (`ValvulasBaquelitaPalette.xaml`, light: cream bakelite, dark brown text, bottle green) and **Nogal** (`ValvulasNogalPalette.xaml`, dark: walnut, ivory text, glowing green). The header and the bottom bar are a wooden strip; the bar has three round ivory buttons.

- **Inicio:** the stations sit along a glass tuning dial with a printed scale; the focused one is under the red needle in the lit window, with two neighbours on each side, and `emisora {n} de {total}` below. A swipe on the dial, the tuning knob (a press steps to the next station; on Windows Left/Right step and Enter or Space plays), the mouse wheel on Windows and pressing a neighbour move one station at a time. Below, on the speaker grille, the information panel shows the name, details, heart, the play state and `Escuchar`, which plays without leaving the page (`Apagar` while it plays). Pressing the focused station on the dial does the same. The current station is marked with the lit fill (Baquelita) or an outline (Nogal).
- **Favoritos:** the account row, then the radio log: one dial label per favourite (logo, name, details, play state, `Escuchar` and the heart that removes it). Pressing a label plays it in place.
- **Reproductor:** the house and the gear in the header; the dial with only the current station, the magic eye and the power knob (`Reproducir` / `Detener`); the panel with the logo, details, heart, status line and `Reintentar`; and five ivory preset keys with your favourites (`Presintonía {n}: {nombre}`), the current one pressed down, with arrow keys and `1/2` when there are more. No bottom bar. When the radio is turned on, the dial window glows in over 1.5 s and the magic eye closes over 0.8 s; on stop both return in 0.6 s. With reduced motion, or a page shown while playing, the final state appears at once.
- **Ajustes:** the two entries as wide ivory keys; Personalización and Preferencias sit on a rounded cloth panel.

**Casete portátil** (`CaseteLayout.xaml`, one palette `CaseteMetalPalette.xaml` "Metal azul", dark) is a 1980s portable cassette player: blue metal, a dark cassette window, brushed silver keys and paper, and orange foam. The bottom bar is three silver keys `INICIO`, `REPRODUCTOR` and `FAVORITAS`; the current one is pushed in.

- **Inicio:** the stations are cassette spines in a shoebox (logo, name on the label band, heart). Pressing a spine pulls it out with an insert below it (details, play state and `Reproducir`, which plays without leaving the page and reads `Detener` while it plays); pressing the pulled-out spine again does the same. Only one spine is out at a time, and a new search goes back to the first. On Windows, Up/Down move between spines and Enter plays. The current station's spine has the orange outline.
- **Favoritos:** the account row, then the silver J-card `Mis favoritas` with `CARA A` and `CARA B`, five numbered favourites each (`Cara A, pista {n}: {nombre}`). Pressing a line plays it in place, its button removes it, and `Dar la vuelta` shows the next ten (`1/2`); on Windows Left/Right change the side. The current line is an orange band.
- **Reproductor:** the keys `REW` (`Favorita anterior`), `PLAY` (`Reproducir`, stays pushed in while playing), `STOP` (`Detener`, only while playing) and `FF` (`Favorita siguiente`); the cassette window with the station logo and name on its label and two reels that turn while playing (half a turn per second, speeding up over 0.8 s and slowing down over 1 s); the panel with details, heart, status line and `Reintentar`; and side A of the J-card with five favourites. No bottom bar.
- **Ajustes:** two big silver keys; Personalización and Preferencias sit on a brushed panel.

**Boombox** (`BoomboxLayout.xaml`, one palette `BoomboxCromadoPalette.xaml` "Cromado", dark) is a 1980s street boombox: black plastic, gunmetal, chrome trim, hot pink keys, cyan display digits and yellow hearts. The header has a chrome trim line; the bottom bar has three rubber buttons, the current one with a lit cyan LED.

- **Inicio:** the stations are rows on a dark tuner display (`TUNING · {n} emisoras`) between two speakers: beside it on windows at least 720 wide, as short strips above and below it on narrower ones (a phone in portrait). Pressing a row plays it without leaving the page; pressing the current row again stops it. The current row has the pink outline and a pink `▶`. On Windows, Up/Down move between rows and Enter plays.
- **Favoritos:** the account row, a closed cassette door labelled `MIS FAVORITAS` and the same display with only the favourites; the heart removes one.
- **Reproductor:** the carrying handle and antenna above the header (house and gear); two speakers that pulse while playing, framing a cyan level meter with a pink peak; the display with the logo, the name in large digits, details, status line and `Reintentar`; the keys `◀◀` (`Favorita anterior`), `▶ PLAY` / `■ STOP` and `▶▶` (`Favorita siguiente`) and the heart; and the memories `M1`–`M6` (`Memoria {n}: {nombre}`), the current one lit cyan, with arrows and `1/2` when there are more (Left/Right move between them on Windows). No bottom bar.
- **Ajustes:** two wide rubber buttons on a gunmetal panel; Personalización and Preferencias sit inside a chrome border.

**Mesa de DJ** (`MesaDjLayout.xaml`, one palette `MesaDjClubPalette.xaml` "Club", dark) is a club DJ booth with a single deck: a near-black body, dark screens, a cyan accent and orange hearts. Every visible label is Spanish (`CARGAR`, `ATRÁS`, `SIGUIENTE`, `EN DIRECTO`); the `FORMATO` column shows the station's first tag as the API gives it. The header has an `EN DIRECTO` tag under the title that lights while a station is buffering or playing, on every page: it reads `PlaybackIndicator`, which `PlaybackService` keeps up to date for page chrome whose view model does not know the play state. The bottom bar is three flat buttons with a cyan underline under the current section.

- **Inicio:** a track browser with the headings `NOMBRE`, `PAÍS` and `FORMATO`; each row has the logo, name, country and format (one secondary line on windows narrower than 720, such as a phone), the heart and `CARGAR`. Pressing a row or `CARGAR` plays it without leaving the page; on the current station it stops it. The current row has the teal background and a cyan bar on its left. Rows are at least 48 units tall. On Windows, Up/Down move between rows and Enter loads.
- **Favoritos:** the account row, a tab `Favoritas ({n})` and the same browser with only the favourites; the heart removes one.
- **Reproductor:** the house and the gear; the jog wheel with the logo in its centre (it turns like the Tocadiscos record), a vertical level meter, the station, details, heart, status line and `Reintentar`; `ATRÁS` (`Favorita anterior`), the round play/stop button (`Reproducir` / `Detener`) and `SIGUIENTE` (`Favorita siguiente`); and eight pads in a 4×2 grid (`Pad {n}: {nombre}`), the current one orange and glowing slowly while playing, with arrows and `1/2` when there are more (the arrow keys move between pads on Windows). No bottom bar.
- **Ajustes:** two large pads; Personalización and Preferencias sit on a screen panel.

**MP3** (`Mp3Layout.xaml`, one palette `Mp3AzulPalette.xaml` "Azul", dark) is a mid-2000s MP3 stick with a blue back-lit LCD. The header is the LCD's status line with a decorative battery; the bottom bar is three rubber buttons `INICIO`, `REPRODUCTOR` and `FAVORITAS`, the current one light blue. On the inverted cursor row only the body text colour is readable, so every text on an LCD line uses it.

- **Inicio:** the stations are lines on the LCD under `/EMISORAS/`, with the counter `{n}/{total}`. The cursor line is inverted, shows `▶` and the characteristics; the current station's line shows `♪` and its play state (from `PlaybackIndicator`, because the line is shared with Favoritos). `▲` (`Subir`), `▼` (`Bajar`) and `OK` (`Aceptar: reproducir {nombre}`) move the cursor and play in place; touching a line moves the cursor there and touching it again plays it; `OK` on the current station stops it. A new search puts the cursor on the first line. On Windows, Up/Down move and Enter plays.
- **Favoritos:** the email and `Cerrar sesión` on the LCD, then `/FAVORITAS/` with the same cursor and buttons; the heart removes the line and the cursor moves to the next (or the previous after the last).
- **Reproductor:** the house, battery and gear; the LCD now-playing screen with the logo in a framed square, the name (it scrolls sideways at 40 units/s with 1 s pauses when it does not fit; with reduced motion it wraps to two lines), the characteristics, an 8-bar equaliser and the status line with `Reintentar`; and the pad `◀◀` (`Favorita anterior`), `▶❙❙` (`Reproducir` / `Detener`), `▶▶` (`Favorita siguiente`) and the heart. No bottom bar and no other favourites control.
- **Ajustes:** an LCD menu with the lines `Preferencias` and `Personalización`; the settings pages sit inside an LCD bezel.

**Clásico de rueda** (`RuedaLayout.xaml`) is a mid-2000s pocket player with a screen of menus and a round click wheel, with two variants: **Blanco** (`RuedaBlancoPalette.xaml`, light: white body, grey-blue screen, blue highlighted row) and **Negro** (`RuedaNegroPalette.xaml`, dark: black body, dark screen, dark blue highlighted row). The highlighted row's text is `AccentTextColor` (white) in both; for that, Negro's accent is a deep blue with white text instead of the brief's light blue. Inicio, Reproductor and Favoritos are a device centred with a maximum width: the screen on top with its title bar (a play-state glyph, the title and the gear), the wheel under it (its size never depends on the text size), and a slim bottom bar under the wheel on every page.

- **Wheel:** `MENÚ` (`Menú`) at the top, `◀◀` on the left, `▶▶` on the right, play/pause at the bottom and the centre button, all real buttons; sliding a finger around the ring (`Rueda: desliza en círculo para moverte por la lista`) moves the cursor one row about every 24°. The buttons give an 80 ms press feedback. On Windows, Up/Down, the mouse wheel and Enter work on the menu, Space is play/pause, Left/Right are `◀◀`/`▶▶` and Escape is `MENÚ` (except while typing in the search box).
- **Inicio:** the search row and the filters bar, then the Emisoras menu (name, heart, `›`). The centre (`Seleccionar`) plays the cursor's station in place (or stops the current one), play/pause (`Reproducir o pausar`) toggles the current station, `◀◀`/`▶▶` (`Anterior`/`Siguiente`) move one screen of rows, and touching a row moves the cursor (touching it again plays it). The current station's row shows ▶ or ❙❙. `MENÚ` is disabled here.
- **Favoritos:** the account row first, then the Favoritas menu with the same wheel; the heart removes a row. Logged out: `Inicia sesión para ver tus favoritas.` and an `Iniciar sesión` row. `MENÚ` opens Inicio.
- **Reproductor:** the Reproduciendo screen (the logo large and square, name, details, a full bar labelled `En directo`, status line and `Reintentar`). On the wheel, play/pause is `Reproducir` / `Detener`, `◀◀`/`▶▶` are `Favorita anterior` / `Favorita siguiente`, the centre is the heart, the ring does nothing and is not read, and `MENÚ` opens Inicio.
- **Ajustes:** a screen menu with `Preferencias ›` and `Personalización ›`; the settings pages keep the title bar and the slim bar.

Shared controls added for the retro themes, also UI only and in `Views/Controls/`: `SpinDriver` (the record's speed-up and slow-down, now used by `VinylDisc`), `PagedSlots` (a paged grid of favourites with its own `PagedSlot` numbers), `CursorList` (a list with a cursor), `LevelMeter` (a decorative level meter) and `ButtonSwipe` (reads swipes that start on a button on Android). Radio de válvulas adds `TuningDial` (with `DialSlot` and `TuningKnob`), `MagicEye` and `WarmUpGlow`; Casete portátil adds `CassetteReels` (on `SpinDriver`); Boombox adds `SpeakerCone` (a 450 ms pulse while playing, still with reduced motion) and uses `LevelMeter`; Mesa de DJ adds `JogWheel` (on `SpinDriver`) and `GlowPulse` (the current pad's 1 s glow); MP3 adds `MarqueeLabel` (screen readers read its full text once); Clásico de rueda adds `ClickWheel` (the ring reads the finger's angle from the touch or pointer position on each platform). The art of the retro themes is original and drawn with shapes (`Resources/Images/Themes/LICENSE.txt`, also shipped as `Licences/Themes-LICENSE.txt`).

### Contrast

Every text or icon colour has a contrast ratio of at least 4.5:1 with the background it is drawn on (WCAG relative luminance):

| Where | Pair | Claro | Oscuro |
|---|---|---|---|
| Titles, body text, header title | `PrimaryTextColor` on `PageBackgroundColor` | 18.37:1 | 18.79:1 |
| Card names, Ajustes cards | `PrimaryTextColor` on `SurfaceColor` | 18.37:1 | 16.75:1 |
| Name on the selected card | `PrimaryTextColor` on `SelectedItemBackgroundColor` | 15.18:1 | 12.38:1 |
| Search text | `PrimaryTextColor` on `InputBackgroundColor` | 16.44:1 | 15.20:1 |
| Secondary text, email, notices | `SecondaryTextColor` on `PageBackgroundColor` | 6.80:1 | 10.22:1 |
| Card details | `SecondaryTextColor` on `SurfaceColor` | 6.80:1 | 9.11:1 |
| Details on the selected card | `SecondaryTextColor` on `SelectedItemBackgroundColor` | 5.62:1 | 6.74:1 |
| Search placeholder | `SecondaryTextColor` on `InputBackgroundColor` | 6.08:1 | 8.27:1 |
| Error messages | `ErrorTextColor` on `PageBackgroundColor` | 6.54:1 | 9.20:1 |
| Button text, play icon | `AccentTextColor` on `AccentColor` | 5.71:1 | 9.07:1 |
| Heart on a card | `FavoriteIconColor` on `SurfaceColor` | 5.71:1 | 8.08:1 |
| Heart on the selected card | `FavoriteIconColor` on `SelectedItemBackgroundColor` | 4.72:1 | 5.97:1 |
| Heart in Reproductor | `FavoriteIconColor` on `PageBackgroundColor` | 5.71:1 | 9.07:1 |
| Marked bottom bar button | `TabBarSelectedColor` on `TabBarBackgroundColor` | 5.34:1 | 8.54:1 |
| Other bottom bar buttons | `TabBarUnselectedColor` on `TabBarBackgroundColor` | 6.36:1 | 7.11:1 |

Tocadiscos:

| Where | Pair | Contrast |
|---|---|---|
| Page text, Personalización, Preferencias | `PrimaryTextColor` on `PageBackgroundColor` | 12.31:1 |
| Information panel, preview card | `PrimaryTextColor` on `SurfaceColor` | 9.76:1 |
| Search box | `PrimaryTextColor` on `InputBackgroundColor` | 15.99:1 |
| Details, captions | `SecondaryTextColor` on `PageBackgroundColor` / `SurfaceColor` | 8.71:1 / 6.91:1 |
| Errors | `ErrorTextColor` on `PageBackgroundColor` / `SurfaceColor` | 7.58:1 / 6.01:1 |
| Brass buttons | `AccentTextColor` on `AccentColor` | 7.82:1 |
| Ajustes cards (cream) | `AccentTextColor` on `TabBarUnselectedColor` | 12.87:1 |
| Header title, play state, marked bar button | `TabBarSelectedColor` on `TabBarBackgroundColor` | 10.68:1 |
| Other bar buttons | `TabBarUnselectedColor` on `TabBarBackgroundColor` | 12.74:1 |
| Hearts | `FavoriteIconColor` on `SurfaceColor` | 6.63:1 |

Radio de válvulas. Text on the wooden strip uses the bar colours, and text on the ivory keys uses the wood colour (`PrimaryTextColor` would be 1.16:1 on the wood in Baquelita and 1.15:1 on the keys in Nogal). The lit fill of the current station is drawn only in Baquelita; Nogal marks it with an outline, because its selected colour is too light for text.

| Where | Pair | Baquelita | Nogal |
|---|---|---|---|
| Page text, Personalización, Preferencias | `PrimaryTextColor` on `PageBackgroundColor` | 13.03:1 | 13.85:1 |
| Panel, radio log, lit window on the dial | `PrimaryTextColor` on `SurfaceColor` | 14.61:1 | 11.77:1 |
| Dial labels, search box, logged-out text on blank keys | `PrimaryTextColor` on `InputBackgroundColor` | 11.21:1 | 15.63:1 |
| Current station on the lit fill (Baquelita only) | `PrimaryTextColor` on `SelectedItemBackgroundColor` | 10.54:1 | — |
| Details on the lit fill (Baquelita only) | `SecondaryTextColor` on `SelectedItemBackgroundColor` | 5.57:1 | — |
| Error on the lit fill (Baquelita only) | `ErrorTextColor` on `SelectedItemBackgroundColor` | 4.68:1 | — |
| Details, captions, position | `SecondaryTextColor` on `PageBackgroundColor` / `SurfaceColor` | 6.89:1 / 7.73:1 | 9.03:1 / 7.67:1 |
| Scale numbers, empty-favourites text on blank keys | `SecondaryTextColor` on `InputBackgroundColor` | 5.93:1 | 10.19:1 |
| Play state in the panel | `AccentColor` on `SurfaceColor` | 5.59:1 | 8.28:1 |
| Errors | `ErrorTextColor` on `PageBackgroundColor` / `SurfaceColor` | 5.79:1 / 6.49:1 | 8.71:1 / 7.40:1 |
| Green buttons (`Escuchar`, `Reintentar`) | `AccentTextColor` on `AccentColor` | 6.34:1 | 9.15:1 |
| Header title, marked bar label | `TabBarSelectedColor` on `TabBarBackgroundColor` | 8.78:1 | 12.10:1 |
| Other bar labels | `TabBarUnselectedColor` on `TabBarBackgroundColor` | 10.52:1 | 13.06:1 |
| Ivory keys and bar buttons (text and icons) | `TabBarBackgroundColor` on `TabBarUnselectedColor` | 10.52:1 | 13.06:1 |
| Pressed preset key, marked bar button | `TabBarBackgroundColor` on `TabBarSelectedColor` | 8.78:1 | 12.10:1 |

Casete portátil. Text on the silver keys and the J-card is dark, and orange is never text on the blue panels (`AccentColor` on `SurfaceColor` is 3.88:1). The current J-card line is an orange band with dark text, because an orange outline on the silver would be 1.71:1.

| Where | Pair | Contrast |
|---|---|---|
| Page text, Personalización, Preferencias | `PrimaryTextColor` on `PageBackgroundColor` | 10.58:1 |
| Spine names, inserts, information panel, play state | `PrimaryTextColor` on `SurfaceColor` | 8.32:1 |
| Search box | `PrimaryTextColor` on `InputBackgroundColor` | 16.11:1 |
| Details, captions | `SecondaryTextColor` on `PageBackgroundColor` / `SurfaceColor` | 7.41:1 / 5.83:1 |
| Errors | `ErrorTextColor` on `PageBackgroundColor` / `SurfaceColor` | 6.75:1 / 5.31:1 |
| Orange buttons and the PLAY key, current J-card line | `AccentTextColor` on `AccentColor` | 7.52:1 |
| Silver keys, J-card text, cassette label | `AccentTextColor` on `TabBarUnselectedColor` | 12.90:1 |
| Pushed-in bar key | `AccentTextColor` on `TabBarSelectedColor` | 9.78:1 |
| Header title and icons | `TabBarUnselectedColor` on `TabBarBackgroundColor` | 10.67:1 |
| Hearts | `FavoriteIconColor` on `SurfaceColor` / `InputBackgroundColor` | 5.04:1 / 9.76:1 |

Boombox:

| Where | Pair | Contrast |
|---|---|---|
| Page text, Personalización, Preferencias, header title | `PrimaryTextColor` on `PageBackgroundColor` | 16.10:1 |
| Rubber buttons, filter bar, cursor row | `PrimaryTextColor` on `SurfaceColor` | 12.84:1 |
| Search box, Ajustes buttons | `PrimaryTextColor` on `InputBackgroundColor` | 17.87:1 |
| Station names, display name, `TUNING`, play state | `TabBarSelectedColor` on `InputBackgroundColor` / `SurfaceColor` | 11.97:1 / 8.60:1 |
| Details | `SecondaryTextColor` on `InputBackgroundColor` / `SurfaceColor` / `PageBackgroundColor` | 10.34:1 / 7.43:1 / 9.32:1 |
| Errors | `ErrorTextColor` on `InputBackgroundColor` / `PageBackgroundColor` | 8.61:1 / 7.76:1 |
| The `▶` of the current row | `AccentColor` on `InputBackgroundColor` | 6.40:1 |
| Pink keys and buttons | `AccentTextColor` on `AccentColor` | 6.50:1 |
| Memories, `MIS FAVORITAS` label | `AccentTextColor` on `TabBarUnselectedColor` | 12.42:1 |
| Current memory | `AccentTextColor` on `TabBarSelectedColor` | 12.16:1 |
| Header icons, arrows | `TabBarUnselectedColor` on `TabBarBackgroundColor` | 11.91:1 |
| Hearts | `FavoriteIconColor` on `InputBackgroundColor` / `SurfaceColor` | 13.62:1 / 9.78:1 |

Mesa de DJ:

| Where | Pair | Contrast |
|---|---|---|
| Page text, Personalización, Preferencias, header title | `PrimaryTextColor` on `PageBackgroundColor` | 17.06:1 |
| Browser rows, deck screen, flat buttons, `Favoritas ({n})` | `PrimaryTextColor` on `SurfaceColor` | 14.87:1 |
| Current row | `PrimaryTextColor` / `SecondaryTextColor` on `SelectedItemBackgroundColor` | 11.83:1 / 6.07:1 |
| Search box, pad names, Ajustes pads | `PrimaryTextColor` on `InputBackgroundColor` | 18.02:1 |
| Details, columns, headings | `SecondaryTextColor` on `SurfaceColor` / `PageBackgroundColor` | 7.63:1 / 8.75:1 |
| Errors | `ErrorTextColor` on `SurfaceColor` / `PageBackgroundColor` | 6.63:1 / 7.60:1 |
| Play state, pad numbers | `AccentColor` on `SurfaceColor` / `InputBackgroundColor` | 8.92:1 / 10.81:1 |
| `CARGAR`, the lit `EN DIRECTO` tag, the round button | `AccentTextColor` on `AccentColor` | 9.69:1 |
| Current pad | `AccentTextColor` on `FavoriteIconColor` | 7.72:1 |
| Marked / other bar items, dim `EN DIRECTO` | `TabBarSelectedColor` / `TabBarUnselectedColor` on `TabBarBackgroundColor` | 10.62:1 / 6.66:1 |
| Hearts | `FavoriteIconColor` on `SurfaceColor` / `SelectedItemBackgroundColor` | 7.11:1 / 5.65:1 |

MP3. On the cursor row (`SelectedItemBackgroundColor`) `SecondaryTextColor` would be 3.94:1, `ErrorTextColor` 3.93:1 and `AccentColor` 4.28:1, so lines use only `PrimaryTextColor`.

| Where | Pair | Contrast |
|---|---|---|
| LCD lines, status line title, menu lines | `PrimaryTextColor` on `SurfaceColor` | 11.54:1 |
| Cursor row (name, `▶`, characteristics, play state) | `PrimaryTextColor` on `SelectedItemBackgroundColor` | 6.64:1 |
| Page text, Personalización, Preferencias | `PrimaryTextColor` on `PageBackgroundColor` | 12.65:1 |
| Search box | `PrimaryTextColor` on `InputBackgroundColor` | 15.43:1 |
| Path, counter, characteristics on the now-playing screen | `SecondaryTextColor` on `SurfaceColor` | 6.85:1 |
| Play state on the now-playing screen | `AccentColor` on `SurfaceColor` | 7.45:1 |
| Errors | `ErrorTextColor` on `SurfaceColor` / `PageBackgroundColor` | 6.83:1 / 7.49:1 |
| Rubber buttons | `PrimaryTextColor` on `DividerColor` | 8.43:1 |
| `OK`, `▶❙❙`, current bar button | `AccentTextColor` on `AccentColor` | 9.41:1 |
| Hearts | `FavoriteIconColor` on `SurfaceColor` / `SelectedItemBackgroundColor` / `TabBarBackgroundColor` | 10.11:1 / 5.82:1 / 12.83:1 |

Clásico de rueda. On the highlighted row the name, glyph, heart and `›` use `AccentTextColor`.

| Where | Pair | Blanco | Negro |
|---|---|---|---|
| Screen text, menu rows, title bar | `PrimaryTextColor` on `SurfaceColor` | 15.93:1 | 16.74:1 |
| Highlighted row | `AccentTextColor` on `SelectedItemBackgroundColor` | 5.71:1 | 7.06:1 |
| Page text, Personalización, Preferencias | `PrimaryTextColor` on `PageBackgroundColor` | 16.90:1 | 13.83:1 |
| Search box | `PrimaryTextColor` on `InputBackgroundColor` | 18.61:1 | 11.78:1 |
| Details, `En directo`, email | `SecondaryTextColor` on `SurfaceColor` | 5.87:1 | 8.42:1 |
| Errors | `ErrorTextColor` on `SurfaceColor` | 5.60:1 | 8.23:1 |
| Buttons | `AccentTextColor` on `AccentColor` | 5.71:1 | 5.73:1 |
| Wheel labels | `TabBarUnselectedColor` on `InputBackgroundColor` | 6.43:1 | 5.92:1 |
| Slim bar: other / marked items | `TabBarUnselectedColor` / `TabBarSelectedColor` on `TabBarBackgroundColor` | 5.84:1 / 5.18:1 | 7.88:1 / 7.06:1 |
| Hearts | `FavoriteIconColor` on `SurfaceColor` / `InputBackgroundColor` | 4.89:1 / 5.71:1 | 7.54:1 / 5.31:1 |

### Fonts and text size

The six fonts are bundled in `Resources/Fonts/`, each with its licence file next to it. The licences are also copied into the app's `Licences/` folder.

| Font | Files | Licence |
|---|---|---|
| Ubuntu (default) | `Ubuntu-Regular.ttf`, `Ubuntu-Bold.ttf` | Ubuntu Font Licence (`Ubuntu-UFL.txt`) |
| Dosis | `Dosis-Variable.ttf` | SIL OFL (`Dosis-OFL.txt`) |
| Caladea | `Caladea-Regular.ttf` | SIL OFL (`Caladea-OFL.txt`) |
| Cormorant Garamond | `CormorantGaramond-Regular.ttf`, `CormorantGaramond-Bold.ttf` | SIL OFL (`CormorantGaramond-OFL.txt`) |
| Recursive | `Recursive-Variable.ttf` | SIL OFL (`Recursive-OFL.txt`) |
| Pixelify Sans | `PixelifySans-Variable.ttf` | SIL OFL (`PixelifySans-OFL.txt`) |

The files come from Google's font repository, except the static Cormorant Garamond files, which come from the official Cormorant project. Bold text uses the font's bold file when it has one; the variable fonts contain their own bold weight.

The text size scales four base sizes: caption 12, body 16, subtitle 18 and title 24. For example, `Enorme` makes body text 25.6 and titles 38.4. Icons are as large as a title letter (24 at `Normal`), inside buttons of at least 48 units. The header of `Iniciar sesión` and `Crear cuenta` is drawn by the platform, so `ShellChrome` gives it a title styled like the rest of the app. On Android `ShellChrome` (with a small file under `Platforms/Android/`) also recolours the circles of the radio buttons after a theme change, because Android otherwise keeps the colours of the theme the app started with.

### How to add a theme

1. **Palettes:** copy `Resources/Themes/ModernoLightPalette.xaml` and its `.xaml.cs` for each variant (for example `RetroPalette.xaml`), rename the class and change the colours. Keep all 14 keys.
2. **Layout:** copy `ModernoLayout.xaml` (or `TocadiscosLayout.xaml`) and its `.xaml.cs` (for example `RetroLayout.xaml`), rename the class, and change the templates as you like. Keep every template key and the bindings and commands they use (the page view models, `AppNavigation` for the gear and the bar, the `ContentPresenter` in `SettingsPageTemplate`); `SettingsPanelStyle` is optional. Use only palette keys for colours and the named styles for text: never set a font family or a font size. The controls in `Views/Controls/` can be reused.
3. **Registry:** add one entry to `ThemeCatalog.All` in `ThemeCatalog.cs`:

   ```csharp
   new("retro", "Retro", typeof(RetroLayout),
   [
       new("dark", "Oscuro", typeof(RetroPalette), AppTheme.Dark),
   ]),
   ```

   The last value of each palette tells native controls whether to use their light or dark look. With a single palette, `Personalización` hides the `Variante` choice.

No page, view model, service or style needs to change; the theme appears in `Personalización` at once.

## Manual check list

1. Start the API and open `http://localhost:5080/api/health`: `status` is `ok` and `timeUtc` is an ISO 8601 UTC timestamp.
2. Run the app on Windows or the Android emulator. Inicio lists up to 50 real stations.
3. Search `jazz`: every listed name contains "jazz". Press `Limpiar`: the box is empty and the top stations return. Search a nonsense name: `No se encontraron emisoras.`
4. Use the bottom bar to switch between Inicio, Reproductor and Favoritos: the current one is marked. Press the current section's button: nothing changes. Press the gear: `AJUSTES` opens with no bar button marked; press the gear again: nothing changes.
5. Inicio shows portrait cards: logo, name, details, play button and heart; cards in a row have the same height. Widen the Windows window: more columns appear; narrow it: never fewer than two. Tap a card (or its play button): Reproductor opens with its large logo (or the placeholder), name and details, the status goes `Cargando…` → `Reproduciendo`, and you hear audio. Back in Inicio, that card is highlighted; selecting another one moves the highlight.
6. In Reproductor, press the stop icon (`Detener`): audio stops, the status shows `Detenido` and the centre shows the play icon. Press it (`Reproducir`): it plays again. Restart the app and open Reproductor first: `Selecciona una emisora en Inicio` and the play/stop control is disabled.
7. Switch to Favoritos while audio plays: the audio keeps playing. Return to Reproductor: the same station shows `Reproduciendo`. Minimise the app: the audio stops. Select another station in Inicio: the new station starts and the old one does not keep playing.
8. Let Inicio load the list, then turn off the computer's internet connection (on the emulator: turn on airplane mode) and select a station: within 15 seconds the status shows `Error`, `No se pudo reproducir esta emisora.` and `Reintentar`. The app keeps working. Turn the connection back on and press `Reintentar`: the station plays.
9. Stop the API and relaunch the app: Inicio shows `No hay conexión con el servidor.` with a `Reintentar` button, and the app keeps working. Start the API again and press `Reintentar`: the list loads without restarting the app.
10. Run the API with two invalid mirrors and search in the app: Inicio shows `No se pudo obtener el catálogo de emisoras.` For example:

    ```powershell
    $env:RadioBrowser__Servers__0 = "https://at1.api.radio-browser.info"
    $env:RadioBrowser__Servers__1 = "https://nl1.api.radio-browser.info"
    dotnet run --project src/OpenWave.Api
    ```

    Close that terminal (or remove both variables) afterwards to return to the configured mirrors.
11. Open Favoritos while logged out: it shows `Inicia sesión para ver tus favoritas.` and `Iniciar sesión`. Log in or create an account: you return to Favoritos, which shows your email and `Cerrar sesión`. Press `Cerrar sesión`: the logged-out state returns. Restart the app: it starts logged out.
12. Log in. In Inicio press the outline heart on a card: it fills and the page does not change. Play a station and press its outline heart in Reproductor: it fills and the audio keeps playing.
13. Open Favoritos: your email and `Cerrar sesión` are in one row under the header, and both stations are listed as cards, newest first, with filled hearts; the playing one is highlighted. Tap one: Reproductor opens and plays it. Press the filled heart of the other: it disappears, and its heart in Inicio is an outline again. Remove the last one: `Aún no tienes emisoras favoritas.`
14. Log out and press a heart in Inicio: `Iniciar sesión` opens. Go back: nothing is added. Press it again and log in: you return to Inicio with that heart filled.
15. Stop the API and press a heart while logged in: `No se pudo actualizar tus favoritas.` and the heart goes back. Start the API with an unreachable database (see item 10, using `$env:ConnectionStrings__Default = "Server=<HOST>;Database=2DAM_alumno19;User ID=<USER>;Password=<PASSWORD>;SslMode=Preferred;Connect Timeout=5;"`; keep `Connect Timeout=5` so the API answers before the app's 10-second timeout) and open Favoritos: `No se pudo acceder a la base de datos.` and `Reintentar`. Start the API normally and press `Reintentar`: the list loads.
16. Press `Cerrar sesión`: every heart in Inicio and Reproductor is an outline.
17. Open `Ajustes` with the gear: it shows the cards `PERSONALIZACIÓN` and `PREFERENCIAS`. Open each one: its header reads `PERSONALIZACIÓN` / `PREFERENCIAS` and no bar button is marked. Go back (Android back button, the window's back arrow on Windows, or the gear): you return to `Ajustes`, and going back once more leaves `Ajustes` (there is never a second one). From `Personalización` press `Favoritos` in the bar: Favoritos opens and is marked.
18. In `Personalización` select `Oscuro`, then `Pixelify Sans`, then `Enorme`: the page, the header, the bar and the icons change at once, and so do Inicio, Reproductor, Favoritos, `Iniciar sesión` and `Crear cuenta`. The text inputs turn dark. Your session, favourites, selected station and preferences are unchanged, and a station still plays normally (one stream only). With `Enorme` on a phone, no header title or text is cut and Reproductor's controls are reached by scrolling while the header and bar stay fixed.
19. Close and reopen the app: it starts with the same theme, font and size, and `Personalización` shows them selected. Log in and out: nothing changes.
20. Press `Restablecer valores por defecto`: the app returns to `Moderno` `Claro`, `Ubuntu` and `Normal`.
21. With every preference at `Todos`, Inicio shows no filter bar. In `Ajustes` → `Preferencias` choose `Spain` as `País` and press `Guardar`: `Preferencias guardadas.` Open Inicio: the list reloads with Spanish stations only, under `Filtros activos` / `Spain`. Search `radio`: every result contains "radio" and is from Spain.
22. Press `Desactivar`: the list is unfiltered and the bar reads `Preferencias desactivadas` with `Activar`; `Preferencias` still shows `Spain`. Close and reopen the app: the filter is active again. Press `Activar` after another `Desactivar`: the list is filtered again.
23. Choose `Andorra` and `Deportes` and save: Inicio shows `No se encontraron emisoras con tus preferencias.` with `Desactivar`. Favoritos still lists every favourite, and they still play in Reproductor.
24. Stop the API and open `Preferencias`: `No hay conexión con el servidor.` and `Reintentar`, and the saved values are not lost. Start the API and press `Reintentar`: the pickers show the saved values. Press `Restablecer`: all four go back to `Todos`.
25. Log in with three favourites (A newest, then B, then C) and play B from Favoritos. In Reproductor press next: C plays; next again: A (wrap); previous: C. Play a station that is not a favourite: next plays A, previous plays C. Remove two favourites so only the current one is left: next and previous change nothing. Remove it too: previous shows `Aún no tienes emisoras favoritas.` Log out and press next: `Iniciar sesión` opens; go back: nothing is added.
26. Migration: close the app, set `appearance_theme` to `black` in the preferences file (Windows: `%LOCALAPPDATA%\User Name\com.openwave.app\Settings\preferences.dat`) and start the app: it opens in `Moderno` `Oscuro` and the file now holds `moderno` and `dark`. With `white` it opens in `Claro`; with an unknown value it opens in `Moderno` `Claro` without crashing.
27. Keyboard (Windows): with Tab you reach the gear, the search box, `Buscar`, `Limpiar búsqueda`, each card's play button and heart, and the bottom bar; the focused control shows an outline, and Enter or Space activates it. On a card's play button, Enter opens Reproductor and plays the station.
28. Screen reader (Narrator on Windows, TalkBack on Android): the gear is read as `Ajustes`, a card's buttons as `Reproducir <name>` and `Añadir a favoritas` / `Quitar de favoritas`, Reproductor's as `Emisora favorita anterior`, `Reproducir` / `Detener` and `Emisora favorita siguiente`, and the marked bar button with `, seleccionado`.
29. Tocadiscos: in `Personalización` select `Tocadiscos` while a station plays: every page changes at once, `Variante` disappears, and the audio keeps playing. Select `Moderno` again: the variant you had comes back. Restart the app: it opens in the last theme chosen.
30. Tocadiscos Inicio: the wheel, Up/Down, swipe and pressing a smaller sleeve move the stack one station at a time and the panel follows. Press the big sleeve: it plays and Inicio stays on screen (`Reproduciendo` in the panel); press it again: it stops. The playing station's sleeve has the brass outline and the peeking record. Search: the first result is focused.
31. Tocadiscos Favoritos: remove the focused favourite with its heart: the next one is focused (the previous one when it was the last).
32. Tocadiscos Reproductor: open it while playing: the tonearm is already on the record and the record is turning. Press the tonearm: it lifts and the record slows down to a stop; press it again: it lands and the station plays. A broken stream lifts the arm and shows `Error` and `Reintentar`. The house opens Inicio and the gear Ajustes; there is no bottom bar.
33. Tocadiscos crate: logged out it reads `Inicia sesión para ver tus favoritas` and opens the login page. With 8 favourites: 6 records, arrows and `1/2`; the next arrow (or a swipe) shows the other 2. Pressing a record plays it, and its record is raised and outlined. On Windows, Right/Left move between records and Enter plays.
34. Reduced motion: with Windows animations turned off (Settings > Accessibility > Visual effects) or Android's animator duration scale at 0, the tonearm moves at once and the record does not turn.
35. Radio de válvulas: in `Personalización` select it while a station plays: every page changes at once, `Variante` shows `Baquelita` and `Nogal`, and the audio keeps playing. From Moderno `Oscuro` it opens in `Nogal`; choose `Baquelita` and go back to Moderno: it opens in `Claro`.
36. Radio de válvulas Inicio: a swipe on the dial, a press on the knob, the mouse wheel over the dial and (with the knob focused) Left/Right move one station at a time; the knob is read as `Sintonizar, emisora {n} de {total}: {nombre}` and the caption shows the position. Pressing a neighbour tunes to it. `Escuchar` (or pressing the station under the needle) plays without leaving Inicio and the panel shows `Cargando…`, then `Reproduciendo`, and the button reads `Apagar`; press it to stop. The current station is lit (Baquelita) or outlined (Nogal). A new search puts the first station under the needle.
37. Radio de válvulas Favoritos: pressing a label plays it in place and shows its play state; its heart removes it. Logged out, empty, loading and error show the same texts as in Moderno.
38. Radio de válvulas Reproductor: with no station the dial is empty and the panel reads `Selecciona una emisora en Inicio o en tus favoritas`. Press the power knob: the station plays, the dial window glows in and the magic eye closes; press it again: it stops and both return. A broken stream shows `Error` and `Reintentar`. With reduced motion the glow and the eye change at once.
39. Radio de válvulas presets: logged out, the keys are blank and pressing them opens `Iniciar sesión`. With no favourites they are blank and read `Aún no tienes emisoras favoritas.` With seven favourites the keys show the first five and `1/2`; the next arrow shows the other two and three blank keys. The current station's key is pressed down, and pressing a key plays it.
40. Radio de válvulas keyboard (Windows): Tab reaches the gear, the search row, the filters, the knob, the panel and the bar in that order; on the knob Right twice and then Enter plays the station two places further. At `Pequeño` and `Enorme` every Personalización and Preferencias control is usable and the dial keeps its height.
41. Casete portátil: select it while a station plays: every page changes at once, `Variante` disappears and the audio keeps playing. Moderno comes back with the variant it had.
42. Casete portátil Inicio: press a spine: it is pulled out with its insert and any other goes back. `Reproducir` (or pressing the pulled-out spine again) plays without leaving Inicio and reads `Detener` while it plays. The current station's spine has the orange outline. A new search goes back to the first spine. On Windows, Down moves to the next spine and Enter plays it.
43. Casete portátil Favoritos: with twelve favourites the J-card shows ten (`CARA A` and `CARA B`) and `1/2`; `Dar la vuelta` flips to the other two. Removing the only favourite of the last side shows the previous one. Pressing a line plays it in place; its button removes it. On Windows, Left/Right change the side. Logged out, empty, loading and error replace the card.
44. Casete portátil Reproductor: `PLAY` plays and stays pushed in; the reels speed up and turn; `STOP` stops (the reels slow down) and becomes disabled. `REW`/`FF` play the previous/next favourite. With no station the label is blank and the panel reads `Selecciona una emisora en Inicio o en tus favoritas`. Side A shows five favourites; logged out it reads `Inicia sesión para ver tus favoritas` and opens the login page. With reduced motion the reels do not turn and the card flips at once. Narrator reads `Favorita anterior`, `Reproducir`, `Detener`, `Favorita siguiente`, `Dar la vuelta a la cinta` and `Cara A, pista {n}: {nombre}`.
45. Casete portátil at `Pequeño` and `Enorme`: every page, Personalización and Preferencias stay usable.
46. Boombox: select it while a station plays: every page changes at once, `Variante` disappears and the audio keeps playing. The bar button of the current section has its LED lit.
47. Boombox Inicio: on a wide window the speakers are beside the display; make the window narrow (or use a phone): they become strips above and below and every row stays fully visible. Pressing a row plays it in place with the pink outline and `▶`; pressing it again stops. A new search goes back to the first row. On Windows, Down to the third row and Enter plays it.
48. Boombox Favoritos: the cassette door reads `MIS FAVORITAS`; pressing a row plays it in place and its filled heart removes it at once. Logged out, empty, loading and error replace the display.
49. Boombox Reproductor: with no station the display reads `Selecciona una emisora en Inicio o en tus favoritas` and the speakers are still. `▶ PLAY` plays (the key reads `■ STOP`), the speakers pulse and the meter moves; stop: the speakers settle and the meter empties. `◀◀`/`▶▶` play the previous/next favourite. With eight favourites the memories show `M1`–`M6` and `1/2`; the current one is lit cyan; logged out, pressing them opens `Iniciar sesión`. With reduced motion the speakers are still and the meter shows a fixed middle level while playing. On the Android emulator the page stays responsive while everything moves. Narrator reads `Memoria {n}: {nombre}` and skips the speakers, handle, grilles and meter.
50. Boombox keyboard and text size: Tab, arrows and Enter reach and use every control on Windows; at `Pequeño` and `Enorme` every page stays usable.
51. Mesa de DJ: select it while a station plays: every page changes at once, `Variante` disappears and the audio keeps playing. `EN DIRECTO` is lit on every page (Inicio, Favoritos, Reproductor, Ajustes, Personalización, Preferencias) and dim when stopped, without blinking. No visible label is in English.
52. Mesa de DJ Inicio: on a wide window `PAÍS` and `FORMATO` have their own columns; on a narrow one (or a phone) each row shows one secondary line and stays at least 48 units tall. `CARGAR` or pressing a row plays it in place (teal row, cyan bar); on the current station it stops. A new search goes back to the first row. On Windows, Down to a row and Enter loads it.
53. Mesa de DJ Favoritos: the tab reads `Favoritas ({n})`; `CARGAR` plays a favourite in place and its heart removes it. Logged out, empty, loading and error replace the browser.
54. Mesa de DJ Reproductor: with no station the jog wheel's centre is blank and the page reads `Selecciona una emisora en Inicio o en tus favoritas`. The round button plays (the wheel turns, the meter moves) and stops (the wheel slows down; the button shows the play icon). `ATRÁS`/`SIGUIENTE` play the previous/next favourite. With ten favourites the pads show eight and `1/2`; the current pad is orange and glows; logged out, pressing them opens `Iniciar sesión`. On Windows, arrow keys move between pads and Enter plays. With reduced motion the wheel does not turn and the pad does not glow. Narrator reads `Cargar y reproducir {nombre}` and `Pad {n}: {nombre}` and skips the wheel and meter.
55. Mesa de DJ at `Pequeño` and `Enorme`: every page, Personalización and Preferencias stay usable.
56. MP3: select it while a station plays: every page changes at once, `Variante` disappears and the audio keeps playing. The header shows the title, the battery and the gear.
57. MP3 Inicio: `▼` moves the cursor one line and the counter changes; `OK` plays the cursor's station in place (`♪ Cargando…`, then `♪ Reproduciendo` on its line); `OK` again stops it. Touching a line moves the cursor; touching it again plays it. A new search puts the cursor on the first line; going to another page and back keeps it. On Windows, Down twice and Enter plays the station two lines below.
58. MP3 Favoritos: the email and `Cerrar sesión` are on the LCD; removing the cursor's favourite moves the cursor to the next one (the previous one when it was the last). Logged out, empty, loading and error show on the LCD.
59. MP3 Reproductor: with no station the LCD reads `Selecciona una emisora en Inicio o en tus favoritas`. A long name (for example `Deutschlandfunk | DLF | MP3 128k`) scrolls sideways with pauses at each end; a short one stays still. `▶❙❙` plays (its name becomes `Detener`) and the equaliser moves; stopped, it is empty. `◀◀`/`▶▶` play the previous/next favourite. With reduced motion the name wraps to two lines with `…` and the equaliser shows a fixed level while playing. Narrator reads the full name once and skips the battery and equaliser.
60. MP3 Ajustes and text size: the LCD menu opens `Preferencias` and `Personalización`; at `Pequeño` and `Enorme` every page stays usable.
61. Clásico de rueda: in `Personalización` it offers `Blanco` and `Negro`. Moderno `Oscuro` → Radio de válvulas gives `Nogal` → Clásico de rueda gives `Negro` → Moderno gives `Oscuro`; Clásico de rueda `Blanco` → Boombox → Moderno gives `Claro`. Switching while a station plays keeps the audio.
62. Clásico de rueda wheel: a quarter turn of a finger around the ring in Inicio moves the cursor about four rows (three or four). The four edge buttons and the centre are real buttons read as `Menú`, `Anterior`, `Siguiente`, `Reproducir o pausar` and `Seleccionar`. On Windows, Up/Down and the mouse wheel move the cursor, Enter plays, Space is play/pause, Left/Right move one screen and Escape is `MENÚ` (not while typing in the search box). Down three times and Enter plays the station three rows below the first.
63. Clásico de rueda Inicio and Favoritos: `MENÚ` is disabled in Inicio; the centre plays the cursor's station in place and stops the current one; play/pause toggles the current station; touching a row moves the cursor and touching it again plays it; the current station's row shows ▶ or ❙❙ even when the cursor is elsewhere. A search puts the cursor on the first result. In Favoritos the account row comes first, `Cerrar sesión` logs out, the heart removes a row (the cursor moves to the next one, or the previous after the last), and `MENÚ` opens Inicio.
64. Clásico de rueda Reproductor: the slim bar is shown with `Reproductor` marked; the screen shows the logo, name, details, `En directo` and the status line (or `Selecciona una emisora en Inicio o en tus favoritas`). Play/pause reads `Reproducir`/`Detener`, `◀◀`/`▶▶` play the previous/next favourite, the centre adds or removes the favourite (its heart fills), sliding on the ring does nothing, and `MENÚ` opens Inicio.
65. Clásico de rueda at `Pequeño` and `Enorme`: menu rows truncate or scroll, the wheel and the slim bar keep their size; Ajustes, Personalización and Preferencias stay usable. With reduced motion the list jumps to the cursor and the wheel buttons have no press animation.

## Notes

- `appsettings.Development.json` is ignored by git; never commit real credentials.
- The API logs database failures (to the console and, on Windows, to the Application event log) with the cause only: never passwords, request bodies or the connection string. Entity Framework Core's own connection-error message may name the database server.
