# Tasks

Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android -p:AndroidSdkDirectory=$env:LOCALAPPDATA\Android\Sdk`) with 0 errors and a short summary; stop after each group.

## 1. Palette and licence

- [ ] 1.1 Create `Resources/Themes/TocadiscosPalette.xaml`(+.cs) with the 14 keys and the values of design decision 1; verify with a search that it has exactly the same keys as `ModernoLightPalette.xaml`, and with a contrast script that every pair in the design table is at least 4.5:1
- [ ] 1.2 Create `Resources/Images/Tocadiscos/LICENSE.txt` (original art drawn in XAML for this project, no third-party glyphs) and ship it as `MauiAsset` under `Licences\` in the `.csproj`; verify by build that it is not treated as an image
- [ ] 1.3 Run the three builds with 0 errors

## 2. Audio host, registry and live switching

- [ ] 2.1 Move the single `MediaElement` into `MediaElementAudioPlayer`: it creates the element and connects its handler to the window's `MauiContext` when `App.CreateWindow` builds the window; remove the `MediaElement` content and the attach/detach code from `ReproductorPage`; keep `App.OnSleep` stopping playback. Verify by search that the app has exactly one `MediaElement`, and verify on Windows and Android that:
  - a station started in Moderno `Reproductor` keeps playing in `Inicio`, `Favoritos` and `Ajustes`;
  - only one stream is heard;
  - minimising the app stops it.

  If a platform does not play the element, try the overlay fallback of design decision 3; if that also fails, stop and report.
- [ ] 2.2 Create `Resources/Themes/TocadiscosLayout.xaml`(+.cs) with temporary copies of every Moderno key (`HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `FavoritosTemplate`, `ReproductorTemplate`, `AjustesTemplate`, `SettingsPageTemplate`); register `tocadiscos` "Tocadiscos" with the single palette `standard` (`TocadiscosPalette`, Dark) in `ThemeCatalog`; verify by build
- [ ] 2.3 `AppearanceService` (design decision 9): a single-palette theme does not overwrite `appearance_theme_variant`; switching to a theme uses the stored variant when that theme has it. Verify on Windows with the preferences file:
  - Moderno Oscuro → Tocadiscos → restart → still Tocadiscos, file still holds `dark`;
  - back to Moderno restores Oscuro;
  - `Restablecer` gives Moderno Claro;
  - an unknown theme gives Moderno Claro.
- [ ] 2.4 Verify on Windows and Android that `Personalización` lists `Moderno` and `Tocadiscos`, hides `Variante` for Tocadiscos and that the preview uses its palette; with every page already opened and a station playing, switch Moderno → Tocadiscos → Moderno and check that every page (including `Personalización` and an existing `Reproductor`) follows each time and that the audio is never interrupted. If an existing page does not refresh, add the re-apply fallback of design decision 7 and verify again
- [ ] 2.5 Run the three builds with 0 errors

## 3. View-model members and custom controls

- [ ] 3.1 Add the members of design decision 4 to `InicioViewModel` (`PlayHereCommand`, `PlaybackStatusText`), `FavoritosViewModel` (same) and `ReproductorViewModel` (`Favorites`, `IsLoggedIn`, `HasNoFavorites`, `SelectFavoriteCommand`, `LogInCommand`), keeping every existing member; verify by build and by checking in Moderno that search, selection, play/stop, hearts and previous/next behave as before
- [ ] 3.2 `Views/Controls/MotionSettings.cs` with its Windows and Android parts (design decision 5); verify on Windows by turning animations off in the system settings and reading the value from a temporary label
- [ ] 3.3 `Views/Controls/VinylDisc` (grooves, label with circular logo crop and fallback, spindle, non-rotating shine) and `Views/Controls/Tonearm` (arm drawing, pivot outside the record, invisible hit button at least 56 units wide following the arm, state-dependent description), both with the motion and visibility rules of design decision 6; verify on a temporary page that:
  - toggling the state lowers and raises the arm with the settle;
  - the record speeds up and slows down from its current angle without jumps;
  - hiding the page stops the timer;
  - reduced motion makes the changes instant.
- [ ] 3.4 `Views/Controls/SleeveStack` (three item views, scales 1/0.8/0.65, dimming, swipe, Windows wheel and Up/Down/Enter/Space, `FocusedItem`, `ActivateCommand`, `FocusOnReset`) and `Views/Controls/RecordCrate` (6 slots in two compartments, caption with ellipsis, raised and outlined current record, pages with arrows, indicator and swipe, Windows Left/Right, logged-out cover button); verify the input methods and paging on a temporary page with 100 and with 8 items
- [ ] 3.5 Restyle in `TocadiscosLayout` the `HeaderTemplate`, `BottomBarTemplate` (wooden strip, brass icons, marked section), the new `ReproductorHeaderTemplate` (house `Inicio` on the left), `AjustesTemplate` (two cream sleeve cards with peeking vinyl and brass border) and `SettingsPageTemplate` (rounded wood panel); verify on Windows and Android that:
  - the gear rules and bar marking behave as in Moderno;
  - Personalización and Preferencias keep every control usable;
  - the bar sits above the Android gesture bar.
- [ ] 3.6 Remove the temporary test page; run the three builds with 0 errors

## 4. Inicio and Favoritos

- [ ] 4.1 `StationCardTemplate` of Tocadiscos: the square cream sleeve with logo or fallback, the peeking disc and `SelectedItemBackgroundColor` outline when `IsSelected`; verify that the marker follows the current station
- [ ] 4.2 `InicioTemplate`:
  - header;
  - search row with `Buscar` and `Limpiar búsqueda`;
  - compact filter bar;
  - `SleeveStack` (`FocusOnReset = First`, `ActivateCommand` = `PlayHereCommand`) with the information panel (name up to 3 lines, characteristics, heart bound to `ToggleFavoriteCommand`, `PlaybackStatusText` when the focused item is the current one, vertical scroll);
  - the loading, error, empty and "no results with your preferences" states, centred;
  - bottom bar.
- [ ] 4.3 `FavoritosTemplate`: the account row, the same stack (`FocusOnReset = KeepPosition`, heart bound to `RemoveCommand`), the logged-out text and `Iniciar sesión`, and the loading, error and empty states
- [ ] 4.4 Run the three builds. On Windows, in Tocadiscos, verify:
  - wheel, Up/Down, swipe (touch or emulator) and pressing a smaller sleeve move the focus one station at a time, and the panel follows;
  - the big sleeve plays without leaving the page, and pressing it again stops;
  - the marker follows the current station;
  - a search and a preference change focus the first station, and returning keeps the focus;
  - the heart adds, removes and opens login when logged out;
  - in Favoritos, removing the focused favourite focuses the next (or the previous when last);
  - every state of Moderno's Inicio and Favoritos works;
  - 100 stations stay responsive.

  On Android verify the two columns, swipe and the touch targets.

## 5. Reproductor

- [ ] 5.1 `ReproductorTemplate` of Tocadiscos:
  - `ReproductorHeaderTemplate`;
  - `VinylDisc` sized from its cell, with `LogoSource` and `IsSpinning` = `IsActive`;
  - `Tonearm` with `IsLowered` = `IsActive`, `Command` = `ToggleCommand`, enabled with a station;
  - the information panel with the heart and the status line (`StatusText`, error message, `Reintentar`);
  - the no-station text `Selecciona una emisora en Inicio o en tus favoritas`;
  - the `RecordCrate` bound to `Favorites`, `SelectFavoriteCommand`, `LogInCommand`, `IsLoggedIn` and the empty texts;
  - no bottom bar and no previous/next buttons.
- [ ] 5.2 Run the three builds. On Windows and Android, in Tocadiscos, verify:
  - the tonearm plays and stops, with the landing and settle and the record's speed-up and slow-down from the current angle;
  - an unreachable stream lifts the arm and shows `Error`, the message and `Reintentar`, which works;
  - opening Reproductor while playing shows the arm down and the record at full speed with no landing;
  - leaving the page stops the animation but not the audio;
  - with 8 favourites: page 1 shows 6, then `1/2`, the arrows and swipe show the other 2;
  - the current record is raised and outlined, and the crate opens on its page;
  - pressing a record switches station;
  - removing favourites clamps to the last page;
  - logged out: empty slots, the text, and the crate opens login without adding anything;
  - no favourites: `Aún no tienes emisoras favoritas.`;
  - the house goes to Inicio and the gear to Ajustes.

## 6. Accessibility, motion, sizes, clean-up and README

- [ ] 6.1 Check every interactive element of the Tocadiscos templates and controls for the Spanish descriptions of the `theme-tocadiscos` spec and the minimum touch target; verify the names with UI Automation on Windows and the accessibility tree on Android
- [ ] 6.2 On Windows, keyboard only: Tab follows the visual order on every Tocadiscos page; Up/Down/Enter in the stack, Left/Right/Enter in the crate; the tonearm and arrows are focusable; the focus outline is visible
- [ ] 6.3 With Windows animations off and Android's animator duration scale at 0, verify that the tonearm moves instantly and the record does not rotate, while the state is still shown
- [ ] 6.4 With `Pequeño` and `Enorme`, verify on Windows and Android:
  - the information panel scrolls when needed;
  - crate captions stay on one line with ellipsis;
  - the header and bar stay fixed;
  - the record size does not depend on the text size;
  - nothing is clipped.
- [ ] 6.5 Remove every copied Moderno template from `TocadiscosLayout.xaml`; verify with a search that it does not reference `ModernoLayout` and defines all eight keys itself, and with the colour/font search of Phase 5 that no literal colour, font family or font size exists outside the palettes and the typography defaults
- [ ] 6.6 Re-measure every text/background pair of `TocadiscosPalette` actually used by the templates; update the README:
  - the theme list;
  - the Tocadiscos controls and their licence note;
  - its contrast table;
  - the line saying that animations only observe `PlaybackService.State`;
  - that audio now continues across pages in both themes;
  - new manual check-list items.

## 7. Full run

- [ ] 7.1 Run the three builds and the full README check list on Windows and the Android emulator in Moderno (Claro and Oscuro) and in Tocadiscos, including switching themes while a station plays; report any item that fails

## Workflow follow-up

- Archive the change with `/opsx:archive` after the developer has reviewed it.
