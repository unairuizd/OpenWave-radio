# Tasks

Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and a short summary of files, commands and what to test manually.

## 1. App localization foundation

- [ ] 1.1 Create `Resources/Strings/AppResources.resx` (Spanish, neutral) and `AppResources.en.resx` with the generated accessor, and verify the Windows build includes the `en` satellite assembly
- [ ] 1.2 Add `Services/Localization/LanguageCatalog.cs` (registry with `es` Español and `en` English) and `Localizer.cs` (singleton indexer that sets `CurrentUICulture`/`CurrentCulture` and raises `Item[]`), and verify a missing English key returns the Spanish text
- [ ] 1.3 Add the `{loc:Translate Key}` markup extension and register the `loc` namespace, and verify a test label on `Ajustes` changes text when the culture is switched from code
- [ ] 1.4 Store the language in `AppearanceService` (new `Preferences` key, unknown code falls back to `es`, applied at start before the first page), and verify the app restarts in the stored language
- [ ] 1.5 Add `AcceptLanguageHandler` and attach it to the four typed `HttpClient`s in `MauiProgram.cs`, and verify with the API log (or a breakpoint) that requests carry `Accept-Language: es`
- [ ] 1.6 Run the three builds and fix errors

## 2. API localization

- [ ] 2.1 Add `AddLocalization` and `UseRequestLocalization` (cultures `es`, `en`, default `es`, Accept-Language provider only) and `Resources/Messages.resx` / `Messages.en.resx`, and verify a request without the header still returns Spanish
- [ ] 2.2 Move every ProblemDetails `detail` text in controllers, services and `DatabaseExceptionFilter` to `Messages` via `IStringLocalizer<Messages>`, keeping status codes, and verify with `curl -H "Accept-Language: en"` that a bad login returns the English detail and `fr` returns Spanish
- [ ] 2.3 Build the content type labels from the localizer (values unchanged; country/language/genre lists still cached once), and verify `GET /api/catalog/content-types` in `es` and `en`
- [ ] 2.4 Search the API for remaining Spanish user-facing literals (`grep` for accented words and quoted sentences) and verify none is left outside `Messages*.resx`
- [ ] 2.5 Run the three builds and fix errors

## 3. Shared bottom bar and settings header

- [ ] 3.1 Rewrite `Views/BottomBar.cs` to draw the house, radio and star icons with `Path` geometry, coloured by `TabBarBackgroundColor`, `TabBarSelectedColor` and `TabBarUnselectedColor`, with localized accessible names, Windows tooltips and the selected state, and verify in Moderno that the marked icon follows the section
- [ ] 3.2 Remove `BottomBarTemplate` from the eight layouts and from the required keys in `ThemeCatalog`, keep each page's bar presence rules (no bar in Reproductor except Rueda, under the wheel there), and verify the bar in every theme on Windows
- [ ] 3.3 Add the shared settings header view (back arrow `Atrás` running `GoToAsync("..")`, title as written, gear) and verify on Windows that the arrow on `Ajustes` opened from Reproductor returns to Reproductor
- [ ] 3.4 Run the three builds and fix errors

## 4. Shared Ajustes page

- [ ] 4.1 Rewrite `AjustesPage.xaml` with the shared header, rows `Personalización` then `Preferencias` (icon, title, description, `›`) and the bottom bar, using only palette, font and size keys and localized texts, and verify the order and descriptions in all eight themes
- [ ] 4.2 Remove `AjustesTemplate` from the eight layouts and `ThemeCatalog`, and verify switching theme only recolours `Ajustes`
- [ ] 4.3 Run the three builds and fix errors

## 5. Shared Personalización page

- [ ] 5.1 Rewrite `PersonalizacionPage.xaml` as a fixed preview row (sample card with `Reproducir` and heart) above a scrolling options area, and verify the preview stays visible while scrolling on Android
- [ ] 5.2 Add the theme cards grid with the theme name and swatches read from each theme's palette, and the variant segmented control, and verify choosing a card applies the theme at once
- [ ] 5.3 Add the font chips (each in its own font, first three plus `+N`, a hidden selected font still shown) and the five-step size control with its localized name, and verify both apply at once
- [ ] 5.4 Add the `Idioma` choice listing `LanguageCatalog`, and verify switching to English changes the page, header and bottom bar texts at once without leaving the page
- [ ] 5.5 Make `Restablecer valores por defecto` also restore `Español`, and verify it on Windows
- [ ] 5.6 Remove `SettingsPageTemplate` and `SettingsPanelStyle` from the eight layouts and `ThemeCatalog`, and verify Personalización in all eight themes
- [ ] 5.7 Run the three builds and fix errors

## 6. Shared Preferencias page and filter pages

- [ ] 6.1 Rework `PreferenciasViewModel` with saved and pending values, the summary (`Inicio mostrará`, `Emisoras de … en … · …` or `Todas las emisoras`) from resource formats, `Descartar`, `Guardar` (`Preferencias guardadas.`) and `Restablecer`, and verify the unsaved changes bar appears and disappears correctly
- [ ] 6.2 Rewrite `PreferenciasPage.xaml` with the summary, the groups `Dónde y en qué idioma` (País, Idioma) and `Qué quieres escuchar` (Género, `Tipo de contenido` chips) and the bar, and verify the layout against the reference image
- [ ] 6.3 Add `FiltroPage` + `FiltroViewModel` on route `filtro` (search box, `Todos` / `sin filtro`, `Recientes`, full list with station counts in the app's number format, tick on the pending value, callback and `GoToAsync("..")`), and verify choosing `Austria` returns with the row updated and the bar shown
- [ ] 6.4 Add the case- and accent-insensitive search with `Sin resultados`, and verify `belgica` finds `Bélgica`
- [ ] 6.5 Store the last 5 saved values per filter in `Preferences` (newest first, no duplicates, `Todos` excluded), and verify `Recientes` after two saves
- [ ] 6.6 Keep the saved-value-missing behaviour of the old page, and verify it with a stored value not in the list
- [ ] 6.7 Run the three builds and fix errors

## 7. Translate the rest of the app

- [ ] 7.1 Move every user-visible text of the themed pages and layouts (titles, buttons, accessible names, tooltips, empty and error states, Mesa de DJ booth labels) to `AppResources` with English translations, and verify each theme in English on Windows
- [ ] 7.2 Move every user-visible text in view models, services and controls (`PreferenceLabels`, `AccountRules`, session and playback messages, theme, palette and size names) to `AppResources`, and verify a wrong password shows the English message in English
- [ ] 7.3 Reload the app's cached content type labels when the language changes, and verify the chips switch language
- [ ] 7.4 Search the app for remaining Spanish literals in `Text=`, `Title=`, `Description=`, `ToolTipProperties.Text=` and C# strings shown to the user, and verify none is left outside the resources
- [ ] 7.5 Run the three builds and fix errors

## 8. Documentation and conventions

- [ ] 8.1 Update `openspec/config.yaml` so user-facing strings are "in the app language, Spanish (es-ES) by default, from the resource files", and verify `openspec validate --all --strict` still passes
- [ ] 8.2 Update the README theme section (layout keys now: `HeaderTemplate`, `StationCardTemplate`, `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`) and add "Adding a language" (app resx, API resx, two registry lines), and verify the steps match the code
- [ ] 8.3 Update the theme briefs and the comment at the top of each layout file to drop the removed keys, and verify `grep` finds no `AjustesTemplate`, `SettingsPageTemplate`, `SettingsPanelStyle` or `BottomBarTemplate` left

## 9. Integration check

- [ ] 9.1 Manual run on Windows and on the Android emulator: in every theme, open `Ajustes` from Inicio, Reproductor and Favoritos and use the back arrows, change theme, font, size and language, save and discard preferences, use a filter page with search and recent values, and verify Inicio loads with the saved filters in both languages
- [ ] 9.2 Run the three builds a last time and report the real output

## Workflow follow-up

- Archive the change with `/opsx:archive` after the manual checks pass.
