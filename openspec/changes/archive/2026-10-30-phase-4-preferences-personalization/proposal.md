# Proposal

## Why

Every time the app opens, Inicio shows the same global top stations, and the look of the app cannot be changed: it follows the operating system theme with one fixed font and size. This phase adds an `Ajustes` area so each user can set default station filters (country, language, genre and content type) and personalise the theme, font and text size. Both are remembered on the device and work without an account.

## What Changes

- **Contracts**: new `OptionDto(Value, Label, StationCount)`, plus a shared constant list of the four content types (`music`, `news`, `talk`, `sports` with Spanish labels), so the API and the app use one source.
- **API, catalogue options** (additive, Controllers → Services, reusing the Phase 2 mirror fallback):
  - `GET /api/catalog/countries`, `/languages` and `/genres` read Radio Browser's country, language and tag lists, map them to `OptionDto`, drop unusable entries and sort them by label.
  - `GET /api/catalog/content-types` returns the fixed list without an external call.
  - The three Radio Browser lists are cached in memory for 60 minutes (`IMemoryCache`). Failures are not cached.
  - If every mirror fails, the endpoints return 502 with `No se pudo obtener el catálogo de emisoras.`
- **API, station search** (`GET /api/stations`):
  - **BREAKING (behaviour)**: `country`, `language` and `tag` now match exactly, ignoring case, instead of matching substrings.
  - New optional `contentType` parameter, limited to the four values. Any other value returns 400 with `Tipo de contenido no válido.`
  - When both `tag` and `contentType` are present, every returned station has both tags. Radio Browser's `tagList` matches substrings, so the API also filters the results itself. The name search keeps working with every filter.
- **App, appearance**:
  - Every colour, font family and font size in the styles becomes a semantic key read with `DynamicResource`, including Shell and the tab bar. The pages already reference only styles.
  - Two theme dictionaries, `Blanco` (white) and `Negro` (true black), registered in a small `ThemeCatalog`, so a future theme needs one dictionary and one entry.
  - Six bundled open-licence fonts: Ubuntu (the new default, replacing today's Open Sans), Dosis, Caladea, Cormorant Garamond, Recursive and Pixelify Sans, each with its licence file. Ubuntu uses the Ubuntu Font Licence; the other five use the SIL Open Font License.
  - Five font-size presets, from `Pequeño` (×0.85) to `Enorme` (×1.6).
  - An `AppearanceService` applies the saved choices before the first page appears and applies changes immediately. The app stops following the operating system theme; the default is `Blanco`.
- **App, settings**:
  - A fourth tab `Ajustes` with two entries:
    - `Personalización`: theme, font and size selectors, a live preview, and `Restablecer valores por defecto`. Changes apply and save immediately.
    - `Preferencias`: four single-choice pickers whose options come from the API, plus `Guardar` and `Restablecer`.
  - Everything is stored locally with MAUI `Preferences`. Nothing goes to the database, and nothing is cleared on logout.
- **App, Inicio**:
  - Saved preferences are sent as default filters with every search.
  - A `Filtros activos` bar summarises them and can switch them off for the current session with `Desactivar` (and back on with `Activar`).
  - A specific empty message appears when the preferences match nothing. `Favoritos` and `Reproductor` are unaffected.
- **README**: the new endpoints, the settings, and how to add a theme.

Out of scope: storing preferences or appearance in the database or per user; any theme besides `Blanco` and `Negro`; following the operating system theme; multi-select filters; recommendations; translating country, language or genre names; status-bar or navigation-bar tinting; a mini-player; unit tests. The database and `database/schema.sql` are not touched.

## Capabilities

### New Capabilities
- `catalog-options`: the API lists of countries, languages, genres and content types used to choose filters, with their mapping, sorting, caching and the unavailable-catalogue response.
- `station-preferences`: the app's saved default station filters. The `Preferencias` page, local storage, behaviour when the option lists cannot be loaded, and independence from the login state.
- `app-appearance`: the app's theme, font family and font size. The `Personalización` page, immediate application across every page, persistence, startup without a wrong-theme flash, no hard-coded appearance values, and adding themes through `ThemeCatalog`.

### Modified Capabilities
- `station-catalog`: the `Live station catalogue endpoint` requirement changes. `country`, `language` and `tag` match exactly, ignoring case; `contentType` is added and validated; combining `tag` and `contentType` requires both tags.
- `station-browsing`: new requirements for applying the saved preferences in `Inicio`, the `Filtros activos` bar with `Desactivar`/`Activar`, and the empty state when preferences match nothing.
- `app-navigation`: the top-level destinations become four (`Ajustes` is added after `Favoritos`), and a new requirement defines the `Ajustes` page with its two entries.

## Impact

- **New files**:
  - Contracts: `OptionDto`, content-type constants.
  - API: `CatalogController`, `ICatalogService`/`CatalogService`.
  - App: `Resources/Themes/WhiteTheme.xaml` and `BlackTheme.xaml`; `ThemeCatalog`, `AppearanceService`, `PreferencesService`, `CatalogApiClient`; `AjustesPage`, `PersonalizacionPage` and `PreferenciasPage` with their view models; six font files plus licence texts under `Resources/Fonts/`, replacing the unused Open Sans files.
- **Changed files**:
  - API: `IRadioBrowserClient`/`RadioBrowserClient` (list requests, exact and tag-list search), `StationQuery`, `StationService`, `StationsController`, `Program.cs` (memory cache, catalogue service), `OpenWave.Api.http`.
  - App: `App.xaml`/`App.xaml.cs`, `MauiProgram.cs` (fonts, services), `AppShell` (tab and routes), `Resources/Styles/Colors.xaml` (replaced by the theme dictionaries) and `Styles.xaml`, `InicioPage`/`InicioViewModel`, `IStationService`/`StationService`, `FavoritesService` (the heart size key), and every page that references a renamed style key.
  - Other: `README.md`.
- **New dependencies**: none. `IMemoryCache` ships with ASP.NET Core. The font files are downloaded from their official open-source repositories; if that fails, implementation stops and the developer is told which files to place.
- **Unchanged**: the database, `database/schema.sql`, the account and favourites endpoints, and the session. Preferences and appearance are per device, not per user.
- **Behaviour changes users will notice**: users whose operating system uses a dark theme will now see `Blanco` until they choose `Negro`. Phase 2 clients that relied on substring `country`/`language`/`tag` matching get exact matches instead; the app does not use those filters yet.
