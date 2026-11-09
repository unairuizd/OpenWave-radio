# Design

## Context

See proposal.md (Why). Current state that shapes the approach:

- Each theme is a palette dictionary (14 colour keys) plus a layout dictionary in `Resources/Themes/<Theme>Layout.xaml` with the keys `HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`, `AjustesTemplate`, `SettingsPageTemplate` and the optional `SettingsPanelStyle`. `ThemeCatalog` registers the themes and `AppearanceService` swaps the merged dictionaries.
- `AjustesPage`, `PersonalizacionPage` and `PreferenciasPage` are `ContentPage`s whose content is a `ControlTemplate` from the layout. `BottomBar` (`Views/BottomBar.cs`) hosts `BottomBarTemplate`.
- Navigation: the gear runs `AppNavigation.OpenAjustesCommand`, which pushes the `ajustes` route on the current section's stack. `personalizacion` and `preferencias` are pushed on top of it. Popping the stack therefore already returns to the origin page.
- `PreferenciasViewModel` uses four Pickers fed by `CatalogApiClient` (`OptionDto(Value, Label, StationCount)`) and saves through `PreferencesService` (MAUI `Preferences`).
- All user texts are Spanish literals: about 134 in XAML and 125 in C# view models and services in the app, and about 26 `detail` texts plus the content type labels in the API.
- The app's four typed `HttpClient`s are registered in `MauiProgram.cs`. The API returns ProblemDetails built in its controllers and services.

## Goals / Non-Goals

**Goals:**
- One settings implementation, drawn only with palette keys, for all eight themes.
- One `BottomBar` control that draws its own icons, used by every theme.
- A small localization mechanism, the same idea in the app and the API, where a new language is a new `.resx` file plus one registry line.

**Non-Goals:**
- Translating station data (names, countries, languages, tags) or the country/language/genre labels that come from Radio Browser.
- Right-to-left layout, plural rules beyond what the texts need, or per-user language stored in the database.
- Changing the header, station card or Inicio/Reproductor/Favoritos templates of the themes, apart from translating their texts.

## Decisions

### Shared settings pages instead of per-theme templates

The settings pages get their own XAML content (no `ControlTemplate` lookup) built only from `DynamicResource` palette keys, the font and size keys, and shared styles in `Resources/Styles/Styles.xaml`. `AjustesTemplate`, `SettingsPageTemplate` and `SettingsPanelStyle` are deleted from the eight layouts and from the list of required keys in `ThemeCatalog` and the README.
*Alternative:* keep the template keys and point every layout at one shared template. That keeps eight copies of the wiring and lets a theme diverge again, so it was rejected.

### Shared settings header

The settings pages use one shared header view (back arrow, title as written, gear) instead of the layout's `HeaderTemplate`, because the themed headers write the title in upper case and some add decorations (`EN DIRECTO`, handle, battery) that the settings pages must not show. The back arrow has the accessible name `Atrás` and calls `Shell.Current.GoToAsync("..")`. Because `Ajustes` is pushed on the section's stack, popping returns to Inicio, Reproductor or Favoritos without storing the origin.
*Alternative:* remember the origin route in a field. Not needed while the stack already holds it, and it could get out of date when the bottom bar switches sections.

### Bottom bar drawn by the control

`BottomBar` builds its three buttons in code or in one shared XAML view: house, radio and star glyphs drawn as `Path` data (no new icon font, no new package), coloured with the existing palette keys `TabBarBackgroundColor`, `TabBarSelectedColor` (marked) and `TabBarUnselectedColor`, so no palette changes. Accessible names and Windows tooltips come from the localizer. Each page keeps deciding whether it shows the bar, so the presence rules do not change. `BottomBarTemplate` is removed from the layouts.
*Alternative:* an icon font such as Material Symbols. That adds a font file and licence notes for three icons, so `Path` geometry is simpler.

### App localization: resx + one localizer

- `Resources/Strings/AppResources.resx` holds the Spanish texts (the default, neutral resource) and `AppResources.en.resx` the English ones. Spanish being the neutral resource means any key missing from a language falls back to Spanish automatically.
- `Services/Localization/LanguageCatalog.cs` is the registry: a list of `(code, displayName)`, starting with `("es", "Español")` and `("en", "English")`. The `Idioma` choice in `Personalización` lists this registry. Language names are shown in their own language.
- `Services/Localization/Localizer.cs` is a singleton `ObservableObject` with an indexer `this[string key]`. It sets `CultureInfo.CurrentUICulture` and `CurrentCulture` and then raises `PropertyChanged` for `Item[]`. XAML binds with a small markup extension, `{loc:Translate Key}`, which creates a binding to `Localizer.Instance[Key]`, so every open page updates at once without being rebuilt. C# code (view models, services) uses `Localizer.Instance["Key"]` or `string.Format` with it.
- Because number formatting follows `CurrentCulture`, the station counts read `1.204` in Spanish and `1,204` in English with no extra code.
- The language is stored by `AppearanceService` under a new `Preferences` key next to theme, variant, font and size, and `Restablecer valores por defecto` resets it to `es`. An unknown stored code falls back to `es`.
- Theme names, palette names, font size names and the content type labels shown before the catalogue loads are resource keys too.
*Alternative:* JSON dictionaries loaded at start. They need custom fallback and parsing; resx is built into .NET and the MAUI tooling, so it is the simpler choice.

### Language sent to the API

A `DelegatingHandler` (`AcceptLanguageHandler`) adds `Accept-Language: <current code>` to every request and is attached to the four typed `HttpClient`s in `MauiProgram.cs`. When the language changes, the catalogue lists cached in the app (content types) are reloaded so their labels follow.

### API localization

- `builder.Services.AddLocalization()` and `app.UseRequestLocalization(...)` with supported cultures `es` and `en`, default `es`, using only the `Accept-Language` provider.
- `Resources/Messages.resx` (Spanish, neutral) and `Messages.en.resx` hold every `detail` text and the content type labels. Services get `IStringLocalizer<Messages>` and use it where they currently write literals. Status codes and the content type values (`music`, `news`, `talk`, `sports`) do not change.
- The catalogue cache keeps per-language content type lists (they are a static list, so they are built on each call from the localizer). Country, language and genre lists stay cached once, since they are not translated.
- Adding a language to the API means one `Messages.<code>.resx` and one entry in the supported cultures list.

### Preferencias with pending changes and a filter page

- `PreferenciasViewModel` keeps a saved snapshot and pending values. The `Cambios sin guardar` bar is visible while pending differs from saved. `Descartar` copies saved to pending; `Guardar` saves pending and shows `Preferencias guardadas.`.
- The `País`, `Idioma` and `Género` rows push a new route `filtro` (`FiltroPage` + `FiltroViewModel`) with the filter kind as a query parameter. The page reads the already loaded lists from `CatalogApiClient`, so no new API call is needed.
- `PreferenciasViewModel` passes the filter kind, the current pending value and a callback (`Action<string>`) in the Shell navigation parameters; `FiltroViewModel` calls the callback with the chosen value and navigates with `GoToAsync("..")`. Pending values survive because `PreferenciasPage` and its view model are still on the stack.
- Searching compares strings after removing diacritics (`string.Normalize(FormD)` and dropping non-spacing marks) and lowering with the invariant culture, so `belgica` finds `Bélgica`.
- Recent values: on `Guardar`, each saved filter value that is not `Todos` is put first in a per-filter list stored in `Preferences` as a joined string, without duplicates, limited to 5.
- The summary (`Inicio mostrará` + `Emisoras de <country> en <language> · <genre>` or `Todas las emisoras`) is a computed property built from the pending values and resource format strings, so each language can order the words its own way.

### Personalización

- The page is a `Grid` with the preview in a fixed row and the options in a `ScrollView` below, so the preview stays visible.
- Theme cards are a `CollectionView` (grid) whose items show the theme's display name and a few swatches. The swatch colours are read from the theme's palette dictionary in `ThemeCatalog` (instantiated once), so they show the theme's own colours, not the current palette.
- Font chips: the first three fonts plus a `+N` chip that expands the list; when the selected font is one of the hidden ones it is shown as a chip as well.
- The size is a five-segment control bound to the existing five size steps; the name under it comes from resources.
- These controls are plain XAML (Border, Grid, TapGestureRecognizer, VisualStates) in shared styles; no new package.

## Risks / Trade-offs

- [Moving about 285 texts to resources can miss some or break bindings] → Each group of tasks ends with a check that searches the app and the API for remaining Spanish literals in user-visible places, plus a manual run in English through every page and theme.
- [The bindable indexer does not refresh texts set once in code-behind] → Code-behind texts are moved to bindings or set again on the localizer's `PropertyChanged`.
- [Removing four layout keys from eight themes is a wide edit] → The keys are removed in one group, followed by all three builds and a theme-by-theme visual check.
- [Accept-Language from a device in another language] → The app always sends its own chosen language, not the device's, so the API answer matches the screen.
- [The theme specs and briefs still describe the old settings looks] → The delta specs remove those requirements; the briefs and README are updated in the docs task.

## Migration Plan

No data migration. Existing appearance preferences keep working; a device without a stored language starts in `Español`. Rollback is reverting the change; the extra `Preferences` keys (language, recent values) are ignored by the old version.

## Assumptions

Recorded from the planning conversation; none changes the specs:

- The settings headers show their titles as written (`Ajustes`, `Personalización`), following the images.
- The bottom bar shows icons only, with accessible names and Windows tooltips.
- Where each theme shows the bar does not change (no bar in Reproductor in six themes; the Rueda bar sits under the wheel).
- `Restablecer valores por defecto` also restores `Español`; `Restablecer` stays in `Preferencias`.
- Country, language and genre labels stay as Radio Browser gives them (for example `Spain`).
- The `EN DIRECTO` tag of Mesa de DJ appears only on that theme's own pages.
- Recent values are the last 5 saved values per filter.
