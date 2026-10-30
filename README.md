# OpenWave

Cross-platform online radio player for the DEIN course project. An ASP.NET Core API serves the live Radio Browser catalogue and stores user accounts and favourite stations in a remote MySQL database. A .NET MAUI app (Windows and Android) lets you browse, search and listen to stations, create an account, log in and keep your favourites.

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
| `country`, `language`, `tag` | Optional filters forwarded to Radio Browser |
| `limit` | Maximum number of stations: 50 by default, at most 100 |

The mirrors are tried in the order listed in `RadioBrowser:Servers` in `appsettings.json`, with a 5-second timeout each. If every mirror fails, the API returns `502` with a ProblemDetails body whose `detail` is `No se pudo obtener el catálogo de emisoras.` and logs the technical cause. `OpenWave.Api.http` has ready-made example requests.

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

The app reads the base URL from the MAUI `Preferences` key `api_base_url`. With no override it uses the platform default (`http://localhost:5080` on Windows, `http://10.0.2.2:5080` on Android). There is no settings screen in this phase; set the value programmatically when needed:

```csharp
Preferences.Default.Set("api_base_url", "http://192.168.1.10:5080");
```

## Search and error states in Inicio

- Type a name in the search box and press Enter or `Buscar`: Inicio lists the stations whose name contains that text. `Limpiar` empties the box and shows the top stations again.
- While a request is running, Inicio shows `Cargando emisoras...`.
- When there are no results, Inicio shows `No se encontraron emisoras.` The search box stays available.
- When the API cannot be reached, Inicio shows `No hay conexión con el servidor.`
- When the API answers `502` (Radio Browser unavailable) or something unexpected, Inicio shows `No se pudo obtener el catálogo de emisoras.`
- Both errors show `Reintentar`, which repeats the last request, including the search text.
- The station selected in the app is highlighted in the list, and the highlight is restored after a new search or when returning to Inicio.

## Playback in Reproductor

Selecting a station in Inicio opens Reproductor and starts the station's stream with `CommunityToolkit.Maui.MediaElement`. The page shows the station name, country and logo (a local placeholder when there is no logo or it cannot be loaded).

| Status | Meaning | Button |
|---|---|---|
| `Detenido` | Nothing is playing | `Reproducir` |
| `Cargando…` | The stream is opening or buffering | `Detener` |
| `Reproduciendo` | Audio is playing | `Detener` |
| `Error` | The stream could not be opened, did not start within 15 seconds, or stopped unexpectedly | `Reintentar` |

In the `Error` state the page also shows `No se pudo reproducir esta emisora.` `Reintentar` tries the same station again; you can also pick another station in Inicio. Selecting another station stops the previous stream first; selecting the station that is already playing keeps it playing.

Limitations in this phase:

- **Playback is tied to the Reproductor page.** Switching to Inicio or Favoritos stops the audio. When you come back, the same station is shown as `Detenido` and you press `Reproducir` to listen again.
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

Each station in `Inicio` and the station in `Reproductor` show a heart: `♥` when it is one of your favourites, `♡` when it is not or when nobody is logged in.

- **Adding and removing:** while logged in, press `♡` to add the station and `♥` to remove it. The heart changes at once; it does not select the station, change the page or stop playback. If the change cannot be saved, the heart goes back and the app shows `No se pudo actualizar tus favoritas.`
- **Heart while logged out:** the app opens `Iniciar sesión`. After logging in (or creating an account) the station is added and you return to the screen where you pressed the heart. Going back without logging in adds nothing. In `Reproductor` this stops the audio, because playback is tied to that page.
- **`Favoritos`:** lists your favourites, newest first, with the name and country. Tap a station to open `Reproductor` and play it, or press `Quitar` to remove it at once (no confirmation). The list reloads from the API every time the page opens.
  - While loading it shows `Cargando favoritas...`; with no favourites, `Aún no tienes emisoras favoritas.`
  - If the list cannot be loaded it shows `No hay conexión con el servidor.` or `No se pudo acceder a la base de datos.` with `Reintentar`.
- **Logging out** empties every heart and the list.

## Manual check list

1. Start the API and open `http://localhost:5080/api/health`: `status` is `ok` and `timeUtc` is an ISO 8601 UTC timestamp.
2. Run the app on Windows or the Android emulator. Inicio lists up to 50 real stations.
3. Search `jazz`: every listed name contains "jazz". Press `Limpiar`: the box is empty and the top stations return. Search a nonsense name: `No se encontraron emisoras.`
4. Switch between Inicio, Reproductor and Favoritos.
5. Select a station in Inicio: Reproductor opens with its name, country and logo (or the placeholder), the status goes `Cargando…` → `Reproduciendo`, and you hear audio. Back in Inicio, that station is highlighted; selecting another one moves the highlight.
6. In Reproductor, press `Detener`: audio stops and the status shows `Detenido`. Press `Reproducir`: it plays again.
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
12. Log in. In Inicio press `♡` on a station: it becomes `♥` and the page does not change. Play a station and press `♡` in Reproductor: it becomes `♥` and the audio keeps playing.
13. Open Favoritos: both stations are listed, newest first. Tap one: Reproductor opens and plays it. Press `Quitar` on the other: it disappears, and its heart in Inicio is `♡` again. Remove the last one: `Aún no tienes emisoras favoritas.`
14. Log out and press `♡` in Inicio: `Iniciar sesión` opens. Go back: nothing is added. Press it again and log in: you return to Inicio with that heart `♥`.
15. Stop the API and press a heart while logged in: `No se pudo actualizar tus favoritas.` and the heart goes back. Start the API with an unreachable database (see item 10, using `$env:ConnectionStrings__Default = "Server=<HOST>;Database=2DAM_alumno19;User ID=<USER>;Password=<PASSWORD>;SslMode=Preferred;Connect Timeout=5;"`; keep `Connect Timeout=5` so the API answers before the app's 10-second timeout) and open Favoritos: `No se pudo acceder a la base de datos.` and `Reintentar`. Start the API normally and press `Reintentar`: the list loads.
16. Press `Cerrar sesión`: every heart in Inicio and Reproductor is `♡`.

## Notes

- `appsettings.Development.json` is ignored by git; never commit real credentials.
- The API logs database failures (to the console and, on Windows, to the Application event log) with the cause only: never passwords, request bodies or the connection string. Entity Framework Core's own connection-error message may name the database server.
