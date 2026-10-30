# Tasks

## 1. Palettes, registry and persistence

- [ ] 1.1 Create `Resources/Themes/ModernoLightPalette.xaml`(+.cs) and `ModernoDarkPalette.xaml`(+.cs) with the same 14 keys and values as `WhiteTheme`/`BlackTheme`; delete `WhiteTheme.*` and `BlackTheme.*`; verify with a search that no file references `WhiteTheme`, `BlackTheme`, `"white"` or `"black"` outside the migration code
- [ ] 1.2 Create an initial `Resources/Themes/ModernoLayout.xaml`(+.cs) `x:Class` dictionary (empty for now) so the registry can reference it; verify it compiles
- [ ] 1.3 Add `ThemePalette` and extend `AppThemeDefinition` with `LayoutDictionaryType` and `Palettes` in `ThemeCatalog.cs`; register only `moderno` "Moderno" with `light` "Claro" and `dark` "Oscuro"; set `DefaultId = "moderno"` and `DefaultPaletteId = "light"`; add a lookup of a palette within a theme; verify by build
- [ ] 1.4 In `AppearanceService`: add the `PaletteId` property, the `appearance_theme_variant` key, `SetPalette(paletteId)`, `SetTheme(themeId)` keeping the palette when possible, and migration `white`→`moderno`+`light` / `black`→`moderno`+`dark` with fallback to Moderno Claro; `ResetToDefaults` → Moderno Claro; still swap only the palette here (layout swap is group 2); verify by build
- [ ] 1.5 Make `PersonalizacionViewModel`/`Page` compile against the new registry (theme list shows `Moderno`; variant choice comes in group 5)
- [ ] 1.6 Run the three builds (`dotnet build src/OpenWave.Api`, App Windows, App Android) with 0 errors. On Windows, verify that a device with stored `black` starts dark and stored `white` starts light, and that both are rewritten as `moderno` + variant

## 2. Layout engine and Reproductor template

- [ ] 2.1 `AppearanceService` keeps `_layoutDictionary` and swaps it (palette first, then layout) only when the theme changes; verify by build
- [ ] 2.2 Move today's `Reproductor` UI unchanged into `ReproductorTemplate` (root `x:DataType="vm:ReproductorViewModel"`, colours and styles via `DynamicResource`, a 1×1 `ContentPresenter` for the audio host); `ReproductorPage` sets `ControlTemplate="{DynamicResource ReproductorTemplate}"` and keeps only the `MediaElement` `Player` as its `Content`; code-behind unchanged; verify with a search that there is exactly one `MediaElement` in the app
- [ ] 2.3 On Windows: play a station, stop, play again, error and `Reintentar`, heart; then change the variant in `Personalización`, return to `Reproductor` and verify play/stop still works, there is no crash, and no audio starts by itself
- [ ] 2.4 Verify the live template refresh on an existing `Reproductor` page after a theme swap (temporarily swap the layout dictionary from `Personalización` by switching the variant twice). If it does not refresh, add the re-assignment helper from design Decision 5 and verify again; run the three builds

## 3. Inicio and Favoritos as templates (today's list look)

- [ ] 3.1 Move today's `Inicio` UI unchanged into `InicioTemplate` and the item template into `InicioStationTemplate`; `InicioPage` uses `ControlTemplate="{DynamicResource InicioTemplate}"`; verify by build
- [ ] 3.2 Move today's `Favoritos` UI unchanged into `FavoritosTemplate` and `FavoritoStationTemplate`; `FavoritosPage` uses `ControlTemplate="{DynamicResource FavoritosTemplate}"`; verify by build
- [ ] 3.3 On Windows, verify every function and state is unchanged in `Inicio`:
  - search with Enter and with `Buscar`, then `Limpiar`;
  - the filter bar with `Desactivar` and `Activar`, and the empty result with preferences plus its `Desactivar`;
  - loading;
  - the API stopped → error → `Reintentar`;
  - the selection highlight, and the heart logged out (opens `Iniciar sesión`) and logged in.
- [ ] 3.4 On Windows, verify `Favoritos` is unchanged:
  - logged out: the prompt and `Iniciar sesión`;
  - logged in: the email, `Cerrar sesión`, loading, empty, error and `Reintentar`;
  - play and remove.

  Run the three builds.

## 4. Moderno card grid and player

- [ ] 4.1 Add `Behaviors/AdaptiveSpanBehavior.cs` (`MinColumnWidth`, at least 2 columns, a new `GridItemsLayout` only when the span changes); verify on Windows that resizing the window from narrow to about 1000 units changes 2 → 5 columns
- [ ] 4.2 Add the card tokens to `Styles.xaml` / `ModernoLayout.xaml`: `MinTouchTarget` (48), grid spacing, `CardIconButton` style, column width 180, player artwork size 240. Raise the implicit `Button` and `FavoriteToggleButton` minimums to `MinTouchTarget`. Verify there are no literal colours or font sizes with a search of the XAML
- [ ] 4.3 Write the Moderno `InicioStationTemplate` and `InicioTemplate` grid:
  - the card has a square artwork with the placeholder, the name (2 lines, ellipsis), country · language, a play button (`SelectStationCommand`) and the heart (`ToggleFavoriteCommand`);
  - tapping the card selects, as today, and the selection highlight is kept.

  Verify on Windows that the actions and the highlight behave as in 3.3.
- [ ] 4.4 Write the Moderno `FavoritoStationTemplate` and `FavoritosTemplate` grid: the card with the play button (`PlayCommand`) and the filled heart (`RemoveCommand`, no confirmation). Verify play and remove on Windows, including a removal while the API is stopped, which shows `No se pudo actualizar tus favoritas.`
- [ ] 4.5 Write the Moderno `ReproductorTemplate`: a scrollable column with the large artwork, the name with the heart, the country, the state, a wide play/stop button, `Reintentar`, and the `ContentPresenter` for the audio host. Verify playback, stop, error/retry and the heart on Windows, and that no new controls appear
- [ ] 4.6 Run the three builds; on the Android emulator, verify Inicio shows 2 columns, the card tap and play open `Reproductor`, the heart works, and `Favoritos` play/remove work

## 5. Personalización: theme, variant, preview and reset

- [ ] 5.1 `PersonalizacionViewModel`: add `SelectedPaletteId`, `Palettes` and `ShowPaletteChoice`, kept in step with `AppearanceService` (including after the reset); verify by build
- [ ] 5.2 `PersonalizacionPage`: add the variant radio group (Spanish label, shown only when `ShowPaletteChoice`), and keep `Tipo de letra`, `Tamaño del texto`, `Vista previa` and `Restablecer valores por defecto`. Verify on Windows that `Moderno` + `Claro`/`Oscuro` show the current values
- [ ] 5.3 Verify that the variant choice is generic: temporarily register a test theme with one palette, check that no variant group appears, then remove the test entry before finishing the group
- [ ] 5.4 Verify the live switch on Windows and Android:
  - `Claro` → `Oscuro` → `Claro` updates `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Iniciar sesión`, `Crear cuenta`, the tab bar, the header, the inputs and the Android radio circles without a restart;
  - the session, favourites and preferences are unchanged;
  - `Restablecer` returns to Moderno Claro, `Ubuntu`, `Normal`;
  - after a restart the choice is kept, with no flash of the wrong theme.

  Run the three builds.

## 6. Accessibility, text sizes, README and full run

- [ ] 6.1 Add Spanish `SemanticProperties.Description` to every button and card in the three templates (`Reproducir {nombre}`, `Añadir a favoritas`/`Quitar de favoritas` by state, `Detener`/`Reproducir`, `Buscar`, `Limpiar`, `Reintentar`…). Verify with Windows UI Automation that the names are exposed
- [ ] 6.2 On Windows, verify with Tab, Enter and Space that each card's play button and heart, and the buttons of `Inicio`, `Reproductor` and `Favoritos`, are reachable, show a visible focus and activate
- [ ] 6.3 Verify the touch targets: on Android, the heart and play button measure at least 48×48 (UI dump bounds); on Windows, they are at least 32 px
- [ ] 6.4 Re-compute the contrast of every text/background pair used by the Moderno layouts in both variants (at least 4.5:1) and record the lowest values in the README
- [ ] 6.5 At `Pequeño` and `Enorme`, on Windows and Android, verify that `Inicio`, `Favoritos` and `Reproductor` clip no text vertically, that long names end with an ellipsis, and that every control is reachable
- [ ] 6.6 Update `README.md`:
  - the Themes and fonts section: Moderno with Claro/Oscuro, the migration from Blanco/Negro;
  - a "How to add a theme" section: palette dictionaries, one layout dictionary with the five required keys, and one `ThemeCatalog` entry, with no other code; themes never set fonts or sizes;
  - an updated manual check list.

  Verify that the README steps match the code.
- [ ] 6.7 Full run on Windows and on the Android emulator of the manual check list (all earlier functions plus this phase); final three builds with 0 errors; leave both devices in Moderno Claro, `Ubuntu`, `Normal`, logged out

## Workflow follow-up

- Sync the delta spec into `openspec/specs/app-appearance` and archive the change after review.
