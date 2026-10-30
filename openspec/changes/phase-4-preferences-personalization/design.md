# Design

## Context

See proposal.md for the motivation and the specs for the required behaviour. Current state that shapes the approach:

- **API**:
  - `RadioBrowserClient.SearchAsync(name, country, language, tag, limit)` sends `country`, `language` and `tag` as substring filters. Its private mirror loop (5-second timeout per mirror, `User-Agent: OpenWave/1.0`, `RadioBrowserUnavailableException` when every mirror fails) is written for the station search only.
  - `StationService` trims the name, caps the limit, filters names exactly and removes duplicate ids. `StationsController` maps `RadioBrowserUnavailableException` to 502.
  - Phase 3 added `ServiceResult<T>`/`ServiceError` and `ToProblem` for expected business errors.
- **Radio Browser** (checked against the server documentation and live requests while planning):
  - `countryExact`, `languageExact` and `tagExact` are boolean flags next to `country`, `language` and `tag`, and they are **case-sensitive**: `country=Spain` matches but `spain` does not, and `language=Spanish` returns nothing.
  - Languages and tags are stored in lower case; countries are stored with capitals (for example `Spain`).
  - `tagList` requires every listed tag but matches them as **substrings**: of 60 results for `jazz,music`, only 13 had both tags exactly (others had `smooth jazz`, `lounge music`).
  - The top 100 tags include `music`, `news`, `talk` and `sports`. `/json/countries` returns 227 countries, 2 of them with an empty name.
- **App styles**:
  - No page sets a literal colour, font or size; pages only reference styles (`PageTitleLabel`, `SecondaryLabel`, `ErrorLabel`, `StationNameLabel`, `StationCard`, `FavoriteToggleButton`, `PlayerArtwork`).
  - Every colour in `Styles.xaml`, and in Inicio's selected-card trigger, is an `AppThemeBinding` of two `StaticResource` colours from `Colors.xaml`, so the app follows the OS theme and would not react to a dictionary swap.
  - Font family and sizes already use `DynamicResource` (`AppFontFamily` = `OpenSansRegular`, `TitleFontSize` 24, `BodyFontSize` 16, `CaptionFontSize` 13, `FavoriteIconFontSize` 24). `PlayerArtworkSize` 200 is a layout size, not a font size.
  - The app icon and splash colours in the csproj are build-time assets, outside the runtime theme.
- **Fonts**:
  - `OpenSans-Regular.ttf` and `OpenSans-Semibold.ttf` are bundled today (without their licence). Open Sans is not among the six chosen fonts, so it becomes unused.
  - `<MauiFont Include="Resources\Fonts\*" />` would also treat licence text files as fonts.
  - Google's official `google/fonts` repository has all six chosen fonts:
    - **Static files:** Ubuntu (`ufl/ubuntu/`: `Ubuntu-Regular.ttf`, `Ubuntu-Bold.ttf`, licence `UFL.txt`, the Ubuntu Font Licence) and Caladea (`ofl/caladea/`: `Caladea-Regular.ttf`, `Caladea-Bold.ttf`, `OFL.txt`).
    - **Variable files only:** Dosis (`ofl/dosis/Dosis[wght].ttf`), Cormorant Garamond (`ofl/cormorantgaramond/CormorantGaramond[wght].ttf`), Recursive (`ofl/recursive/Recursive[CASL,CRSV,MONO,slnt,wght].ttf`) and Pixelify Sans (`ofl/pixelifysans/PixelifySans[wght].ttf`), each folder with its `OFL.txt`.
- **Android**: `MainActivity` already declares `ConfigChanges.UiMode`, so changing the app's light/dark mode does not recreate the activity.

## Goals / Non-Goals

**Goals:**
- One place for every colour (the theme file) and every font family and size (resources updated by one service), read with `DynamicResource` so changes are live.
- Station filters that are exact and case-insensitive for any input, built from the values the catalogue uses.
- Keep the layering (Controllers → Services → Radio Browser client) and reuse the Phase 2 mirror fallback and the Phase 3 `ServiceResult`.
- Preferences and appearance work offline from the database, without an account, and survive restarts.

**Non-Goals:**
- Storing anything in the database or per user, following the OS theme, more than two themes, multi-select filters, translating catalogue names, tinting system bars, unit tests.
- Changing the account, favourites or playback behaviour.

## Decisions

### 1. One mirror loop for every Radio Browser request

`RadioBrowserClient` gets a private generic `GetFromMirrorsAsync<T>(string pathAndQuery, ct)` holding today's loop: mirrors in order, 5-second timeout each, warning per failure, then `RadioBrowserUnavailableException`. The search and three new methods use it:

- `GetCountriesAsync` calls `json/countries?hidebroken=true`.
- `GetLanguagesAsync` calls `json/languages?hidebroken=true&order=stationcount&reverse=true&limit=100`.
- `GetTagsAsync` calls `json/tags?…` with the same parameters as languages.

Each returns a small internal `RadioBrowserNamedCount(Name, StationCount)` record. The client only fetches; mapping stays in services.

### 2. Catalogue options service with an in-memory cache

A new `ICatalogService`/`CatalogService` and `CatalogController` (`api/catalog/countries|languages|genres|content-types`).

- **Mapping:**
  - Values and labels are trimmed. Entries with an empty name or `stationcount` 0 are dropped, and genres also drop single-character names.
  - Duplicate values are dropped case-insensitively, keeping the first.
  - Labels capitalise the first character (`char.ToUpperInvariant`); country labels stay as returned.
  - Sorting uses `StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: true)`, so `Español…` sorts among the E's.
- **Cache:** `AddMemoryCache()` plus `IMemoryCache`, one key per list. The service reads with `TryGetValue` and calls `Set(..., TimeSpan.FromMinutes(60))` only after a successful load, so a `RadioBrowserUnavailableException` propagates and nothing is cached. `GetOrCreateAsync` was rejected because it can cache a faulted result unless written carefully; the explicit form is easier to explain.
- **Content types:** a static `ContentTypes.All` list of `OptionDto` (`music`/`Música`, `news`/`Noticias`, `talk`/`Charlas`, `sports`/`Deportes`) in `OpenWave.Contracts`. Both the API (validation, endpoint) and the app (summary labels) read this one list. It is constant data, not logic, which keeps Contracts within its "DTOs, no logic" rule. The alternative, duplicating the four labels in the app, was rejected.
- **Errors:** the controller maps `RadioBrowserUnavailableException` to 502 with `No se pudo obtener el catálogo de emisoras.`, as `StationsController` does.

### 3. Exact, case-insensitive station filters

`StationQuery` gains `ContentType`. `StationService.GetStationsAsync` now returns `ServiceResult<IReadOnlyList<StationDto>>`. `StationsController` uses `ToProblem` for `Invalid` and keeps its 502 catch. The steps:

1. **Validate `contentType`:** after trimming, a non-empty value must be one of `ContentTypes.All` values (case-insensitive). Otherwise the result is `Invalid` with `Tipo de contenido no válido.` (400).
2. **Canonicalise country, language and tag**, because Radio Browser's exact flags are case-sensitive. Each value is looked up case-insensitively in the cached option lists from decision 2 and replaced by the catalogue's own spelling (`spain` → `Spain`). Values not in the lists fall back to the catalogue's own convention: the trimmed value for country, lower case for language and tag. If loading a list fails, the search would fail anyway, so the exception is not caught here.
3. **Build the Radio Browser search:**
   - `countryExact=true` and `languageExact=true` when those filters are present.
   - Tags: the set {tag, contentType} without duplicates. One tag uses `tag` + `tagExact=true`. Two tags use `tagList=a,b`, and the API asks Radio Browser for 300 stations instead of `limit`, because many results are dropped in the next step.
4. **Post-filter:**
   - Keep stations whose tags contain every required tag exactly, ignoring case. This is cheap and also guards the one-tag case.
   - Then apply the existing name filter, remove duplicate ids and `Take(limit)`.

`IRadioBrowserClient.SearchAsync` takes a `RadioBrowserSearch` record (name, country, language, tags, fetch limit) instead of five loose parameters.

### 4. Semantic keys, theme dictionaries and `DynamicResource`

**Colour keys** (defined in every theme file):
- The requested set: `PageBackgroundColor`, `SurfaceColor`, `PrimaryTextColor`, `SecondaryTextColor`, `AccentColor`, `AccentTextColor`, `DividerColor`, `SelectedItemBackgroundColor`, `ErrorTextColor`, `TabBarBackgroundColor`, `TabBarSelectedColor` and `TabBarUnselectedColor`.
- Two more, so nothing borrows a colour meant for something else: `FavoriteIconColor` (the heart) and `InputBackgroundColor` (entries and pickers).

**Renamed and removed keys:**

| Old | New |
|---|---|
| `OnAccentTextColor` | `AccentTextColor` |
| `ErrorColor` | `ErrorTextColor` |
| `SelectedSurfaceColor` | `SelectedItemBackgroundColor` |
| `TitleFontSize` / `BodyFontSize` / `CaptionFontSize` | `FontSizeTitle` / `FontSizeBody` / `FontSizeCaption` |
| (new) | `FontSizeSubtitle` |
| `FavoriteIconFontSize` | removed; the heart uses `FontSizeTitle`, so it scales with the text |
| `PlayerArtworkSize` | kept unchanged (a layout size, not a font size) |

**Files and styles:**
- `Colors.xaml` and every `*Dark` key are deleted.
- `Styles.xaml` keeps only font family and size defaults (`AppFontFamily`, the four `FontSize*` values at scale 1.0) and styles whose colour, font and size setters all use `{DynamicResource …}`. Inicio's selected-card `DataTrigger` uses the same keys.
- Implicit styles cover `Page`, `Shell` (navigation bar and tab bar colours), `Label`, `Button`, `Entry`, `Picker`, `RadioButton` and `ActivityIndicator`.
- `StationNameLabel` uses `FontSizeSubtitle`, giving the subtitle size a real use.

**Theme dictionaries:** `Resources/Themes/WhiteTheme.xaml` and `BlackTheme.xaml`, each with `x:Class` and code-behind, so they are types the registry can create. Both have contrast-checked colours: Blanco has a white background (`#FFFFFF`) and near-black text; Negro has `#000000` with near-white text and a lighter accent.

**Registry:**
- `ThemeCatalog.All` holds `AppThemeDefinition(Id, DisplayName, ResourceDictionaryType, BaseTheme)` entries: `white`/`Blanco`/`WhiteTheme`/`AppTheme.Light` and `black`/`Negro`/`BlackTheme`/`AppTheme.Dark`.
- Adding a theme means one XAML file and one line here.
- `Personalización` builds its theme list from `ThemeCatalog.All`.

### 5. `AppearanceService` and startup without a flash

`AppearanceService` is a singleton `ObservableObject` with `ThemeId`, `FontFamilyId`, `FontScale`, `Initialize()`, `SetTheme`, `SetFontFamily`, `SetFontScale` and `ResetToDefaults`.

**Applying a choice:**
- **Theme:** removes the previous theme dictionary from `Application.Current.Resources.MergedDictionaries` (the instance it keeps), adds a new instance of the selected type, and sets `Application.Current.UserAppTheme` from `BaseTheme`. Native controls (text inputs, pickers, dialogs) therefore follow; on Android this is already handled by the `UiMode` config change.
- **Font:** writes `Application.Current.Resources["AppFontFamily"]` = the font alias.
- **Size:** writes the four `FontSize*` entries as base × scale, rounded to one decimal place.

Top-level entries override the defaults in the merged `Styles.xaml`, and every `DynamicResource` reference updates at once.

**Storage:**
- Each setter writes MAUI `Preferences` (`appearance_theme`, `appearance_font`, `appearance_font_scale`).
- `Initialize()` reads them. An unknown theme id, an unknown font or a scale that matches no preset (tolerance 0.001) falls back to its default.
- Each read is wrapped so that a corrupt stored value (for example a string where a double is expected) is treated as missing.

**Startup:** `App`'s constructor receives the service and calls `Initialize()` right after `InitializeComponent()`, before `CreateWindow` builds `AppShell`. The first page is therefore created with the right resources.

**Presets:** an `AppearanceOptions` class holds the font list (id = alias = display name: `Ubuntu`, the default, then `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive`, `Pixelify Sans`) and the five size presets with their Spanish names, used by both the service and the page.

### 6. Fonts: official files, one regular file per family

- **Files:** downloaded from `google/fonts`, one regular file per family, renamed so the file name has no brackets, with the family's licence saved next to it as `<Family>-OFL.txt` (Ubuntu: `Ubuntu-UFL.txt`):
  - Ubuntu: `Ubuntu-Regular.ttf`.
  - Caladea: `Caladea-Regular.ttf`.
  - Dosis: `Dosis[wght].ttf` saved as `Dosis-Variable.ttf`.
  - Cormorant Garamond: `CormorantGaramond[wght].ttf` saved as `CormorantGaramond-Variable.ttf`.
  - Recursive: `Recursive[CASL,CRSV,MONO,slnt,wght].ttf` saved as `Recursive-Variable.ttf`.
  - Pixelify Sans: `PixelifySans[wght].ttf` saved as `PixelifySans-Variable.ttf`.
- **Registration:** `ConfigureFonts` registers each file with its display name as alias (`"Ubuntu"`, `"Cormorant Garamond"`, …).
- **Csproj:** the `MauiFont` include becomes `Resources\Fonts\*.ttf`, so the licence files are not treated as fonts. They are added as plain content, so they ship with the app.
- **Bold:** texts marked bold use `FontAttributes="Bold"`, which renders the variable fonts' bold axis or a synthesised bold. Group 3 verifies regular and bold text for all six fonts on Windows and Android. Pixelify Sans is a pixel-style display font and Cormorant Garamond has small letterforms; both are checked for readability at `Normal` and `Pequeño`.
- **Fallbacks:**
  - If a variable font renders wrongly, implementation stops and asks before using upstream static files.
  - If the download fails, implementation stops and lists the exact files and destinations. No font is substituted.
  - The Open Sans registrations and the two `OpenSans-*.ttf` files are removed, because no option uses them any more.

### 7. Personalización page

- **Selectors:** three groups of `RadioButton`s (theme, font, size) bound to view-model option lists.
  - A `RadioButton` takes its own `FontFamily`, so each font option sets `FontFamily` to its alias and is displayed in that font. A `Picker` cannot style individual items, so it was rejected.
  - Checking a button calls the matching `AppearanceService` setter, which applies and stores the change at once.
- **Preview card:** uses the normal styles (`PageTitleLabel`, body `Label`, `SecondaryLabel`, a sample `Button`), so it shows exactly what the app shows.
- **Reset:** `Restablecer valores por defecto` calls `ResetToDefaults()` and the radio selections refresh from the service.

### 8. Preferences: service, storage and page

- **`PreferencesService`** (singleton):
  - Holds `Current` (`UserPreferences(Country, Language, Genre, ContentType)`, nullable strings), `Save(UserPreferences)`, `Reset()`, a `Changed` event, and `ApplyInInicio` (in memory, `true` at every start; changing it also raises `Changed`).
  - Storage uses the keys `pref_country`, `pref_language`, `pref_genre` and `pref_content_type`; a null value removes its key.
  - The service never touches the database or the session, so logout leaves it alone.
- **`CatalogApiClient`:** a typed `HttpClient` with the same base URL and 10-second timeout. It reuses `StationRequestException`: unreachable → `Unreachable`, 502 → `CatalogueUnavailable`, anything else → `Unexpected` (shown as the catalogue message).
- **`PreferenciasViewModel`** (transient):
  - On appearing it loads the four lists in parallel, showing a loading indicator and an error with `Reintentar`.
  - Each picker's items are `Todos` (value null) plus the options. If the saved value is missing, the view model adds an option for it, labelled with the same capitalisation rule, and selects it.
  - `Guardar` is enabled only when the lists are loaded; it saves and shows `Preferencias guardadas.`
  - `Restablecer` works even when the lists failed: it saves an empty `UserPreferences` and sets every picker to `Todos`.
  - A failed load never calls `Save`.
- **Summary labels:**
  - Country: the value as stored.
  - Language and genre: first letter capitalised.
  - Content type: the label from `ContentTypes.All`.
  - Computed in one helper used by both the page and Inicio.

### 9. Inicio integration

- **Request:** `IStationService.SearchAsync(name, StationFilters? filters, ct)` adds `country`, `language`, `tag` and `contentType` query parameters when present.
- **View model:** `InicioViewModel` reads `PreferencesService`.
  - Filters are sent only when `ApplyInInicio` is true and at least one value is set.
  - On `Changed` it sets a reload flag; `LoadIfNeeded()` (already called from `OnAppearing`) reloads with `_lastQuery` when the flag is set.
- **Bar:** new view-model properties `ShowFilterBar` (any value saved), `FiltersApplied`, `FilterSummary`, `IsPreferenceEmpty`, and `DeactivateFiltersCommand`/`ActivateFiltersCommand`. These set `ApplyInInicio` and reload with the current search text.
- **Empty states:** `IsEmpty` splits into two messages:
  - With filters applied: `No se encontraron emisoras con tus preferencias.` plus a `Desactivar` button.
  - Otherwise: the existing `No se encontraron emisoras.`
- **Other pages:** `FavoritesService` and `PlaybackService` are not touched, so Favoritos and Reproductor ignore preferences.

### 10. Navigation

- **Tab:** `AppShell` gets a fourth `ShellContent` `Ajustes` (route `ajustes`).
- **Routes:** `Routing.RegisterRoute("preferencias", typeof(PreferenciasPage))` and `("personalizacion", typeof(PersonalizacionPage))`. Both are pushed onto the Ajustes tab with `GoToAsync`, so Shell's back button returns to Ajustes.
- **`AjustesPage`:** two tappable entries (cards using `StationCard`-like styling).
- **Lifetimes:** Ajustes is a singleton page; `Personalización` and `Preferencias` are transient pages and view models.

## Risks / Trade-offs

- **Shell or tab bar colours not refreshing live on a platform** → All Shell colours come from `DynamicResource` style setters. If a platform ignores the change, `AppearanceService` re-applies the Shell colour properties directly after a theme swap. This is checked in groups 3 and 7.
- **Variable fonts render differently per platform** → Verified per platform in group 3. If it fails, implementation stops and asks before switching to static files.
- **Post-filtering two tags can return fewer than `limit` stations** → The API fetches 300 stations from Radio Browser in that case. Narrow combinations may still return few or none, and the specific empty message explains it.
- **Canonicalising against cached lists adds a list request before the first filtered search** → It happens once per list per hour, and only when a filter is present.
- **Users of a dark OS theme see Blanco after the update** → An explicit decision of this phase (OS-following is out of scope); `Negro` is one tap away.
- **Larger text sizes can crowd layouts** (`Enorme` makes titles about 38 units) → Checked page by page on both platforms in group 7. Fixed sizes (artwork) do not scale.
- **Picker lists of about 200 countries** → The native picker scrolls; accepted for a single-choice list.

## Migration Plan

No data migration:
- Existing installs have no `appearance_*` or `pref_*` keys, so they start with `Blanco`, `Ubuntu`, `Normal` and no filters. The default font therefore changes from today's Open Sans to Ubuntu.
- The database and `database/schema.sql` are not touched.
- Rollback is reverting the source; the extra MAUI `Preferences` keys are harmless and ignored by older builds.
