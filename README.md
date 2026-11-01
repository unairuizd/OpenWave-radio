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

- **Playback is tied to the Reproductor page.** Going to another page (Inicio, Favoritos or Ajustes) stops the audio. When you come back, the same station is shown as `Detenido` and you press `Reproducir` to listen again.
- **No mini-player** outside Reproductor.
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

**Icons** are original 24×24 single-colour drawings made for this project (`Resources/Images/ic_*.svg`, see `Icons-LICENSE.txt`, also shipped in the app's `Licences/` folder). They are tinted at runtime with the palette colours, and their size follows the title text size.

**Known limitation (Windows):** after a text size change in `Personalización`, icons on pages that were already open keep their previous size; the text resizes at once. Pages opened afterwards, and every page after restarting the app, show the icons at the new size. The CommunityToolkit tint draws each icon at the size it had when it was first drawn. Android resizes every icon immediately.

## Ajustes and Personalización

`Ajustes` opens from the gear. It shows two full-width cards, `PERSONALIZACIÓN` and `PREFERENCIAS`; each opens its page, and the back button (or the gear) returns to `Ajustes`.

`Personalización` works without an account; logging in or out does not change it.

- **Selectors:** `Tema` (every registered theme; today only `Moderno`), `Variante` (`Claro`, `Oscuro`; shown only when the theme has more than one), `Tipo de letra` (each font name is written in its own font) and `Tamaño del texto` (`Pequeño`, `Normal`, `Grande`, `Muy grande`, `Enorme`). The current choices are selected when the page opens.
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
| `appearance_theme` | `moderno` | `moderno` (Moderno) |
| `appearance_theme_variant` | `light` | `light` (Claro), `dark` (Oscuro) |
| `appearance_font` | `Ubuntu` | `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive`, `Pixelify Sans` |
| `appearance_font_scale` | `1.0` | `0.85` Pequeño, `1.0` Normal, `1.2` Grande, `1.4` Muy grande, `1.6` Enorme |

The previous version stored `white` (Blanco) or `black` (Negro) in `appearance_theme`. On the first start they become `moderno` with `light` or `dark`, and the new values are written back. Any other missing, unknown or corrupt value falls back to its default (`Moderno` `Claro`, `Ubuntu`, `Normal`). The app does not follow the operating system's light or dark setting.

### Themes

A theme is three things in `Resources/Themes/`:

- **Palettes** (one per variant): the colours. Every palette defines the same 14 keys: `PageBackgroundColor`, `SurfaceColor`, `InputBackgroundColor`, `DividerColor`, `SelectedItemBackgroundColor`, `PrimaryTextColor`, `SecondaryTextColor`, `ErrorTextColor`, `AccentColor`, `AccentTextColor`, `FavoriteIconColor`, `TabBarBackgroundColor`, `TabBarSelectedColor`, `TabBarUnselectedColor`.
- **A layout:** the page templates and the shared pieces: `HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`, `AjustesTemplate` and `SettingsPageTemplate`. Pages pick their template with `DynamicResource`, so switching theme swaps the layout.
- **An entry in `ThemeCatalog`.**

Pages, layouts and styles never contain literal colours, fonts or sizes: colours come only from the palettes, and the font family and sizes only from `Resources/Styles/Styles.xaml` (replaced at runtime by the appearance settings). Themes never set font families or text sizes. The single audio player (`MediaElement`) is the content of `ReproductorPage`, not part of any layout, so a theme change can never duplicate or restart it.

**Moderno** (`ModernoLayout.xaml`) has two variants:

- **Claro** (`ModernoLightPalette.xaml`): white background (`#FFFFFF`), near-black text (the colours of the former Blanco).
- **Oscuro** (`ModernoDarkPalette.xaml`): true black background (`#000000`), near-white text and a lighter accent (the colours of the former Negro).

Each palette also sets the light or dark mode of the native controls (text inputs, pickers and dialogs).

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
2. **Layout:** copy `ModernoLayout.xaml` and its `.xaml.cs` (for example `RetroLayout.xaml`), rename the class, and change the templates as you like. Keep every template key and the bindings and commands they use (the page view models, `AppNavigation` for the gear and the bar, the `ContentPresenter` in `ReproductorTemplate` and `SettingsPageTemplate`). Use only palette keys for colours and the named styles for text: never set a font family or a font size.
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
7. Switch to Favoritos while audio plays: the audio stops. Return to Reproductor: the same station shows `Detenido`. Select another station in Inicio: the new station starts and the old one does not keep playing.
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

## Notes

- `appsettings.Development.json` is ignored by git; never commit real credentials.
- The API logs database failures (to the console and, on Windows, to the Application event log) with the cause only: never passwords, request bodies or the connection string. Entity Framework Core's own connection-error message may name the database server.
