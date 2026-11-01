# Tasks

Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android` with `-p:AndroidSdkDirectory=$env:LOCALAPPDATA\Android\Sdk` when `ANDROID_HOME` is not set) and a short summary; stop after each group.

## 1. Palettes, registry and persistence

- [x] 1.1 Create `Resources/Themes/ModernoLightPalette.xaml`(+.cs) and `ModernoDarkPalette.xaml`(+.cs) with the same 14 keys and values as `WhiteTheme`/`BlackTheme`, then delete `WhiteTheme.*` and `BlackTheme.*`; verify with a search that nothing references `WhiteTheme` or `BlackTheme`
- [x] 1.2 Create an empty `Resources/Themes/ModernoLayout.xaml`(+.cs) `x:Class` dictionary so the registry can reference it; verify it compiles
- [x] 1.3 In `ThemeCatalog.cs` add `ThemePalette` and change `AppThemeDefinition` to `(Id, DisplayName, LayoutDictionaryType, Palettes)` with a palette lookup; register only `moderno` "Moderno" with `light` "Claro" and `dark` "Oscuro"; `DefaultId = "moderno"`, `DefaultPaletteId = "light"`; verify by build
- [x] 1.4 In `AppearanceService` add `PaletteId`, the `appearance_theme_variant` key, `SetPalette`, `SetTheme` keeping the palette when possible, migration `white`→`moderno`+`light` and `black`→`moderno`+`dark` (written back), fallback to Moderno Claro for unknown or corrupt values, and `ResetToDefaults` → Moderno Claro; merge the palette dictionary (layout swap comes in group 2); verify by build
- [x] 1.5 Make `PersonalizacionViewModel`/`Page` compile against the new registry (the theme list shows `Moderno`; the variant choice comes in group 7)
- [x] 1.6 Run the three builds with 0 errors; on Windows, write `black` into the preferences file and verify the app starts dark and the file now holds `moderno` + `dark`; repeat with `white` (light) and with an unknown value (Moderno Claro, no crash)

## 2. Icons, header, bottom bar and navigation

- [x] 2.1 Draw the 12 original 24×24 single-colour SVG icons in `Resources/Images/` (gear, search, clear, home, player, favourites, heart filled, heart outline, play, stop, previous, next) and add `Icons-LICENSE.txt` saying they are original work of this project; change the `.csproj` image item to `Resources\Images\*.svg` and ship `Resources\Images\*.txt` as `MauiAsset` under `Licences\`; verify by build that every icon is generated
- [x] 2.2 Add to `Styles.xaml` the tokens `MinTouchTarget` (48, replacing the 44 literals), `CardMinWidth` (170), `CardSpacing`, `PlayerArtworkMaxSize` and the styles `IconButton`, `AccentIconButton` and `SettingsCardButton`; verify with a search that no literal colour, font or font size was added outside the palettes and the typography defaults
- [x] 2.3 Create `Services/AppNavigation.cs`: the `Section` attached property (`None`, `Inicio`, `Reproductor`, `Favoritos`), `OpenAjustesCommand` (nothing on Ajustes, back to Ajustes from its sub-pages, otherwise push `ajustes`) and `GoToSectionCommand` (nothing when already on that section's root, otherwise `//route`); verify by build
- [x] 2.4 In `ModernoLayout.xaml` define `HeaderTemplate` (title in upper case centred on the full width, gear `ImageButton` "Ajustes" on the right) and `BottomBarTemplate` (Inicio, Reproductor, Favoritos as icon `ImageButton` plus label, tinted with `TabBarSelectedColor` or `TabBarUnselectedColor` from the page's `Section`, selected one described as "…, seleccionado"), and `SettingsPageTemplate` (header, `ContentPresenter`, bar); all icons tinted with `IconTintColorBehavior` and sized with `FontSizeTitle`
- [x] 2.5 `AppShell`: remove the `Ajustes` tab, register the `ajustes` route, set `Shell.TabBarIsVisible="False"`; remove `ShellChrome.ApplyTabFonts` and its Android/Windows parts and its call in `AppShell`, keeping `EnsureTitleView` and `ApplyRadioColors`
- [x] 2.6 `AppearanceService` swaps the layout dictionary (only when its type changes) after the palette; apply `SettingsPageTemplate` to `Personalización` and `Preferencias` with `Shell.NavBarIsVisible="False"` and remove their in-page title labels; temporarily open `ajustes` from the existing Inicio page to reach them
- [x] 2.7 Run the three builds. On Windows and Android, verify:
  - Personalización and Preferencias show the header `PERSONALIZACIÓN`/`PREFERENCIAS` centred, with the gear;
  - the gear returns to Ajustes without stacking a second one;
  - the Android back button returns to Ajustes;
  - the bar has no marked button and its buttons leave settings;
  - no native tab bar or empty pane is shown (Windows);
  - the bar sits above the Android gesture bar;
  - Iniciar sesión still shows the native header with its styled title;
  - switching Claro/Oscuro updates the header, bar and icons at once.

## 3. Reproductor as a template (today's content)

- [x] 3.1 Move today's `Reproductor` UI unchanged into `ReproductorTemplate` (root `BindingContext` from the templated parent, header, bottom bar marking Reproductor, and the 1×1 `ContentPresenter` for the player outside the artwork); `ReproductorPage` keeps only the `MediaElement` `Player` as content, sets `AppNavigation.Section="Reproductor"` and `Shell.NavBarIsVisible="False"`; its code-behind is unchanged; verify with a search that the app has exactly one `MediaElement`
- [x] 3.2 Run the three builds. On Windows, verify:
  - select a station, play, stop, play again;
  - an unreachable stream shows the error and `Reintentar` works;
  - the heart works;
  - leaving Reproductor stops audio exactly as before;
  - after switching Claro/Oscuro in Personalización, Reproductor (already created) shows the new colours and plays normally, with one stream only.

  If the existing page does not refresh after a layout swap, add the re-apply fallback from design decision 2 and verify again.

## 4. Inicio, Favoritos and Ajustes as templates (behaviour-preserving)

- [x] 4.1 Move today's `Inicio` content into `InicioTemplate` (header, bar marking Inicio); the page keeps its view model, `Section="Inicio"`, hidden nav bar and unchanged code-behind
- [x] 4.2 Move today's `Favoritos` content into `FavoritosTemplate` (header, bar marking Favoritos), same rules
- [x] 4.3 Create `AjustesTemplate` with the two cards `PERSONALIZACIÓN` and `PREFERENCIAS` (`SettingsCardButton`, full width, stacked) bound to the existing `AjustesViewModel` commands; the page has `Section` `None`; remove the temporary entry from 2.6
- [x] 4.4 Run the three builds. On Windows, verify:
  - Inicio: loading, list, search with the button and with Enter, Limpiar, the filter bar with Desactivar/Activar, "no results with your preferences", API stopped with Reintentar, selecting a station opens Reproductor and plays, the highlight;
  - Favoritos: logged out with Iniciar sesión, logged in list, play, remove, empty text, error with Reintentar, Cerrar sesión;
  - the gear from each section opens Ajustes with no bar button marked.

## 5. Moderno cards, Inicio search row, Favoritos and Ajustes

- [x] 5.1 Extend `StationListItem` with `Details` (country · language · up to three tags, `Desconocido` for a missing country or language), `PlayCommand` and `HeartCommand`; `InicioViewModel` sets `SelectStationCommand`/`ToggleFavoriteCommand` on each item; verify by build
- [x] 5.2 `FavoritosViewModel` lists `StationListItem` (with `IsFavorite` true and `IsSelected` from the current station), sets `PlayCommand`/`RemoveCommand` on each item, and moves the highlight when the current station changes; verify by build
- [x] 5.3 Create `Behaviors/AdaptiveSpanBehavior.cs` (`Span = max(2, (int)(Width / CardMinWidth))`, changing `Span` on the existing `GridItemsLayout`); verify by build
- [x] 5.4 Define `StationCardTemplate` in `ModernoLayout.xaml`:
  - a portrait card;
  - square logo with fallback;
  - name with at most 2 lines and two-line placeholder;
  - details with at most 2 lines and two-line placeholder;
  - a row with the play `AccentIconButton` ("Reproducir {name}") and the heart `IconButton` (filled or outline icon, "Quitar de favoritas"/"Añadir a favoritas");
  - card tap → `PlayCommand`;
  - selected trigger.
- [x] 5.5 `InicioTemplate`: search row (`Entry` filling the width, magnifier `Buscar` and clear `Limpiar búsqueda` icon buttons), filter bar as today, then the card grid with `AdaptiveSpanBehavior` and today's states
- [x] 5.6 `FavoritosTemplate`: compact email + `Cerrar sesión` row directly under the header (logged in only), the logged-out text and button as today, and the same card grid with today's states
- [x] 5.7 Remove the `FavoriteGlyph` properties and the `FilledHeart`/`EmptyHeart` constants once nothing uses them; verify with a search
- [x] 5.8 Run the three builds. On Windows, verify:
  - cards are taller than wide;
  - the column count grows with the window width and is at least 2;
  - all cards in a row have the same height with one-line and two-line names;
  - tap and play select and open Reproductor;
  - the hearts add and remove without selecting;
  - the highlight follows the current station in both lists;
  - all the states of 4.4 still work.

  On Android, verify two columns and the 48 dp buttons.

## 6. Moderno Reproductor and previous/next favourite

- [x] 6.1 `ReproductorViewModel`: add `IsActive`, `IsFavorite`, `Details` and `Notice`, and remove `ShowPlayerControls` and `ToggleButtonText` once nothing uses them; add `PreviousFavoriteCommand`/`NextFavoriteCommand` with the rules of design decision 7 (logged out → login with no pending station, favourites order newest first, wrap at both ends, first/last when the current station is not a favourite, reload once when empty, "Aún no tienes emisoras favoritas." otherwise, nothing when the target is the current station); `Notice` clears when the station or the state changes; verify by build
- [x] 6.2 `ReproductorTemplate` Moderno layout:
  - a large `AspectFit` logo with fallback;
  - name and details on the left, the heart on the right;
  - previous / play-stop (play icon "Reproducir" or stop icon "Detener", disabled without a station) / next;
  - a status line (state text, `Notice`, error message and `Reintentar`);
  - with no station, "Selecciona una emisora en Inicio" in place of the logo and info;
  - the content scrolls while the header and bar stay fixed;
  - the `ContentPresenter` for the player is kept.
- [x] 6.3 Run the three builds. On Windows, logged in with three favourites, verify:
  - next/previous follow the Favoritos order and wrap at both ends;
  - from a non-favourite, next plays the first and previous the last;
  - with one favourite that is current, nothing changes;
  - with no favourites, the notice shows;
  - logged out, previous/next open Iniciar sesión and going back adds nothing;
  - the heart is filled only for favourites and updates live;
  - the centre icon switches play/stop with the state;
  - the error and Reintentar work;
  - with no station, play/stop is disabled.

## 7. Personalización and live switching

- [x] 7.1 `PersonalizacionViewModel`: `SelectedPaletteId`, `Palettes` of the selected theme and `ShowPaletteChoice` (more than one palette), kept in step with `AppearanceService` (also after `Restablecer`); `PersonalizacionPage`: a "Variante" radio group shown only when `ShowPaletteChoice`; verify by build
- [x] 7.2 Update the README: theme and variant in the Personalización section, the `appearance_theme`/`appearance_theme_variant` keys with the migration, the new navigation (gear, bottom bar, Ajustes cards), the Moderno look, and a short "How to add a theme" section (palette dictionaries + layout dictionary with the required template keys + one `ThemeCatalog` entry, no other code; themes never set font families or sizes)
- [x] 7.3 Run the three builds. On Windows and Android, switch Claro ↔ Oscuro from Personalización and verify that every templated page, the header, the bar, the icons, the cards, the native inputs, Iniciar sesión and Crear cuenta follow at once without restart, that the session, favourites, selected station and preferences are unchanged, and that a station plays normally afterwards; verify `Restablecer valores por defecto` returns to Moderno Claro, Ubuntu, Normal and that the choice survives a restart; with a temporary second theme of one palette registered in `ThemeCatalog`, verify that it appears, the variant choice hides, and that removing it again leaves only Moderno

## 8. Accessibility, text sizes and final run

- [x] 8.1 Check every interactive control on the templated pages for a Spanish `SemanticProperties.Description` and the `MinTouchTarget` minimum; on Windows, verify with UI Automation that the names are read (gear, Buscar, Limpiar búsqueda, Reproducir {name}, hearts, Emisora favorita anterior/siguiente, Reproducir/Detener, bottom bar with "seleccionado")
- [x] 8.2 On Windows, use only the keyboard: Tab through the header, search row, cards' buttons and bottom bar; Enter/Space activates; the focused control shows an outline
- [x] 8.3 With `Pequeño` and `Enorme`, verify on Windows and on the Android emulator that Inicio, Favoritos, Reproductor, Ajustes, Personalización and Preferencias have no clipped text and no unreachable controls, and that the header and the bar stay fixed while the content scrolls
- [x] 8.4 Measure the contrast of every text/background pair of both palettes (including the bar and selected cards) and record the values in the README; every pair must be at least 4.5:1
- [ ] 8.5 Update the README manual check-list for the new navigation, cards, Reproductor controls, previous/next favourite, variants, migration and keyboard use; run the three builds and the full check-list on Windows and the Android emulator; verify with a search that no page, template or style outside the palettes and the typography defaults contains a literal colour, font family or font size

## Workflow follow-up

- Archive the change with `/opsx:archive` after the developer has reviewed it.
