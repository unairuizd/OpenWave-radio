# Design

## Context

See proposal.md for the motivation. The briefs in `openspec/theme-briefs/` and the mock-up canvas are the visual reference; the specs of this change are the contract. Where a brief and a spec disagree, the spec wins.

**What exists after Phase 6** (observed in the code):
- **Engine.** `ThemeCatalog` lists `AppThemeDefinition(Id, DisplayName, LayoutDictionaryType, Palettes)`; each `ThemePalette(Id, DisplayName, ResourceDictionaryType, BaseTheme)`.
  - `AppearanceService` swaps the palette dictionary on every change and the layout dictionary only when its type changes.
  - `SetTheme` uses the stored variant if the new theme has it, else the theme's first palette.
  - `StoreTheme` writes the variant only for themes with more than one palette.
- **Layout contract.** Each layout defines eight keys: `HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `FavoritosTemplate`, `ReproductorTemplate`, `AjustesTemplate` and `SettingsPageTemplate`. A layout may add theme-only keys (Tocadiscos adds `ReproductorHeaderTemplate` and `CrateRecordTemplate`).
- **Palette contract.** 14 colour keys. `Styles.xaml` uses `SurfaceColor` as a background and `TabBarUnselectedColor` as a text colour, so both must work in those roles in every palette.
- **Controls** in `Views/Controls/`:
  - `PageAwareView`: knows whether its page is shown.
  - `MotionSettings`: reduced motion.
  - `VinylDisc`: `LogoSource`, `IsBlank`, `IsSpinning`; its own dispatcher timer integrating angular speed.
  - `Tonearm`.
  - `SleeveStack`: focus, `FocusOnReset`, `ActivateCommand`, and a `#if WINDOWS` hook for `PointerWheelChanged`/`KeyDown`.
  - `RecordCrate`: 6 slots, internal paging, `SelectCommand`, `LogInCommand`, `IsLoggedIn`, `LoggedOutText`, `EmptyText`.
  - `ItemBinding`.
- **View models** already expose everything a play-in-place theme needs:
  - Inicio and Favoritos: `PlayHereCommand`, `PlaybackStatusText`.
  - Reproductor: `Favorites`, `IsLoggedIn`, `HasNoFavorites`, `SelectFavoriteCommand`, `LogInCommand`, `PreviousFavoriteCommand`, `NextFavoriteCommand`, `ToggleCommand`, `RetryCommand`, `ToggleFavoriteCommand`.
  - Favoritos: `RemoveCommand`, `LogOutCommand`.
- **Licences.** The `.csproj` ships `Resources\Images\Tocadiscos\LICENSE.txt` as `Licences\Tocadiscos-LICENSE.txt`.
- **Phase 6 left three manual checks unticked** (`MotionSettings` 3.2, reduced motion 6.3, full run 7.1). Every animated control here depends on `MotionSettings`.
- **Leftover diagnostics.** The working tree still has `// TEMP-DIAG` calls to `DiagLog` (in `AppShell`, `AppearanceService`, `PersonalizacionViewModel`, `BottomBar` and `Platforms/Windows/App.xaml.cs`) left from a Phase 6 investigation.

## Goals / Non-Goals

**Goals:**
- Six themes, each made only of palettes, one layout, controls in `Views/Controls/` and one catalogue entry, following the Phase 6 pattern.
- As few new controls as possible: one paged-slot control for every favourites holder, and one cursor list for every list with a cursor.
- Variant memory by light/dark with no new stored key.
- Moderno and Tocadiscos look and behave exactly as before.

**Non-Goals:**
- No theme-specific fonts, no raster art, no sound, no real audio levels.
- No change to services other than `AppearanceService`, to view models, the API or the database. One exception, decided during group 6: `PlaybackService` also writes its `IsActive` to a small static, bindable `PlaybackIndicator`, so the Mesa de DJ `EN DIRECTO` tag can light on every page, including the settings pages whose view models do not know the play state. In group 7 it also carries `StatusText`, so the MP3 line shared by Inicio and Favoritos can show the current station's play state.
- No change to the eight-key layout contract or the 14-key palette contract.
- No navigation change: Clásico de rueda keeps a bottom bar, and `MENÚ` only opens `Inicio`.

## Decisions

### 1. Variant memory through `BaseTheme` and unique palette ids

Every palette id is unique across the catalogue:
- Moderno: `light`, `dark`.
- Tocadiscos: `standard`.
- Radio de válvulas: `baquelita`, `nogal`.
- Casete portátil: `metal`.
- Boombox: `cromado`.
- Mesa de DJ: `club`.
- MP3: `azul`.
- Clásico de rueda: `blanco`, `negro`.

The light/dark kind of a stored id is therefore known from the catalogue (`ThemePalette.BaseTheme`).

`SetTheme` and `ReadStoredTheme` choose the palette in this order:
1. The stored id, if the theme has it.
2. Otherwise the theme's palette with the same `BaseTheme` as the stored id's palette.
3. Otherwise the theme's first palette.

`StoreTheme` is unchanged: only multi-palette themes write the variant, so a single-palette theme still leaves it alone.

- **Example:** `dark` → Radio de válvulas gives `nogal` (stored) → Clásico de rueda gives `negro` → Moderno gives `dark`.
- **Rollback:** a stored `nogal` read by Moderno maps to `dark`.
- **Alternative rejected:** a new `appearance_theme_mode` key. It is a second source of truth that could disagree with the stored variant.
- **Alternative rejected:** a new `IsDark` field on `ThemePalette`. `BaseTheme` already says exactly this.

### 2. Each layout starts complete and is replaced piece by piece

As in Phase 6:
- Each layout file starts as a copy of the Moderno templates, so a newly registered theme is always usable.
- The copies are replaced inside that theme's task group.
- The group ends with a search proving the file does not reference `ModernoLayout` and defines all eight keys.

Theme-only keys are allowed (for example `ReproductorHeaderTemplate` for the house button, or `PresetKeyTemplate`).

### 3. `PagedSlots`: one control for every favourites holder

A new `PagedSlots` control (a `PageAwareView`, because it reads the current item on `Appearing`) shows `Columns × Rows` item views from `ItemsSource` and pages through the rest.

| Bindable property | Purpose |
|---|---|
| `ItemsSource`, `ItemTemplate` | The items and how each one is drawn |
| `EmptySlotTemplate` | How an empty slot is drawn |
| `Columns`, `Rows` | The grid of slots |
| `ColumnMajor` | Fill the slots column by column |
| `SelectCommand`, `LogInCommand`, `IsLoggedIn` | Play a favourite, or open login when logged out |
| `LoggedOutText`, `EmptyText` | The texts for the logged-out and empty states |
| `ItemIsCurrentPath` | Which item is the current station |

Read-only properties for the layout:

| Read-only property | Purpose |
|---|---|
| `PageIndex`, `PageCount`, `PageText` (`1/2`) | The page indicator |
| `HasPages` | Whether to show the paging controls |
| `NextPageCommand`, `PreviousPageCommand` | Change the page |

- **Paging rules:** the same as `RecordCrate`. It opens on the page with the current item, clamps after removals and moves page on a Left/Right swipe.
- **The layout draws its own arrows and indicator** from the read-only properties, because they differ per theme: arrow keys at the ends (Válvulas), `Dar la vuelta` (Casete), `‹ 1/2 ›` (Boombox, Mesa de DJ).

| Theme | Use | Columns × Rows |
|---|---|---|
| Radio de válvulas | Preset keys | 5×1 |
| Boombox | Memories | 3×2 |
| Mesa de DJ | Pads | 4×2 |
| Casete portátil | J-card in Favoritos (`ColumnMajor`, `CARA A`/`CARA B`) | 2×5 |
| Casete portátil | Side A in Reproductor | 1×5 |

- In Favoritos, Casete binds the J-card to `FavoritosViewModel.Stations` with `SelectCommand` = `PlayHereCommand`. A remove button in each line's template uses `RemoveCommand`.
- **Alternative rejected:** generalising `RecordCrate`. It would touch the finished Tocadiscos theme.
- **Alternative rejected:** a control per theme. That would mean four copies of the paging logic.

### 4. `CursorList`: one control for every list with a cursor

A new `CursorList` wraps a vertical `CollectionView` (virtualised by the platform) and keeps a cursor.

| Bindable property | Purpose |
|---|---|
| `ItemsSource`, `ItemTemplate` | The items and how each one is drawn |
| `CursorItem` | The item under the cursor (read-only, bindable) |
| `ActivateCommand` | What the centre, `OK` or a second touch does |
| `FocusOnReset` | `First` or `KeepPosition`, with the same rules as `SleeveStack` |
| `MoveCommand(int)` | Move the cursor by that many rows |
| `PageMoveCommand(int)` | Move the cursor by one screen of rows |

- **Cursor drawing.** The cursor is drawn by the item template through the `CollectionView` selection state (`VisualStateManager` "Selected"). Selection is driven only by the control, so tapping a row moves the cursor without playing.
- **Second touch.** A `TapGestureRecognizer` in the item template calls the control's `TapCommand(item)`: the cursor item activates, any other item gets the cursor.
- **Scrolling.** `ScrollTo` keeps the cursor visible: 120 ms animated, or instant with reduced motion.
- **Windows.** A `#if WINDOWS` hook like `SleeveStack`'s: Up/Down move, Enter activates, the wheel moves.

| Theme | Use | Activate |
|---|---|---|
| Casete portátil | The shoebox (the cursor item is the pulled-out spine) | The first touch pulls the spine out; `Reproducir` in the insert plays |
| MP3 | The LCD browser in Inicio and Favoritos | `▲`/`▼`/`OK` bind to `MoveCommand(-1)`, `MoveCommand(1)` and `ActivateCommand` |
| Clásico de rueda | The Emisoras and Favoritas menus | The wheel binds to the same commands |

Boombox and Mesa de DJ need no cursor: they use a plain `CollectionView` whose row tap and `CARGAR` call `PlayHereCommand`.

### 5. `TuningDial` for Radio de válvulas

The dial is horizontal and the needle stays centred, so it is a separate control. Like `SleeveStack`, it draws only the visible labels (5 views reused), so 100 stations cost 5 views.

- **Properties:** `ItemsSource`, `ItemTemplate`, `FocusedItem`, `FocusedIndex`, `ActivateCommand`, `FocusOnReset`.
- **Input:** a `SwipeGestureRecognizer` (Left/Right); a `TuningKnob` button child, which is the focusable element and takes Left/Right on Windows; and the mouse wheel on Windows.
- **Accessible name:** the knob's name follows the focus (`Sintonizar, emisora {n} de {total}: {nombre}`).
- **Knob dragging is not built.** The brief's rotating-knob drag would need angle maths for little gain: the knob steps by press, swipe or keys. If the developer wants drag later, it is an addition.

### 6. Shared spin logic: `SpinDriver`

`VinylDisc`'s angular-speed integration moves into a small `SpinDriver` class that both controls and helpers can use. It holds:
- the target speed;
- the acceleration and deceleration times;
- one dispatcher timer at about 16 ms, running only while shown and moving;
- a callback that applies the angle;
- `SnapToState()` for `Appearing` and for reduced motion.

| Used by | Target speed | Speed up / slow down |
|---|---|---|
| `VinylDisc` | 198°/s | 1.2 s / 1.8 s |
| `CassetteReels` | 180°/s | 0.8 s / 1.0 s |
| `JogWheel` | 198°/s | 1.2 s / 1.8 s |

- `VinylDisc` keeps its public properties and behaviour; Tocadiscos is checked again after the move.
- **Mesa de DJ gets a new `JogWheel` control**, not a restyled `VinylDisc`. The drawings differ (platter ring and ticks against grooves and shine), and `VinylDisc`'s drawing stays untouched.

### 7. Decorative animated controls

All of these:
- derive from `PageAwareView`;
- take `IsActive` bound to the view model's playing-or-buffering state, the same source Tocadiscos uses;
- read `MotionSettings.IsReducedMotion` on `Appearing`;
- set `AutomationProperties.IsInAccessibleTree="False"` and are not focusable.

| Control | Movement | Reduced motion |
|---|---|---|
| `SpeakerCone` (Boombox) | `ScaleTo` loop of 1 → 1.04, 450 ms each way | Scale 1 |
| `LevelMeter` (Boombox, Mesa de DJ, MP3): `Segments`, `Bars`, `Orientation` | One 100 ms timer per meter; levels from a fixed pseudo-random sequence, smoothed; empties over 400 ms on stop | A fixed middle level while active, empty otherwise |
| `MagicEye` (Válvulas) | Two `Path` sectors whose sweep animates over 0.8 s | Final shape at once |
| `CassetteReels` (Casete) | Two reel groups driven by `SpinDriver` | No rotation |
| `MarqueeLabel` (MP3): `Text` | `TranslationX` loop at 40 units/s with 1 s end pauses, only when the text is wider than the box | Wraps to two lines with `TailTruncation` |

- **Accessible name of `MarqueeLabel`.** Its `SemanticProperties.Description` is the full text, and the moving copy is hidden from screen readers.
- **Warm-up glow (Válvulas).** The dial's lit overlay changes `Opacity` (`FadeTo`) in the layout itself, triggered by a `DataTrigger` on the state. It needs no control.
- **Why one timer per control** rather than a shared clock: it is simpler to explain and stop. At most three timers run, and only on the visible page.

### 8. `ClickWheel` for Clásico de rueda

`ClickWheel` is a `ContentView` with a ring, four real `Button`s at the cardinal points and a centre `Button`, all with Spanish accessible names.

- **Rotation.** A `PanGestureRecognizer` on the ring converts each pan update to an angle around the centre (`atan2`). The angle change is accumulated, and every 24° runs `ScrollStepCommand(±1)`.
- **Bindable commands:** `ScrollStepCommand`, `CenterCommand`, `MenuCommand`, `PreviousCommand`, `NextCommand`, `PlayPauseCommand`, `IsScrollEnabled`.
- **Bindings in Inicio and Favoritos:**
  - the ring and wheel to `CursorList.MoveCommand`;
  - the centre to `ActivateCommand`;
  - `◀◀`/`▶▶` to `PageMoveCommand(∓1)`;
  - play/pause to the view model's `PlayHereCommand` with the current station when it is listed. When the current station is not listed, play/pause does nothing.
- **Bindings in Reproductor:** play/pause to `ToggleCommand`; `◀◀`/`▶▶` to `PreviousFavoriteCommand` and `NextFavoriteCommand`; the centre to `ToggleFavoriteCommand`; `IsScrollEnabled` is false.
- **`MENÚ`** binds to `AppNavigation.GoToSectionCommand` with `inicio`, and is disabled in `Inicio`.
- **Windows.** The layout's root hook (as in `SleeveStack`) maps Escape, Space, Left and Right to the wheel's commands. Up/Down, Enter and the mouse wheel go to `CursorList`.
- **Fallback.** If the angle gesture is unreliable on the Android emulator, the ring falls back to a vertical pan (up/down = steps). That is reported in the group summary; the specs still hold, because rows are touchable and keys work.

### 9. Headers, bars and labels follow the shared contract

- Every theme keeps the required upper-case titles (`INICIO`, `REPRODUCTOR`, `FAVORITOS`, `AJUSTES`, …) centred in its header. The mock-up's screen captions (`EMISORAS`, `Reproduciendo`, `/EMISORAS/`) appear only as secondary captions or paths.
- Themes without a bar on `Reproductor` reuse the Tocadiscos pattern: a theme-only `ReproductorHeaderTemplate` with the house button bound to `AppNavigation.GoToSectionCommand` `inicio`.
- Bottom bar buttons may be text keys without icons (Casete, Boombox, MP3, Mesa de DJ). This is why the `app-navigation` delta says "each with a label". They reuse `BottomBar`'s `IsInicio`/`IsReproductor`/`IsFavoritos` for the marked state and its accessibility.
- Every visible label is Spanish. The REW/FF/PLAY/STOP stamps on the Casete keys are drawn letters, and their accessible names are the Spanish names of the specs.

### 10. Palettes and contrast

The palette values are the ones in the briefs. They were measured with the WCAG formula when the briefs were written: every text pair is at least 4.5:1 after the two Rueda fixes.

Each theme group re-measures the pairs its templates actually use, with the same script (kept outside the app). The table goes into the README.

Rules carried into the layouts:
- **Válvulas `Baquelita`:** text on the wooden strip uses the bar colours, never `PrimaryTextColor` (1.16:1).
- **Casete:** orange is never used for text on `SurfaceColor` (3.88:1).
- **Rueda `Blanco`:** text on the highlighted row is `AccentTextColor`.
- **Rueda `Negro` (changed in group 8):** one layout serves both palettes, so the highlighted row's text is `AccentTextColor` in `Negro` too. Its accent is therefore a deep blue `#2E63C0` with white `AccentTextColor` (5.73:1; 7.06:1 on the highlighted row) instead of the brief's light blue with dark text; the light blue stays for the hearts and the marked bar item.

### 11. Art and licence

- All art is drawn with MAUI shapes, gradients bound to palette keys, and the Phase 5 icons. No literal colour appears anywhere outside the palette files.
- One `Resources/Images/Themes/LICENSE.txt` states that the art of the six themes (in their layouts and in `Views/Controls/`) is original work for this project. It ships as `Licences\Themes-LICENSE.txt`, like the Tocadiscos one.
- If a shape cannot give an acceptable result, implementation stops and asks instead of shipping placeholders.

### 12. Task order

- **Group 1:** the prerequisites (Phase 6's unticked checks, removal of the TEMP-DIAG calls) and the variant memory.
- **Group 2:** the shared controls (`PagedSlots`, `CursorList`, `SpinDriver` with `VinylDisc` moved onto it, `LevelMeter`), each checked on a temporary page.
- **Groups 3–8:** one theme each, in the briefs' order:

  | Group | Theme | Reuses | Adds |
  |---|---|---|---|
  | 3 | Radio de válvulas | `PagedSlots` | `TuningDial`, `MagicEye` |
  | 4 | Casete portátil | `CursorList`, `PagedSlots`, `SpinDriver` | `CassetteReels` |
  | 5 | Boombox | `PagedSlots`, `LevelMeter` | `SpeakerCone` |
  | 6 | Mesa de DJ | `PagedSlots`, `LevelMeter`, `SpinDriver` | `JogWheel` |
  | 7 | MP3 | `CursorList`, `LevelMeter` | `MarqueeLabel` |
  | 8 | Clásico de rueda | `CursorList` | `ClickWheel` |

- **Group 9:** the README and the full run.

Each group ends with the three builds and a stop, so the developer can review one theme at a time.

## Risks / Trade-offs

- **[Change size]** Six themes in one change is large. → Each theme is its own task group that ends in a working, registered theme. The developer can stop after any group and the app stays consistent, because unfinished themes are not yet in the catalogue: each theme is registered only in its own group.
- **[`CollectionView` selection on Windows]** Driving the cursor through `SelectedItem` may fight with the tap gesture. → Verified on a temporary page in group 2. Fallback: an `IsCursor` flag computed per item view by the control, in the style of `ItemBinding`, without touching the view models.
- **[Pan-to-angle gesture on Android]** → The fallback in decision 8, reported in the group 8 summary.
- **[`VinylDisc` refactor breaks Tocadiscos]** → The Tocadiscos turntable checks are re-run at the end of group 2.
- **[Too many animations]** → At most three timers, only on the visible page and only while active. Measured on the Android emulator in groups 5 and 6.
- **[Header titles differ from the mock-up]** → An accepted trade-off of the shared navigation contract, explained in decision 9.
- **[Paging favourites in a J-card with removal]** → `PagedSlots` clamps to the last page, verified with 11 favourites in group 4.

## Migration Plan

- **No data migration.** Stored values of Phase 6 stay valid. The new theme ids are new values, and older builds fall back to Moderno Claro.
- **Variant ids.** The new variant ids map by light/dark (decision 1), so a stored `nogal` read by a build without Radio de válvulas gives Moderno Oscuro once that build has the decision 1 change. A Phase 6 build gives Moderno Claro.
- **Rollback.** Remove a theme's catalogue entry. A stored id of that theme then falls back to Moderno, with the light/dark mapping.
