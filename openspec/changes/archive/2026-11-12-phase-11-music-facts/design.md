# Design

## Context

See proposal.md for the motivation. What this builds on:

- The API already has the shape for an external source: `RadioBrowserClient` is a typed `HttpClient` with a User-Agent, its own per-request timeout and options bound from `appsettings.json`; `CatalogService` caches lists in `IMemoryCache`; controllers return `ProblemDetails` with `detail` from `IStringLocalizer<Messages>` in the request language (`Accept-Language`, Spanish default).
- The app already has typed clients over `HttpClient` that throw `StationRequestException` / `ApiRequestException` with localized messages (`CatalogApiClient`), the `AcceptLanguageHandler` on every client, settings-style pages with `SettingsHeader` + `SizeLimit` + `BottomBar`, and shared controls (`BottomBar`, `SettingsHeader`, `SettingsRow`) built in C# from the semantic keys and used by every theme.
- Each theme's `InicioTemplate` has the same skeleton: header, a `Grid` with the search row (row 0), the saved-filters `Border` (row 1) and the list (row 2), then the bottom bar.
- Phase 9 size keys: `StationRowMin/MaxWidth`, `StationRowMin/MaxHeight`, `SettingsColumnMin/MaxWidth`.
- The Wikimedia Feed API, checked from this machine: `GET https://api.wikimedia.org/feed/v1/wikipedia/{es|en}/onthisday/{events|births}/{MM}/{DD}` answers JSON (`events: [{ text, year, pages: [{ content_urls.desktop.page, … }] }]`), needs zero-padded month and day, and wants a descriptive User-Agent. Content is CC BY-SA.

## Goals / Non-Goals

**Goals:**

- A fact every day with no storage and no account, reproducible for any date.
- The Wikipedia block is an addition that can fail silently; the daily fact never waits for it.
- One shared strip control, one page, one endpoint: the smallest set of pieces that satisfies the spec.

**Non-Goals:**

- Editing or adding facts from the app; marking facts as read; sharing.
- Any Wikipedia lookup for days other than today (history is facts only).
- Translating Wikipedia texts: each language's own feed is used as it comes.
- Persisting facts on the device for offline use.

## Decisions

### 1. Facts live in two JSON files in the API, keyed by id

`Resources/Facts.es.json` and `Facts.en.json`, each an array of `{ "id": 1, "text": "…" }`, same ids in both. About 60 original facts written for this change (short, verifiable, no quotations from copyrighted texts). Loaded once at startup into a singleton (`FactsCatalog`), which fails fast with a clear exception if the two files do not have the same ids.

*Alternative:* a `resx` per language. Rejected: facts are content with ids and ordering, not UI messages, and JSON is easier for the developer to review and extend. *Alternative:* a database table. Rejected: the schema is frozen for the developer to run by hand, and nothing is per user.

### 2. Deterministic pick: `(days since 2026-01-01) mod count`, UTC

`FactsService.GetFact(DateOnly day)` = `catalog[(day.DayNumber − epoch.DayNumber) mod count]` (positive modulo, so dates before the epoch also work). History is the same call for `today − i`, `i = 0..days−1`. UTC is used so the pick does not depend on the server's time zone; the date in the response is that UTC date, and the app shows it as a date (no time), so a user in Spain sees today's date for the whole local day except the hour between 00:00 and 01:00/02:00, which is accepted for a course project and noted in the README.

### 3. One endpoint, one DTO

`GET /api/facts/daily?days=15` → `DailyFactsDto { Date: DateOnly, Facts: FactDto[] (Date, Text), OnThisDay: MusicEventDto[] (Year, Text, ArticleUrl) }`. `days` outside 1–31 → 400 with `detail` from a new `Messages` key `InvalidDays`. One request serves both the strip and the page, which is why the app loads once and shares the result.

### 4. Wikipedia client: separate, time-boxed, cached, never fatal

`WikimediaClient` (typed `HttpClient`, `User-Agent: OpenWave/1.0 (course project)`, 5-second timeout, base URL from `WikimediaOptions`, default `https://api.wikimedia.org/feed/v1/wikipedia/`). `FactsService` asks it for `events` and `births` of today in the request language, filters each item's `text` with a per-language keyword list (`es`: músic, cantante, compositor, banda, álbum, canción, ópera, concierto, guitarr, pianist, orquesta, disco, rock, jazz, …; `en`: music, singer, composer, band, album, song, opera, concert, guitar, pianist, orchestra, rock, jazz, …), matched as whole words or word starts to avoid `pop` in `pope` and `band` in `disband`, takes up to three (births after events, both as the feed orders them), and maps `pages[0].content_urls.desktop.page` to the link. The result (possibly empty) is cached in `IMemoryCache` under `(date, language)` with an absolute expiration at the next UTC midnight. Any exception or non-success status is logged and becomes an empty list; the facts are still returned.

*Alternative:* letting the app call Wikipedia. Rejected: the app talks only to our API; and keeping the filter in the API means one place to improve it.

### 5. Strip: a shared `FunFactStrip` control, one line per theme

`Views/FunFactStrip.cs` (a `ContentView`, like `BottomBar`): a `Border` with the `StationCard` style holding a `Grid` (icon · caption + one-line label with `TailTruncation` · chevron) and a `RowButton` over it for the keyboard and screen readers (`SemanticProperties.Description` = `Dato del día` + fact). It binds to a singleton `FactsState` (see 6) and navigates with `Shell.Current.GoToAsync("curiosidades")`. Height limits `StationRowMin/MaxHeight`, width limits through a `SizeLimit` with the `StationRow` keys, centred. Each `InicioTemplate` gets `<views:FunFactStrip Grid.Row="2" />` and its list moves to row 3 (one more `Auto` row in the `RowDefinitions`), the same edit in the eight files. No theme draws anything of its own for it.

### 6. Shared state: `FactsService` in the app, loaded once per login

`Services/FactsService.cs` (singleton, `ObservableObject`): `IsAvailable` (= `SessionService.IsLoggedIn`), `Today`, `Facts`, `OnThisDay`, `IsLoading`, `ErrorMessage`, `LoadAsync(force)`; it calls `FactsApiClient.GetDailyAsync(15)` only while `IsAvailable`: the first time it is asked after a login, again when `force` is true (`Reintentar`), when the stored `Date` is not today (UTC) at the next ask, and when `Localizer.LanguageChanged` fires. It listens to `SessionService.PropertyChanged`: on login it loads, on logout it clears everything (the same moment `FavoritesService` forgets the favourites). `FunFactStrip` and `CuriosidadesViewModel` both read it, so the strip never triggers a second request and both show the same data.

### 7. Curiosidades page

Logged out, the page shows only `Inicia sesión para ver el dato del día.` and an `Iniciar sesión` button (`GoToAsync("login")`; the existing `..` on success returns here, and `OnAppearing` asks `FactsService` again, which now loads). Logged in, settings-style page: `SettingsHeader`, `ScrollView` → `SizeLimit` (`SettingsColumn` keys) → `VerticalStackLayout`: today's fact card (date in `SecondaryLabel`, text in a body label), the `Tal día como hoy en la música` group (`SettingsGroupLabel`, then a card per item with year + text + a `Ver en Wikipedia` text button that calls `Launcher.Default.OpenAsync(url)`, then the attribution in `SecondaryLabel`; or the empty-state label), the `Días anteriores` group with one card per fact (`BindableLayout` over the 14 earlier facts). Dates are formatted with `ToString("d MMMM yyyy", culture)` through `Localizer`, so they follow the app language. Route `curiosidades`, registered as a transient page like `Preferencias`; a third `SettingsRow` in `Ajustes` with a new icon `ic_bulb.png` (same style as the two existing icons).

### 8. Gate in the app, not in the API

The session exists only in the app (`SessionService`, never stored) and the project accepts that the API endpoints are not authorised (favourites work the same way). So `GET /api/facts/daily` stays open and the app simply does not call it while logged out. The strip's logged-out state navigates to `login` instead of `curiosidades`, so a logged-out user never lands on an empty page from `Inicio`; from `Ajustes` the page is reachable and shows the hint itself.

*Alternative:* requiring the user id or email on the endpoint. Rejected: it would be security theatre without tokens (a non-goal of the whole project) and would break the pattern favourites already follow.

### 9. Strings

App: `Page_Curiosidades`, `Facts_LogInHint` (`Inicia sesión para ver el dato del día.`), `Ajustes_CuriosidadesDescription`, `Facts_Caption` (`DATO DEL DÍA`), `Facts_Today`, `Facts_OnThisDay`, `Facts_OnThisDayEmpty`, `Facts_Previous`, `Facts_ViewOnWikipedia`, `Facts_Attribution`, `Facts_Offline` (`Sin conexión`). API: `InvalidDays`. Existing `Loading`, `Action_Retry` and the connection message are reused.

## Risks / Trade-offs

- [Wikipedia keyword filter is approximate: false positives (a "rock" formation) or days with nothing] → whole-word matching, a review of a few dates during the manual run, and the explicit empty-state text; the block is secondary by design.
- [Wikimedia rate or availability] → one call per language per day thanks to the cache; a failure costs at most 5 seconds on the first request of the day and shows the empty state.
- [UTC day vs. local day] → accepted and documented; a fixed rule everyone can explain beats time-zone handling in a course project.
- [Eight layout edits for the strip] → mechanical and identical (one row added); the manual run checks every theme.
- [Facts quality] → the facts are reviewed by the developer before the change is archived; the JSON files are the only place to fix one.
- [Logged-out users can still call the endpoint by hand] → accepted, as for favourites; the content is public and harmless.
- [`Launcher.OpenAsync` opens the system browser] → it is the platform's standard behaviour for an external link; no in-app browser is added.
