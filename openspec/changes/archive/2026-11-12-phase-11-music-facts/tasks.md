# Tasks

## 1. Contracts and facts content

- [x] 1.1 Add `DailyFactsDto`, `FactDto` and `MusicEventDto` to `OpenWave.Contracts`; verify `dotnet build src/OpenWave.Api` passes
- [x] 1.2 Write `src/OpenWave.Api/Resources/Facts.es.json` and `Facts.en.json` with about 60 original music facts, same ids in both, and mark them as embedded or copied content in the csproj; verify both files parse as JSON and their id sets are identical (a small script or a startup check)

## 2. API: facts service and endpoint

- [x] 2.1 Add `FactsCatalog` (loads both JSON files once, fails fast on mismatched ids) and `IFactsService` / `FactsService` with the deterministic pick `(days since 2026-01-01) mod count` in UTC and the newest-first history; verify with a quick console check (or Swagger) that today's and yesterday's facts are consecutive and stable across calls
- [x] 2.2 Add `WikimediaOptions`, `IWikimediaClient` / `WikimediaClient` (typed `HttpClient`, User-Agent, 5-second timeout, zero-padded month/day, `events` and `births`), the per-language music keyword filter with whole-word matching, the up-to-three selection with article links, and the `IMemoryCache` entry per `(date, language)` expiring at the next UTC midnight; verify in Swagger that a real call returns items for a date known to have music events (for example `en`, `08/16`) and that a second call does not hit Wikipedia (log once)
- [x] 2.3 Add `FactsController` with `GET /api/facts/daily?days=` (default 15, range 1–31, 400 + localized `InvalidDays` otherwise), register everything in `Program.cs` and add `InvalidDays` to `Messages.resx` / `Messages.en.resx`; verify with Swagger: 15 facts newest first in Spanish, English texts with `Accept-Language: en`, 400 for `days=0`, and 200 with an empty `onThisDay` when the Wikimedia base URL is pointed at an unreachable host
- [x] 2.4 Document the endpoint in the README ("Daily fact and Curiosidades": files, pick rule, UTC note, Wikipedia source and attribution, options) and in `OpenWave.Api.http`; verify the documented requests run as written and `dotnet build src/OpenWave.Api` passes

## 3. App: client, state and strings

- [x] 3.1 Add `FactsApiClient` (typed `HttpClient` with `AcceptLanguageHandler`, maps failures to the existing localized connection messages) and the singleton `FactsService` (loads only while `SessionService.IsLoggedIn`: once after login, on `Reintentar`, on a new UTC day and on `LanguageChanged`; clears on logout); register both in `MauiProgram.cs`; verify the Windows build passes
- [x] 3.2 Add the app strings in Spanish and English (`Page_Curiosidades`, `Ajustes_CuriosidadesDescription`, `Facts_Caption`, `Facts_Today`, `Facts_OnThisDay`, `Facts_OnThisDayEmpty`, `Facts_Previous`, `Facts_ViewOnWikipedia`, `Facts_Attribution`, `Facts_Offline`, `Facts_LogInHint`) and the `ic_bulb.png` icon; verify both `.resx` parse and the Windows build passes

## 4. App: strip in every theme

- [x] 4.1 Add `Views/FunFactStrip.cs` (shared control: icon, caption, one-line fact with `…`, chevron, `RowButton` with the accessible name, `StationRow` size keys, `Cargando…` / `Sin conexión` states, opens `curiosidades`; logged out it shows `Inicia sesión para ver el dato del día.` and opens `login`); verify the Windows build passes
- [x] 4.2 Insert `<views:FunFactStrip />` as a new row between the filter `Border` and the list in the `InicioTemplate` of the eight theme layouts (Moderno, Tocadiscos, Casete, Valvulas, Boombox, MesaDj, Mp3, Rueda), moving the list row down; verify the Windows and Android builds pass and that `Inicio` shows the strip under the filters in every theme on Windows, in each theme's colours and font

## 5. App: Curiosidades page and Ajustes entry

- [x] 5.1 Add `CuriosidadesViewModel` (today, on-this-day items, previous facts, dates formatted in the app language, `Retry`, `OpenArticle` via `Launcher`) and `Views/CuriosidadesPage.xaml` (SettingsHeader, SizeLimit, the logged-out hint + `Iniciar sesión`, the three blocks, loading, error + `Reintentar`, empty-state text, attribution, BottomBar); register the `curiosidades` route and the page; verify the Windows build passes and the page shows today's fact, the Wikipedia block and 14 earlier facts
- [x] 5.2 Add the third `SettingsRow` `Curiosidades` to `AjustesPage` and `OpenCuriosidades` to `AjustesViewModel`; verify on Windows that it is the third entry with its description, opens the page and the back arrow returns to `Ajustes`
- [x] 5.3 Verify the session gate on Windows: logged out, the strip shows the hint and opens the login page, `Curiosidades` from `Ajustes` shows the hint and `Iniciar sesión`, and no request to `/api/facts/daily` appears in the API log; after logging in (your own test account) the strip and the page load; after `Cerrar sesión` both show the hint again
- [x] 5.4 Verify the states on Windows: API stopped → strip `Sin conexión` and page `No hay conexión con el servidor.` + `Reintentar`; API started → `Reintentar` loads; switching to `English` re-requests and shows English facts, dates and Wikipedia items; `Ver en Wikipedia` opens the article in the browser
- [x] 5.5 Verify sizes on Windows: smallest window at `Enorme` (strip and page fully reachable, no sideways scroll) and maximised (strip and page centred within their limits)

## 6. Documentation and integration check

- [x] 6.1 Add the manual check items to the README (session gate, strip in every theme, page blocks, Ajustes entry, states, English, sizes, Android) and the app side of "Daily fact and Curiosidades"; verify the steps match the app
- [x] 6.2 Manual run on the Android emulator in portrait: strip in two themes, `Curiosidades` from the strip and from `Ajustes`, `Ver en Wikipedia` opens Chrome, `Enorme` reachable
- [x] 6.3 Run the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and report the real output
