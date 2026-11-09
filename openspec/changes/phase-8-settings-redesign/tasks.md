# Tasks

Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and a short summary of files, commands and what to test manually.

## 1. App localization foundation

- [x] 1.1 Create `Resources/Strings/AppResources.resx` (Spanish, neutral) and `AppResources.en.resx`, read through a `ResourceManager` (no generated accessor: XAML and code look texts up by key), and verify the Windows build includes the `en` satellite assembly
- [x] 1.2 Add `Services/Localization/LanguageCatalog.cs` (registry with `es` Español and `en` English) and `Localizer.cs` (singleton indexer that sets `CurrentUICulture`/`CurrentCulture` and raises `Item[]`), and verify a missing English key returns the Spanish text
- [x] 1.3 Add the `{loc:Translate Key}` markup extension and use it for the page titles, and verify the header shows `HOME` with `en` stored (switching live is checked in 5.4, when `Idioma` exists)
- [x] 1.4 Store the language in `AppearanceService` (new `Preferences` key, unknown code falls back to `es`, applied at start before the first page), and verify the app restarts in the stored language
- [x] 1.5 Add `AcceptLanguageHandler` and attach it to the four typed `HttpClient`s in `MauiProgram.cs`, and verify with the API log (or a breakpoint) that requests carry `Accept-Language: es`
- [x] 1.6 Run the three builds and fix errors

## 2. API localization

- [x] 2.1 Add `AddLocalization` and `UseRequestLocalization` (cultures `es`, `en`, default `es`, Accept-Language provider only) and `Resources/Messages.resx` / `Messages.en.resx`, and verify a request without the header still returns Spanish
- [x] 2.2 Move every ProblemDetails `detail` text in controllers, services and `DatabaseExceptionFilter` to `Messages` (services return a message key, translated with `IStringLocalizer<Messages>` where the ProblemDetails is built), keeping status codes, and verify with `curl -H "Accept-Language: en"` that an invalid login email returns the English detail and `fr` returns Spanish
- [x] 2.3 Build the content type labels from the localizer (values unchanged; country/language/genre lists still cached once), and verify `GET /api/catalog/content-types` in `es` and `en`
- [x] 2.4 Search the API for remaining Spanish user-facing literals (`grep` for accented words and quoted sentences) and verify none is left outside `Messages*.resx`
- [x] 2.5 Run the three builds and fix errors

## 3. Shared bottom bar and settings header

- [x] 3.1 Rewrite `Views/BottomBar.cs` to draw the existing house, radio and star icons (`ic_home`, `ic_player`, `ic_favorites`), coloured by `TabBarBackgroundColor`, `TabBarSelectedColor` and `TabBarUnselectedColor`, with localized accessible names, Windows tooltips and the selected state, and verify in Moderno that the marked icon follows the section
- [x] 3.2 Remove `BottomBarTemplate` from the eight layouts and from the required keys in `ThemeCatalog`, keep each page's bar presence rules (no bar in Reproductor except Rueda, under the wheel there), and verify the bar in every theme on Windows
- [x] 3.3 Add the shared settings header view (back arrow `Atrás` running `GoToAsync("..")`, title as written, gear) and verify on Windows that the arrow on `Ajustes` opened from Reproductor returns to Reproductor
- [x] 3.4 Run the three builds and fix errors

## 4. Shared Ajustes page

- [x] 4.1 Rewrite `AjustesPage.xaml` with the shared header, rows `Personalización` then `Preferencias` (icon, title, description, `›`) and the bottom bar, using only palette, font and size keys and localized texts, and verify the order and descriptions in all eight themes
- [x] 4.2 Remove `AjustesTemplate` from the eight layouts and `ThemeCatalog`, and verify switching theme only recolours `Ajustes`
- [x] 4.3 Run the three builds and fix errors

## 5. Shared Personalización page

- [x] 5.1 Rewrite `PersonalizacionPage.xaml` as a fixed preview row (sample card with `Reproducir` and heart) above a scrolling options area, and verify the preview stays visible while scrolling on Android
- [x] 5.2 Add the theme cards grid with the theme name and swatches read from each theme's palette, and the variant segmented control, and verify choosing a card applies the theme at once
- [x] 5.3 Add the font chips (each in its own font, first three plus `+N`, a hidden selected font still shown) and the five-step size control with its localized name, and verify both apply at once
- [x] 5.4 Add the `Idioma` choice listing `LanguageCatalog`, and verify switching to English changes the page, header and bottom bar texts at once without leaving the page
- [x] 5.5 Make `Restablecer valores por defecto` also restore `Español`, and verify it on Windows
- [x] 5.6 Remove `SettingsPageTemplate` and `SettingsPanelStyle` from the eight layouts and `ThemeCatalog`, and verify Personalización in all eight themes
- [x] 5.7 Run the three builds and fix errors

## 6. Shared Preferencias page and filter pages

- [x] 6.1 Rework `PreferenciasViewModel` with saved and pending values, the summary (`Inicio mostrará`, `Emisoras de … en … · …` or `Todas las emisoras`) from resource formats, `Descartar`, `Guardar` (`Preferencias guardadas.`) and `Restablecer`, and verify the unsaved changes bar appears and disappears correctly
- [x] 6.2 Rewrite `PreferenciasPage.xaml` with the summary, the groups `Dónde y en qué idioma` (País, Idioma) and `Qué quieres escuchar` (Género, `Tipo de contenido` chips) and the bar, and verify the layout against the reference image
- [x] 6.3 Add `FiltroPage` + `FiltroViewModel` on route `filtro` (search box, `Todos` / `sin filtro`, `Recientes`, full list with station counts in the app's number format, tick on the pending value, callback and `GoToAsync("..")`), and verify choosing `Austria` returns with the row updated and the bar shown
- [x] 6.4 Add the case- and accent-insensitive search with `Sin resultados`, and verify `ÁUS` finds `Australia` and `Austria` (the catalogue gives English country names, so accents are tested in the query)
- [x] 6.5 Store the last 5 saved values per filter in `Preferences` (newest first, no duplicates, `Todos` excluded), and verify `Recientes` after two saves
- [x] 6.6 Keep the saved-value-missing behaviour of the old page, and verify it with a stored value not in the list
- [x] 6.7 Run the three builds and fix errors

## 7. Translate the rest of the app

- [x] 7.1 Move every user-visible text of the themed pages and layouts (titles, buttons, accessible names, tooltips, empty and error states, Mesa de DJ booth labels) to `AppResources` with English translations, and verify each theme in English on Windows
- [x] 7.2 Move every user-visible text in view models, services and controls (`PreferenceLabels`, `AccountRules`, session and playback messages, theme, palette and size names) to `AppResources`, and verify an invalid email shows the English message in English (the API's own English messages were checked with curl in 2.2, without touching the database)
- [x] 7.3 Show content type labels in the app language (the chips come from the API in the request language each time Preferencias opens; Inicio's filter summary reads them from the resources and refreshes on a language change), and verify both in English
- [x] 7.4 Search the app for remaining Spanish literals in `Text=`, `Title=`, `Description=`, `ToolTipProperties.Text=` and C# strings shown to the user, and verify none is left outside the resources
- [x] 7.5 Run the three builds and fix errors

## 8. Documentation and conventions

- [x] 8.1 Update `openspec/config.yaml` so user-facing strings are "in the app language, Spanish (es-ES) by default, from the resource files", and verify `openspec validate phase-8-settings-redesign --strict` passes and `openspec validate --all --strict` shows only the long-requirement warnings the main specs already had
- [x] 8.2 Update the README theme section (layout keys now: `HeaderTemplate`, `StationCardTemplate`, `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`) and add "Adding a language" (app resx, API resx, two registry lines), and verify the steps match the code
- [x] 8.3 Update the theme briefs and the comment at the top of each layout file to drop the removed keys, and verify `grep` finds no `AjustesTemplate`, `SettingsPageTemplate`, `SettingsPanelStyle` or `BottomBarTemplate` left

## 9. Integration check

- [x] 9.1 Manual run on Windows and on the Android emulator: in every theme, open `Ajustes` from Inicio, Reproductor and Favoritos and use the back arrows, change theme, font, size and language, save and discard preferences, use a filter page with search and recent values, and verify Inicio loads with the saved filters in both languages
- [x] 9.2 Run the three builds a last time and report the real output

## Workflow follow-up

- Archive the change with `/opsx:archive` after the manual checks pass.
