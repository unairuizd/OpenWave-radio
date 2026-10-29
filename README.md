# OpenWave

Cross-platform online radio player for the DEIN course project. An ASP.NET Core API serves the live Radio Browser catalogue, and a .NET MAUI app (Windows and Android) lets you browse, search and listen to it.

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
src/OpenWave.Contracts/   Shared DTOs (net10.0)
src/OpenWave.Api/         ASP.NET Core Web API (controllers)
src/OpenWave.App/         .NET MAUI app (Android + Windows)
```

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
11. Open Favoritos: it shows `Aún no tienes emisoras favoritas.` and the note that the section is not connected yet.

## Notes

- No database, login or favourites persistence exists yet.
- `appsettings.Development.json` is ignored by git; never commit real credentials.
