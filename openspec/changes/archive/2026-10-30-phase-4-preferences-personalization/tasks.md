# Tasks

Do not run any git command during this change. Do not connect to or change the database, and do not modify `database/schema.sql`. Stop after each group, run the group's builds and checks, report the actual output, and summarise the files, commands and manual checks before continuing.

## 1. API: catalogue options and exact station filters

- [x] 1.1 Add `OptionDto(Value, Label, StationCount)` and the constant `ContentTypes.All` list (`music`/`MÃºsica`, `news`/`Noticias`, `talk`/`Charlas`, `sports`/`Deportes`) to `OpenWave.Contracts`; verify with `dotnet build src/OpenWave.Contracts`.
- [x] 1.2 Refactor `RadioBrowserClient` so its mirror loop is a private generic method used by the search, and add `GetCountriesAsync`, `GetLanguagesAsync` and `GetTagsAsync` with the parameters in design decision 1. Replace the search parameters with a `RadioBrowserSearch` record that sends `countryExact`/`languageExact`, and `tag`+`tagExact` for one tag or `tagList` for two. Verify with `dotnet build src/OpenWave.Api` and a code review showing the timeout, User-Agent and 502 exception behaviour are unchanged.
- [x] 1.3 Add `ICatalogService`/`CatalogService` (mapping, filtering, sorting and the 60-minute `IMemoryCache` that never caches failures) and `CatalogController` with the four `GET /api/catalog/...` endpoints and 502 handling. Register `AddMemoryCache` and the service. Verify with `dotnet build src/OpenWave.Api`.
- [x] 1.4 Add `ContentType` to `StationQuery` and the `contentType` query parameter. Make `StationService` return `ServiceResult` with the `Tipo de contenido no vÃ¡lido.` validation, the case-insensitive canonicalisation against the cached lists, the 300-station fetch when two tags are combined, and the exact-tag post-filter (design decision 3). Update `StationsController` to use `ToProblem`. Verify with `dotnet build src/OpenWave.Api`.
- [x] 1.5 With the API running, verify in Swagger and report the observed results:
  - **countries:** sorted, no empty names, includes `Spain`.
  - **languages:** at most 100, includes `spanish`/`Spanish`.
  - **genres:** at most 100, sorted, no single-character values.
  - **content-types:** the four options.
  - **cache:** a second `genres` request within 60 minutes adds no Radio Browser request to the log.
  - **502:** run with two invalid mirrors (as in the README check-list item 10); a catalogue list gives 502 with the Spanish detail and content-types still gives 200. After restoring the mirrors, the next request succeeds (failure not cached).
  - **stations:** `country=spain` returns only `Spain`; `tag=pop` excludes `k-pop`-only stations; `tag=jazz&contentType=music` returns only stations with both tags exactly; `contentType=podcast` gives 400 `Tipo de contenido no vÃ¡lido.`; `name=radio&country=Spain&contentType=news` respects all three filters.
- [x] 1.6 Add the catalogue and filter examples to `OpenWave.Api.http` and update the README API section: the catalogue endpoints, exact matching, `contentType` and its 400. Verify every example against the running API.
- [x] 1.7 Run `dotnet build src/OpenWave.Api`, report the actual output, and stop before group 2.

## 2. App: semantic resource keys with DynamicResource

- [x] 2.1 Rename the colour, font and size keys as in design decision 4. Move the light colours into a temporary single theme dictionary `Resources/Themes/WhiteTheme.xaml` (with `x:Class` and code-behind) merged in `App.xaml`, and delete `Colors.xaml` and every `*Dark` key. Verify with the Windows build and a search showing no `AppThemeBinding` and no `*Dark` key left.
- [x] 2.2 Rewrite `Styles.xaml` so every colour, font family and font size setter uses `DynamicResource` with the new keys, including implicit styles for `Shell` (navigation and tab bar), `Label`, `Button`, `Entry`, `Picker`, `RadioButton` and `ActivityIndicator`. `StationNameLabel` uses `FontSizeSubtitle`, and the heart uses `FontSizeTitle` and `FavoriteIconColor`. Verify with the Windows build.
- [x] 2.3 Audit every page and template (Inicio including the selected-card trigger, Reproductor, Favoritos, Iniciar sesiÃ³n, Crear cuenta, AppShell) and every C# view model for literal colours, font families or font sizes, and replace any found with keys. Verify with a search over `src/OpenWave.App` (excluding `bin`, `obj`, `Platforms`, the theme files and `Styles.xaml`) returning no colour literal, `FontFamily=` value or `FontSize=` number.
- [x] 2.4 Run the app on Windows and compare every page (Inicio with a selected station, Reproductor playing, Favoritos logged in and out, Iniciar sesiÃ³n and Crear cuenta with an error) with the previous light look. Report any differences, and verify that nothing became unreadable.
- [x] 2.5 Run the Windows and Android builds, report the actual output, and stop before group 3.

## 3. App: themes, fonts and AppearanceService

- [x] 3.1 Complete `WhiteTheme.xaml` (white background, near-black text) and add `BlackTheme.xaml` (`#000000` background, near-white text, lighter accent) with exactly the same keys. Add `AppThemeDefinition` and `ThemeCatalog` with only these two themes. Verify with the Windows build and a key comparison showing both files define the same set.
- [x] 3.2 Download the fonts from `google/fonts` as in design decision 6, after stating each file name, source URL and size to the developer:
  - `ufl/ubuntu/Ubuntu-Regular.ttf` and its `UFL.txt`, saved as `Ubuntu-UFL.txt`.
  - `ofl/caladea/Caladea-Regular.ttf`.
  - `ofl/dosis/Dosis[wght].ttf`, saved as `Dosis-Variable.ttf`.
  - `ofl/cormorantgaramond/CormorantGaramond[wght].ttf`, saved as `CormorantGaramond-Variable.ttf`.
  - `ofl/recursive/Recursive[CASL,CRSV,MONO,slnt,wght].ttf`, saved as `Recursive-Variable.ttf`.
  - `ofl/pixelifysans/PixelifySans[wght].ttf`, saved as `PixelifySans-Variable.ttf`.
  - The five `OFL.txt` files, saved as `<Family>-OFL.txt`.

  Remove the unused `OpenSans-*.ttf` files and their registrations.

  Change the `MauiFont` include to `*.ttf`, add the licences as content, and register the six aliases in `ConfigureFonts`. If any download fails, stop and tell the developer which files to place where. Verify with the Windows build and a listing of `Resources/Fonts`.
- [x] 3.3 Add `AppearanceOptions` (fonts and the five size presets) and `AppearanceService` (`Initialize`, `SetTheme`, `SetFontFamily`, `SetFontScale`, `ResetToDefaults`, `Preferences` storage with fallbacks for unknown or corrupt values). Register it, call `Initialize()` in the `App` constructor before the window is created, and remove the temporary `App.xaml` theme merge. Verify with the Windows and Android builds.
- [x] 3.3b Add the bold font files and `AppFontFamilyBold`, and make the header title and tab labels follow the font and size (design decisions 6 and 6b), after stating each extra download to the developer. Verify with the Windows and Android builds.
- [x] 3.4 Verify on Windows and Android with a temporary call or the debugger, then remove it, and report the observed results:
  - Switching to `Negro` recolours the open page and the tab bar without restarting, and native inputs turn dark.
  - Each of the six fonts renders regular and bold text, and Pixelify Sans and Cormorant Garamond stay readable at `Normal` and `PequeÃ±o`.
  - `Enorme` enlarges the text.
  - After a restart the stored choices appear on the first page with no flash.
  - A corrupt `appearance_font_scale` value starts with the defaults.
- [x] 3.5 Add a README section "Themes and fonts": the colour keys, the two themes, the six fonts and their licences (Ubuntu Font Licence for Ubuntu, SIL OFL for the rest), the size presets, the storage keys, and how to add a theme (one dictionary plus one `ThemeCatalog` entry). Verify that the steps match the code.
- [x] 3.6 Run the Windows and Android builds, report the actual output, and stop before group 4.

## 4. App: Ajustes tab and PersonalizaciÃ³n page

- [x] 4.1 Add the `Ajustes` tab after `Favoritos`, `AjustesPage` with the `Preferencias` and `PersonalizaciÃ³n` entries, and the `personalizacion` and `preferencias` routes. The `preferencias` route points to a placeholder until group 5. Verify with the Windows build.
- [x] 4.2 Add `PersonalizacionViewModel`/`PersonalizacionPage`:
  - Theme radio group from `ThemeCatalog`.
  - Font radio group, each option shown in its own font.
  - Size radio group.
  - Preview card and `Restablecer valores por defecto`.
  - Changes are applied and stored immediately.

  Verify with the Windows and Android builds.
- [x] 4.3 On Windows, verify and report:
  - The four tabs are in order, and each Ajustes entry opens and the back button returns to Ajustes.
  - Every selector change applies at once on PersonalizaciÃ³n, the other pages and the tab bar.
  - Font names render in their own fonts.
  - `Restablecer valores por defecto` restores `Blanco`, `Ubuntu` and `Normal`.
  - Choices survive a restart.
  - Logging in or out does not change the appearance.
- [x] 4.4 Add a README section "Ajustes and PersonalizaciÃ³n" describing the tab, the page and every Spanish label. Update the manual check list, and verify every string against the running app.
- [x] 4.5 Run the Windows and Android builds, report the actual output, and stop before group 5.

## 5. App: PreferencesService and Preferencias page

- [x] 5.1 Add `UserPreferences` and `PreferencesService` (`Current`, `Save`, `Reset`, `Changed`, in-memory `ApplyInInicio` defaulting to true, `pref_*` keys; null removes the key) and the shared summary-label helper. Register them; verify with the Windows build.
- [x] 5.2 Add `CatalogApiClient` (typed `HttpClient`, same base URL and timeout, mapping errors to `StationRequestException`); register it and verify with the Windows build.
- [x] 5.3 Add `PreferenciasViewModel`/`PreferenciasPage`:
  - Four pickers with `Todos` first, and the saved values selected (added as an option when missing from the list).
  - Loading indicator, and an error with the two Spanish messages and `Reintentar`.
  - `Guardar` (enabled only when the lists are loaded) shows `Preferencias guardadas.`
  - `Restablecer` always works.

  Replace the group 4 placeholder route, and verify with the Windows and Android builds.
- [x] 5.4 On Windows, verify and report:
  - Saving and reopening shows the saved values, including after an app restart.
  - `Restablecer` clears all four.
  - A saved value missing from the list is still shown and kept.
  - With the API stopped, the page shows `No hay conexiÃ³n con el servidor.` and `Reintentar`, and the saved values are unchanged.
  - With two invalid mirrors, it shows `No se pudo obtener el catÃ¡logo de emisoras.`
  - `Reintentar` recovers.
  - Preferences work logged out and survive a logout.
- [x] 5.5 Add a README section "Preferencias" describing the four pickers, saving, resetting, the error states and the storage keys. Verify every string against the running app.
- [x] 5.6 Run the Windows and Android builds, report the actual output, and stop before group 6.

## 6. App: preferences in Inicio

- [x] 6.1 Extend the app's `IStationService.SearchAsync` with optional filters that add `country`, `language`, `tag` and `contentType` to the request; verify with the Windows build.
- [x] 6.2 Update `InicioViewModel`/`InicioPage`:
  - Send the saved filters when they are applied, and reload on the next appearance after `Changed`.
  - The `Filtros activos` bar with its summary and `Desactivar`, and `Preferencias desactivadas` with `Activar`.
  - The `No se encontraron emisoras con tus preferencias.` empty state with `Desactivar`, keeping the generic empty message when no filter is applied.

  Verify with the Windows and Android builds.
- [x] 6.3 On Windows, verify and report:
  - With the country `Spain` saved, Inicio lists only Spanish stations and shows `Filtros activos` with `Spain`.
  - A combined genre and content type shows only matching stations.
  - A name search keeps the filters.
  - `Desactivar` lists unfiltered stations and shows `Preferencias desactivadas`, while Preferencias still shows the values.
  - `Activar` re-applies them.
  - After a restart the filters are active again.
  - A combination with no stations shows the specific empty message.
  - With no preferences saved, no bar is shown.
  - Favoritos and Reproductor are unaffected.
- [x] 6.4 Update the README sections for Inicio (filters, the bar and its messages) and the manual check list, and verify every string against the running app.
- [x] 6.5 Run the Windows and Android builds, report the actual output, and stop before group 7.

## 7. Cross-platform integration and final verification

- [x] 7.1 On Windows, run the full flow in both themes and at `PequeÃ±o`, `Normal` and `Enorme`: every tab, login and register pages, Ajustes, both settings pages, Inicio with and without filters, playback and Favoritos. Report any unreadable or clipped element and fix it.
- [x] 7.2 On the Android emulator (deploying with `-t:Install` or `-t:Run`), run the same flow in both themes and at least two font sizes. Check the tab bar and native pickers in `Negro`, and that preferences and appearance survive an app restart. Report the observed results with screenshots of each theme.
- [x] 7.3 Verify that no preference or appearance value is sent to the API except the station filters, and that the database and `database/schema.sql` were not modified (file listing and search; no database connection). Report only what was found.
- [x] 7.4 Run `dotnet build OpenWave.sln` with `ANDROID_HOME` set, confirm 0 warnings and 0 errors for every project, and report the actual output.
- [x] 7.5 Re-read the whole README against the implemented behaviour, correct stale text, and verify the corrected commands.
- [x] 7.6 Report the final files changed, commands run and manual checks performed, then stop without running any git command.

## Workflow follow-up

- Sync the delta specs and archive the change with `/opsx:archive` after the developer has reviewed it.
