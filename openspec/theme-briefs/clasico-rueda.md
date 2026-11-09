# Theme brief: Clásico de rueda (iPod clásico)

## 1. Concept

A mid-2000s pocket player with a screen at the top and a round touch wheel at the bottom: `MENÚ` at the top of
the wheel, previous/next on the sides, play/pause at the bottom and a centre button to select. The whole app is
a stack of menus on the screen: Inicio is the `Emisoras` menu, Favoritos the `Favoritas` menu and Reproductor the
"Reproduciendo" screen. Turning a finger around the wheel scrolls the menu.

The UI name is generic (`Clásico de rueda`); no brand, logo or Apple icon is drawn. See the trade-mark note in
README.md.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `rueda` |
| `DisplayName` | `Clásico de rueda` (or `iPod clásico`, your call) |
| Palettes | `blanco` "Blanco" (`Light`), `negro` "Negro" (`Dark`) |
| Layout class | `Resources/Themes/RuedaLayout.xaml` (+ `.cs`) |
| Palette classes | `RuedaBlancoPalette.xaml`, `RuedaNegroPalette.xaml` (+ `.cs`) |

Same variant-id question as Radio de válvulas: map `light`→`blanco` and `dark`→`negro`, or not (decision).

## 3. Palette

`SelectedItemBackgroundColor` role: **background** (the highlighted menu row). In `blanco` the row text is
`AccentTextColor` (white); in `negro` it is `PrimaryTextColor`.

### `blanco` (Light): white body, grey-blue screen

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#F4F4F2` | white body |
| `SurfaceColor` | `#E9EEF2` | screen |
| `InputBackgroundColor` | `#FFFFFF` | wheel, inputs |
| `DividerColor` | `#C9CDD2` | wheel ring, screen frame, menu separators |
| `SelectedItemBackgroundColor` | `#2A62C8` | highlighted row |
| `PrimaryTextColor` | `#111316` | text |
| `SecondaryTextColor` | `#555B63` | wheel labels, details |
| `ErrorTextColor` | `#B3261E` | errors |
| `AccentColor` | `#2A62C8` | buttons |
| `AccentTextColor` | `#FFFFFF` | text on the highlighted row and buttons |
| `FavoriteIconColor` | `#2A62C8` | hearts |
| `TabBarBackgroundColor` | `#F4F4F2` | the wheel area |
| `TabBarSelectedColor` | `#2A62C8` | marked item |
| `TabBarUnselectedColor` | `#5A5F66` | wheel labels |

Measured: Primary/Page 16.90, Primary/Surface 15.93, Secondary/Page 6.23, Secondary/Surface 5.87,
Error/Page 5.94, Error/Surface 5.60, AccentText/Accent 5.71, AccentText/Selected 5.71, Favorite/Page 5.18,
TabSelected/TabBar 5.18, TabUnselected/TabBar 5.84.

### `negro` (Dark): black body, dark screen

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#232427` | black body |
| `SurfaceColor` | `#0F1215` | screen |
| `InputBackgroundColor` | `#2E3034` | wheel, inputs |
| `DividerColor` | `#45484E` | wheel ring, separators |
| `SelectedItemBackgroundColor` | `#2556A8` | highlighted row |
| `PrimaryTextColor` | `#F0F2F4` | text |
| `SecondaryTextColor` | `#A9AEB5` | wheel labels, details |
| `ErrorTextColor` | `#FF8A80` | errors |
| `AccentColor` | `#6FA4FF` | buttons |
| `AccentTextColor` | `#06142E` | text on accent |
| `FavoriteIconColor` | `#6FA4FF` | hearts |
| `TabBarBackgroundColor` | `#18191B` | wheel area |
| `TabBarSelectedColor` | `#6FA4FF` | marked item |
| `TabBarUnselectedColor` | `#A9AEB5` | wheel labels |

Measured: Primary/Page 13.83, Primary/Surface 16.74, Primary/Selected 6.29, Secondary/Page 6.95,
Secondary/Surface 8.42, Error/Page 6.80, Error/Surface 8.23, AccentText/Accent 7.35, Favorite/Page 6.23,
TabSelected/TabBar 7.06, TabUnselected/TabBar 7.88.

## 4. Pages

Every page has the same two halves: the **screen** (top, about 55%) and the **wheel** (bottom). On wide Windows
windows the device is centred with a maximum width so it keeps its proportions; the page background fills the
rest.

### Header and bottom bar

> **Phase 8:** the bottom bar is now the same icon bar in every theme (`Views/BottomBar.cs`: house, radio and star in the palette's `TabBar*` colours). Only where the theme shows it still comes from this brief; the bar look described here is no longer drawn.

- Header: the screen's title bar: the menu title centred (`Emisoras`, `Favoritas`, `Reproduciendo`, `Ajustes`), a
  play-state glyph on the left and the gear on the right.
- Bottom bar: **replaced by the wheel.** `MENÚ` on the wheel goes back up one level (Reproductor → Inicio,
  Favoritos → Inicio, settings pages → Ajustes). Section changes happen through a small top-level menu: pressing
  `MENÚ` on Inicio shows `Emisoras / Reproduciendo / Favoritas / Ajustes` on the screen. **Decision needed:** this
  changes how sections are reached; the `app-navigation` spec must allow it (Tocadiscos already set the precedent
  of a page without the bar). The alternative is to keep a slim bottom bar under the wheel.

### Station item (`StationCardTemplate`)
A menu row: name (one line, ellipsis), heart, and a `›` chevron. The highlighted row uses the selected
background.

### Inicio
- Screen: the search row and filter bar as the first two menu rows (`Buscar…`, `Filtros: …`), then the station
  rows. The highlighted row is the wheel's cursor.
- Wheel: turning moves the cursor, the centre button plays in place (`PlayHereCommand`), play/pause toggles the
  current station, `◀◀`/`▶▶` jump a page of rows. Touching a row moves the cursor; touching it again plays.

### Favoritos
- Screen: the account row as the first menu row (email, `Cerrar sesión` as a sub-menu), then the favourites.
  Logged out: `Inicia sesión para ver tus favoritas` and an `Iniciar sesión` row.

### Reproductor
- Screen: "Reproduciendo": the logo large and square, the name, the characteristics, the status line, error and
  `Reintentar`, and a thin bar that is full and labelled `En directo` (no progress for a live stream).
- Wheel: centre button = heart, play/pause = `ToggleCommand`, `◀◀`/`▶▶` = previous/next favourite. Turning the
  wheel does nothing here (volume is out of scope); it is not focusable on this page.

### Ajustes and settings pages

> **Phase 8:** superseded. `Ajustes`, `Personalización`, `Preferencias` and the filter pages are shared by every theme and only take the palette colours (see the README, "Ajustes and Personalización"). The settings look this section described is no longer drawn.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | first menu rows in Inicio | Inicio commands |
| Choose and play | wheel cursor + centre, or touch twice | `PlayHereCommand` |
| Play / stop | wheel play/pause | `ToggleCommand`, `PlayHereCommand` |
| Status, error, `Reintentar` | Reproduciendo screen | `StatusText`, `RetryCommand` |
| Heart | row, centre button in Reproductor | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | `◀◀` / `▶▶` | `PreviousFavoriteCommand`, `NextFavoriteCommand` |
| Account row | Favoritos first row | `LogOutCommand`, `LogInCommand` |
| Sections and Ajustes | `MENÚ` top-level menu (or slim bar, decision) | `AppNavigation.GoToSectionCommand`, `OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `ClickWheel` | new | `ScrollStepCommand`, `CenterCommand`, `MenuCommand`, `PreviousCommand`, `NextCommand`, `PlayPauseCommand`, `IsScrollEnabled` | A ring + centre button; four hit areas at the cardinal points are real `Button`s |
| `WheelMenu` | new | `ItemsSource`, `ItemTemplate`, `CursorItem`, `ActivateCommand`, `FocusOnReset` | A `CollectionView` that scrolls to keep the cursor visible |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- Menu cursor moves instantly (like the real device), the list scrolls 120 ms to keep it in view.
- A short "click" scale on the wheel buttons (80 ms). No sound.
- Reduced motion: no scroll animation, no click scale.

## 8. Input and accessibility

- **Wheel rotation:** a `PanGestureRecognizer` on the ring computes the angle from the centre; every 24° is one
  step. Windows: the mouse wheel and Up/Down do the same, Enter = centre, Escape/Backspace = `MENÚ`, Space =
  play/pause, Left/Right = `◀◀`/`▶▶`.
- The ring itself is not the only way to scroll: rows are also touchable, so screen-reader users never need the
  rotation gesture.
- Spanish names: `Menú`, `Seleccionar`, `Reproducir o pausar`, `Anterior`, `Siguiente`, `Rueda: desliza en
  círculo para moverte por la lista`.

## 9. View-model needs

None for playback and favourites. The top-level `MENÚ` menu, if chosen, is a list of the existing navigation
commands built in the layout, not a new view model.

## 10. Open questions and risks

- **Navigation change** (section 4): the biggest decision of this theme; it may need an `app-navigation` spec
  delta.
- The pan-to-angle gesture is the hardest input in the whole project: if it is unreliable on the Android emulator,
  fall back to vertical swipe on the ring.
- Fixed device proportions on very small or very wide windows: define a max width and let the screen scroll.

## 11. Manual checks

- [ ] Ring rotation, mouse wheel and Up/Down move the cursor one row per step; centre plays in place.
- [ ] `MENÚ` goes back one level on every page; every section and Ajustes reachable.
- [ ] Reproduciendo: play/pause, `◀◀`/`▶▶`, heart on the centre, `En directo` bar.
- [ ] Both palettes, `Pequeño` and `Enorme` text: rows truncate, the wheel keeps its size.
