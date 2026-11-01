# Design

## Context

See proposal.md for the motivation. The current state that shapes the approach:

**Theme engine (Phase 5)**
- `ThemeCatalog` lists `AppThemeDefinition(Id, DisplayName, LayoutDictionaryType, Palettes)`; each `ThemePalette(Id, DisplayName, ResourceDictionaryType, BaseTheme)`.
- `AppearanceService` merges one palette dictionary and one layout dictionary into the application resources. It swaps the layout only when its type changes, stores `appearance_theme` and `appearance_theme_variant`, and `SetTheme` keeps the stored variant when the new theme has it, otherwise it uses the theme's first palette.
- The palette contract is 14 colour keys. `Styles.xaml` holds the typography defaults and the shared styles.

**Keys `ModernoLayout` defines** (the contract for every layout):

| Key | Kind | Used by |
|---|---|---|
| `HeaderTemplate` | `ControlTemplate` | the page templates, through `<ContentView ControlTemplate=...>` |
| `BottomBarTemplate` | `ControlTemplate` | the page templates, through `<views:BottomBar ControlTemplate=...>` |
| `StationCardTemplate` | `DataTemplate` of `StationListItem` | the station lists in `InicioTemplate` and `FavoritosTemplate` |
| `InicioTemplate`, `FavoritosTemplate`, `ReproductorTemplate`, `AjustesTemplate` | `ControlTemplate` | the pages' `ControlTemplate="{DynamicResource ...}"` |
| `SettingsPageTemplate` | `ControlTemplate` | `PersonalizacionPage`, `PreferenciasPage` |

Each page template sets its root `BindingContext` from the templated parent. Navigation for the chrome is `AppNavigation.OpenAjustesCommand` / `GoToSectionCommand`. `BottomBar` exposes `IsInicio` / `IsReproductor` / `IsFavoritos`, and `IconTint.Color` tints the Phase 5 icons.

**Audio today**
- `MediaElementAudioPlayer` drives one `MediaElement` that is the content of `ReproductorPage`.
- The page attaches it in `OnAppearing` and detaches it, which stops playback, in `OnDisappearing`. `App.OnSleep` stops playback.
- `PlaybackService` is the single source of truth (`CurrentStation`, `State`, `IsActive`, `StatusText`, `SelectStationAsync`, `ToggleAsync`, `RetryAsync`).

**View models today**
- `InicioViewModel.SelectStationCommand` and `FavoritosViewModel.PlayCommand` select the station and navigate to `//reproductor`.
- `ReproductorViewModel` has no favourites list, no login state and no select-favourite command.

**Names in the brief that differ from the code**
- **"Select-and-play action":** `PlaybackService.SelectStationAsync`, reached through the view-model commands.
- **"Play/stop toggle":** `ReproductorViewModel.ToggleCommand`.
- **"Limpiar":** the clear button keeps its accessible name `Limpiar búsqueda`.
- **Favoritos logged out:** the brief says "the same redirect to Iniciar sesión as today". Today Favoritos shows `Inicia sesión para ver tus favoritas.` with an `Iniciar sesión` button, and Tocadiscos keeps exactly that.
- **"Station item template":** `StationCardTemplate`.
- **Asset names:** the brief suggests names such as `vinyl_disc.svg`. The art is drawn with MAUI shapes instead (decision 8).

## Goals / Non-Goals

**Goals:**
- Tocadiscos is one palette, one layout, one registry entry and reusable UI-only controls. No service, API or database change.
- One audio player for the whole app that no page or template owns, so audio survives page changes and theme changes.
- Every custom control is a pure view of bindable state. Motion observes `PlaybackService.State` (through the view models) and never calls a service.

**Non-Goals:**
- No changes to `ModernoLayout.xaml` or the Moderno palettes.
- No changes to `PlaybackService`, `FavoritesService`, `SessionService` or `PreferencesService`.
- No new packages, no raster art, no sound effects.
- The Windows icon-size limitation from Phase 5 is not fixed here.

## Decisions

### 1. Palette `standard`, within the 14 keys

Text sits on the wood (page background, the bar, the settings panel) or on the brass buttons. Cream is used only behind dark `AccentTextColor` text (Ajustes cards) or behind logos (sleeves).

The shared `Personalización` preview draws `PrimaryTextColor` on `SurfaceColor`. So `SurfaceColor` is a lighter wood, and the cream "paper" comes from `TabBarUnselectedColor`.

| Key | Value | Role in Tocadiscos | Contrast |
|---|---|---|---|
| `PageBackgroundColor` | `#3A2416` | walnut page | — |
| `SurfaceColor` | `#4E3322` | lighter wood: information panel, notes, preview card | `PrimaryText` 9.76, `SecondaryText` 6.91, `ErrorText` 6.01 |
| `InputBackgroundColor` | `#15100C` | vinyl black: records, empty slots, inputs | `PrimaryText` 15.99 |
| `DividerColor` | `#6E4E33` | grooves, separators, wood grain | — |
| `SelectedItemBackgroundColor` | `#E3AE4F` | outline of the current sleeve / crate record | 7.23 on wood |
| `PrimaryTextColor` | `#F6EBD6` | warm light text | 12.31 on page, 14.72 on bar |
| `SecondaryTextColor` | `#D9C6A5` | characteristics, captions | 8.71 on page |
| `ErrorTextColor` | `#FFA48C` | errors | 7.58 on page |
| `AccentColor` | `#D9A441` | brass: buttons, spindle, tonearm, card border | `AccentText` on it 7.82; 5.13 on surface |
| `AccentTextColor` | `#24160B` | text on brass and on cream cards | 14.34 on cream sleeves |
| `FavoriteIconColor` | `#F0BC62` | hearts | 8.36 on page |
| `TabBarBackgroundColor` | `#25170D` | dark wood: bar, settings panel, crate | — |
| `TabBarSelectedColor` | `#F2C46D` | marked bar icon and label, header title | 10.68 on bar |
| `TabBarUnselectedColor` | `#EADBBE` | cream: sleeves, Ajustes cards, unmarked bar icons | 12.74 on bar |

- `BaseTheme` is `Dark`. Every pair is re-measured in task group 6, and the table goes into the README.
- **Alternative rejected:** adding text-on-surface keys to the contract. It would touch the Moderno palettes and the shared styles.

### 2. `TocadiscosLayout` defines every key itself

`TocadiscosLayout.xaml` defines the eight keys above, plus Tocadiscos-only keys (for example `ReproductorHeaderTemplate` with the house button, and the sleeve and record templates used by the controls). It never merges or references `ModernoLayout`.

During task group 2 it starts as a copy of Moderno's templates, so the theme is always complete. Every copied template is replaced by group 6, and a search confirms no Moderno template is left.

**Templates per page:**
- **`InicioTemplate`**, rows: header / search row / filter bar / main area / bottom bar. The main area is a `Grid` with columns `3*,2*`: the `SleeveStack` on the left; on the right the information panel, a `ScrollView` vertically centred, bound to the stack's `FocusedItem`.
- **`FavoritosTemplate`:** the same, with the compact account row instead of the search row. The logged-out, loading, error and empty states sit in the main area, centred.
- **`ReproductorTemplate`**, rows: `ReproductorHeaderTemplate` / turntable area (`3*,2*`: record + tonearm, information panel + status line) / `RecordCrate`, about 28% of the height. No `BottomBar`.
- **`AjustesTemplate`:** header, the two cards, bottom bar. Each card is a `Grid`: a vinyl `Ellipse` peeking out on the right behind a `Button` with a cream background (`TabBarUnselectedColor`), `AccentTextColor` text and an `AccentColor` border. The button is the only interactive element.
- **`SettingsPageTemplate`:** wood background, header, a rounded `Border` (`TabBarBackgroundColor`) around the `ContentPresenter`, then the bottom bar.

**Header and bar:**
- `HeaderTemplate` and `BottomBarTemplate` reuse `AppNavigation`, `BottomBar` and `IconTint` exactly like Moderno, styled as a wooden strip with brass icons.
- `ReproductorHeaderTemplate` adds an `ImageButton` (`ic_home`, `Inicio`) bound to `GoToSectionCommand` `inicio`.

### 3. One app-level audio host

`MediaElementAudioPlayer` creates and owns its single `MediaElement`, and connects the element's handler to the window's `MauiContext` once, when `App.CreateWindow` builds the window.

- No page attaches or detaches it any more. `ReproductorPage` loses its `MediaElement` content and its attach/detach code.
- `App.OnSleep` still stops playback, so there is still no background playback.
- `IAudioPlayer` and `PlaybackService` keep their contract.
- `ModernoLayout`'s `ReproductorTemplate` keeps its 1×1 `ContentPresenter`, which now presents nothing, so Moderno's file and look stay unchanged.

This is the first thing built (task group 2), and it is verified on both platforms before any template work:
- audio plays from `Inicio` with `Reproductor` never opened;
- audio continues across pages and theme changes;
- only one stream is heard;
- background still stops it.

**Alternatives rejected:**
- Keeping the element in `ReproductorPage` without detaching. The page may never have been created, and its platform view is not kept alive while another section is shown.
- Hosting it inside each layout. A theme change would recreate it.

**Fallback if a platform does not play an element without a visual parent:** place the 1×1 element in a persistent overlay view of the window. If neither works, stop and report.

### 4. Additive view-model members

| View model | New members | Behaviour |
|---|---|---|
| `InicioViewModel` | `PlayHereCommand(StationListItem)`, `PlaybackStatusText` | The current station: `PlaybackService.ToggleAsync`. Any other: `SelectStationAsync`. No navigation. |
| `FavoritosViewModel` | `PlayHereCommand(StationListItem)`, `PlaybackStatusText` | Same. |
| `ReproductorViewModel` | `Favorites` (`ObservableCollection<StationListItem>`), `IsLoggedIn`, `HasNoFavorites`, `SelectFavoriteCommand(StationListItem)`, `LogInCommand` | The list is `FavoritesService.Favorites` filtered like Favoritos, with `IsSelected` from the current station. It is rebuilt on `FavoritesChanged`, session changes and `CurrentStation` changes. `LogInCommand` clears `PendingStation` and opens `login`. |

The existing commands stay, including `SelectStationCommand`, `PlayCommand`, `ToggleCommand` and previous/next favourite, and Moderno keeps using them. The crate uses the favourites already in memory, the same list the hearts, `Favoritos` and previous/next use.

### 5. Custom controls (`Views/Controls/`)

All of them are `ContentView`s with bindable properties, drawn with MAUI shapes and palette colours through `DynamicResource`. None of them knows a service or a view-model type.

**`VinylDisc`**
- Properties: `LogoSource`, `IsSpinning`.
- A rotating `Grid` holds the concentric groove ellipses (`DividerColor` strokes on `InputBackgroundColor`) and the label: the logo clipped by an `EllipseGeometry`, over the fallback image.
- The shine (a `LinearGradientBrush` from `PrimaryTextColor` to transparent, at low opacity) and the spindle (`AccentColor`) sit on a separate layer that does not rotate. Only the one element's `Rotation` changes; nothing is redrawn.
- Size: `min(width, height)` of its cell, so the record is never clipped and never depends on the text size.

**`Tonearm`**
- Properties: `IsLowered`, `Command`, `IsEnabled`.
- An arm group (`Path`/`Line`/`Ellipse` in `AccentColor`/`DividerColor`) with `AnchorX`/`AnchorY` at the pivot, outside the record.
- A transparent `Button` at least 56 units wide follows the arm's length and rotation. It is the focusable, accessible and clickable element, so the touch area is wide while the drawing stays thin. Its `SemanticProperties.Description` follows the state.

**`SleeveStack`**
- Properties: `ItemsSource`, `ItemTemplate`, `FocusedItem` (read-only, bindable), `FocusedIndex`, `ActivateCommand`, `FocusOnReset` (`First` for Inicio, `KeepPosition` for Favoritos).
- It shows only three item views (focused, next, next+1) with `Scale` 1 / 0.8 / 0.65, increasing `TranslationY`, `ZIndex` and `Opacity` 1 / 0.75 / 0.55. Any number of stations therefore costs three views, which covers virtualisation.
- **Input:**
  - `SwipeGestureRecognizer` Up/Down.
  - On Windows, a small platform hook on its view: `PointerWheelChanged` and `KeyDown` (Up/Down move, Enter/Space activate), plus `IsTabStop`.
- **Activation:**
  - Pressing the focused view runs `ActivateCommand` with the item.
  - Pressing a smaller view only moves the focus.
  - A focus change has at most a 120 ms fade.
- **Collection changes:**
  - A reset with the previously focused item still present keeps it focused.
  - Otherwise `First` focuses index 0 and `KeepPosition` keeps the same index, clamped. This gives "the next one, or the previous one if it was the last" when Favoritos rebuilds its list after a removal.

**`RecordCrate`**
- Properties: `ItemsSource`, `SelectCommand`, `LogInCommand`, `IsLoggedIn`, `EmptyText`.
- It has 6 slot views: a `Grid` with the record or sleeve drawing, the logo, a one-line caption (`FontSizeCaption`, `TailTruncation`) and a transparent `Button` for focus and activation. Slots 1–3 sit left of a partition and 4–6 right of it, tilted a few degrees.
- **Current record:** raised (`TranslationY` = -20% of the slot height) and outlined.
- **Pages:**
  - The page index is internal to the control.
  - On `Loaded` and on page appearing it moves to the page that contains the current station.
  - When the item count shrinks it clamps to the last page.
  - Arrow `ImageButton`s and a `1/2` label are visible only with more than 6 items; a `SwipeGestureRecognizer` Left/Right also changes page.
- **Keyboard (Windows):** a platform `KeyDown` hook moves the focus Left/Right between the slot buttons.
- **Logged out:** one transparent `Button` covers the crate (`Inicia sesión para ver tus favoritas`) and runs `LogInCommand`.

**`MotionSettings`** (static)
- `IsReducedMotion`: on Windows `UISettings.AnimationsEnabled == false`; on Android `Settings.Global.AnimatorDurationScale == 0`.
- Read each time a control's page appears.

### 6. Motion observes state, stops when hidden

**Tonearm**
- On an `IsLowered` change, `RotateTo` the lowered angle in 600 ms (`CubicOut`), then a settle of about ±2° in two 100 ms steps; or the raised angle in 500 ms.
- With reduced motion, or when the control is not yet shown, it sets `Rotation` directly.

**Record spin**
- An `IDispatcherTimer` of about 16 ms integrates the angular speed `ω` toward its target: 198°/s (33⅓ rpm) while spinning, otherwise 0.
- It speeds up over 1.2 s and slows down over 1.8 s, and adds `ω·dt` to the current `Rotation` modulo 360, so the record never jumps.
- The timer stops when `ω` reaches 0. With reduced motion `ω` is always 0.

**Visibility**
- Each animated control finds its ancestor `Page` on `Loaded`, subscribes to `Appearing`/`Disappearing`, and unsubscribes and stops everything on `Unloaded` (which also covers a theme swap).
- On `Disappearing` it stops the timer and cancels the animations.
- On `Appearing` it snaps to the current state: tonearm at its final angle, `ω` at its target. A page opened while playing therefore shows the record already spinning and the tonearm already down.

**Inicio and Favoritos** have no timed animation besides the stack fade.

### 7. Live theme swap in both directions

`ControlTemplate="{DynamicResource X}"` re-evaluates when the layout dictionary is replaced.
- Phase 5 verified this for palettes only (the layout type never changed). Task group 2 verifies the type swap on every existing page, including `Personalización` itself and an existing `Reproductor`, in both directions.
- If an existing page does not refresh, `AppearanceService` re-applies the template on the existing themed pages after a layout swap. It reads the resource again and calls the page handler's content update, which Phase 5 found necessary on Windows.
- The current page (`Personalización`) is handled the same way but verified separately, because Phase 5's rebuild broke it.

Because audio no longer lives in any page, a swap can never touch it.

### 8. Art and licence

- All Tocadiscos art is drawn in XAML with shapes and gradients bound to palette keys: the record, grooves, shine, label, spindle, tonearm, sleeve, peeking disc, crate front, partition, wood strips and slots. No colour is ever written into the art itself.
- SVG is not used, because multi-colour SVG art would hard-code colours.
- `Resources/Images/Tocadiscos/LICENSE.txt` states that the art (in `Views/Controls/` and `TocadiscosLayout.xaml`) is original work for this project and uses no third-party glyphs. The `.csproj` ships it under `Licences/` like the other licences.
- The Phase 5 icons are reused.
- If the shapes cannot give an acceptable result for a piece, implementation stops and asks, instead of shipping placeholders.

### 9. Personalización and persistence

- **Today's behaviour would lose the Moderno variant.** `SetTheme("tocadiscos")` would store `appearance_theme = tocadiscos` and `appearance_theme_variant = standard`, because Tocadiscos has no `light`/`dark`. Going back to Moderno would then fall back to its first palette, `Claro`.
- **One small `AppearanceService` change keeps it.** When the theme has a single palette, `SetTheme` stores only `appearance_theme` and leaves `appearance_theme_variant` untouched. When it switches to a theme, it uses the stored variant if that theme has it, otherwise the theme's first palette. `Initialize` already does the same: with `tocadiscos` and a stored `dark`, it uses `standard`. This is appearance code only; services with business logic are untouched.
- `ResetToDefaults` still stores `moderno` + `light`.
- The theme list and the variant visibility already work generically (Phase 5).

## Risks / Trade-offs

- **[Off-tree `MediaElement` does not play on a platform]** → Verified first, in task group 2. Fallback: an overlay host in the window; otherwise stop and report.
- **[Audio now continues when leaving `Reproductor` in Moderno too]** → An accepted behaviour change. The README and the `audio-playback` spec say so.
- **[Layout type swap does not refresh an existing page]** → Verified in task group 2, with the re-apply fallback (decision 7).
- **[Windows mouse wheel and keys on a custom control]** → A small `#if WINDOWS` hook in `SleeveStack` and `RecordCrate`. If one input method cannot work, it is reported in the group 4 summary.
- **[Timer cost]** → One 16 ms timer, only while `Reproductor` is visible and the record is moving. No shadows; static gradients only.
- **[Crate shows stale favourites when loading failed at login]** → The same list as the hearts. Opening `Favoritos` reloads it; accepted, as in Phase 5.
- **[Windows icons keep their size after a live text-size change]** → The Phase 5 limitation applies to the new icons too. It is already documented.

## Migration Plan

- No data migration. Stored theme values stay valid. `tocadiscos` is a new value; older builds read it as unknown and fall back to Moderno Claro.
- Rollback: remove the catalogue entry. A stored `tocadiscos` then falls back to Moderno Claro.
