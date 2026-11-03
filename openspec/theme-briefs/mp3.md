# Theme brief: MP3

## 1. Concept

A cheap mid-2000s MP3 stick: a small dark plastic body, a blue back-lit monochrome LCD, a few rubber buttons
(`▲`, `▼`, `OK`, `◀◀`, `▶||`, `▶▶`, `M`) and a lanyard. Everything happens on the tiny screen: Inicio is a file
browser (`EMISORAS/`), the player shows a scrolling marquee with the station name and a little equaliser, and
favourites are the folder `FAVORITAS/`.

The physical buttons are real controls that mirror what the screen offers, so the theme can be used by touch on
the screen or with the buttons.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `mp3` |
| `DisplayName` | `MP3` |
| Palettes | `azul` "Azul" (`Dark`) |
| Layout class | `Resources/Themes/Mp3Layout.xaml` (+ `.cs`) |
| Palette classes | `Mp3AzulPalette.xaml` (+ `.cs`) |

A second palette `verde` (green-yellow LCD, `Light`) is a natural extra but is left out to keep the phase small.

## 3. Palette

`SelectedItemBackgroundColor` role: **background** (the inverted cursor row on the LCD).

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#22252B` | plastic body |
| `SurfaceColor` | `#0A2A5C` | blue back-lit LCD |
| `InputBackgroundColor` | `#0B1220` | LCD bezel, inputs |
| `DividerColor` | `#3A4150` | button outlines, LCD separators |
| `SelectedItemBackgroundColor` | `#1C4E9A` | cursor row |
| `PrimaryTextColor` | `#D6ECFF` | LCD text |
| `SecondaryTextColor` | `#9DB8D6` | LCD secondary text |
| `ErrorTextColor` | `#FF9A8A` | errors |
| `AccentColor` | `#5CC8FF` | `OK` button, progress-like bars |
| `AccentTextColor` | `#031A2B` | text on accent |
| `FavoriteIconColor` | `#7CF0C8` | hearts (mint, the LCD's second colour) |
| `TabBarBackgroundColor` | `#16181C` | button panel |
| `TabBarSelectedColor` | `#5CC8FF` | marked item |
| `TabBarUnselectedColor` | `#A7B0BE` | rubber buttons, unmarked items |

Measured: Primary/Page 12.65, Primary/Surface 11.54, Primary/Selected 6.64, Secondary/Page 7.50,
Secondary/Surface 6.85, Error/Page 7.49, Error/Surface 6.83, AccentText/Accent 9.41, Favorite/Page 11.08,
TabSelected/TabBar 9.45, TabUnselected/TabBar 8.12.

## 4. Pages

### Header and bottom bar
- Header: the LCD's status line: `EMISORAS`, `FAVORITAS` or `REPRODUCIENDO`, a fake battery icon (decorative) and
  the gear.
- Bottom bar: the rubber button panel under the screen with the three sections as buttons labelled `INICIO`,
  `REPRODUCIR`, `FAVORITAS`.

### Station item (`StationCardTemplate`)
An LCD line: `▶` cursor when selected, the name (one line, ellipsis), the heart on the right, and a second line
with characteristics only when the line is under the cursor.

### Inicio
Rows: header / search row (on the LCD, as `BUSCAR: ____`) / filter bar / LCD browser / navigation buttons /
bottom bar.
- **LCD browser:** a list framed as a screen; the path `EMISORAS/` at the top and `{n}/{total}` at the bottom.
- **Navigation buttons** (`▲`, `▼`, `OK`) move a cursor one line and play in place (`PlayHereCommand`); touching a
  line moves the cursor there, touching it again plays it.

### Favoritos
- The account row, then the same LCD browser with the path `FAVORITAS/`. `OK` plays, the heart removes.

### Reproductor
Rows: header (house + gear) / LCD now-playing / button pad.
- **LCD now-playing:** logo in a small pixel-framed square, the **marquee** with the station name when it does not
  fit, the characteristics, a `LevelMeter` (8 thin bars) and the status line, error and `Reintentar`.
- **Button pad:** `◀◀` previous favourite, `▶||` play/stop (`ToggleCommand`), `▶▶` next favourite, and the heart.
- No crate or presets: favourites are reached with `◀◀`/`▶▶` and in Favoritos. No bottom bar.

### Ajustes and settings pages
- Ajustes: an LCD menu with two lines (`Preferencias`, `Personalización`) and the cursor.
- Settings pages: the content inside an LCD-framed panel.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | LCD search row and filter bar | Inicio commands |
| Choose and play | cursor + `OK`, or touch twice | `PlayHereCommand` |
| Play / stop | `▶||` | `ToggleCommand` |
| Status, error, `Reintentar` | LCD now-playing | `StatusText`, `RetryCommand` |
| Heart | LCD line, button pad | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | `◀◀` / `▶▶` | `PreviousFavoriteCommand`, `NextFavoriteCommand` |
| Account row | Favoritos | `LogOutCommand`, `LogInCommand` |
| Ajustes | gear | `AppNavigation.OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `LcdBrowser` | new | `ItemsSource`, `ItemTemplate`, `CursorItem`, `ActivateCommand`, `Path`, `FocusOnReset` | A `CollectionView` + cursor index; `▲`/`▼`/`OK` are its commands |
| `MarqueeLabel` | new | `Text`, `IsRunning` | Scrolls only when the text is wider than the box |
| `LevelMeter` | reused (Boombox) | `Segments` = 8 | Decorative |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- Marquee: 40 units/s, 1 s pause at each end; runs only while the page is visible.
- Level meter as in Boombox, smaller.
- Reduced motion: no marquee (name wraps to two lines with ellipsis), meter fixed.

## 8. Input and accessibility

- Windows: Up/Down move the cursor, Enter plays.
- Spanish names: `Subir`, `Bajar`, `Aceptar: reproducir {nombre}`, `Favorita anterior`, `Reproducir`, `Detener`,
  `Favorita siguiente`.
- The marquee's accessible text is the full name, read once, not the moving copy.
- The battery icon and the lanyard are decorative.

## 9. View-model needs

None beyond Phase 6. The cursor lives in `LcdBrowser`, not in the view model.

## 10. Open questions and risks

- A pixel font would sell the look (Phase 4 already ships Pixelify Sans as a user font choice). Making the theme
  force it needs a new font resource key (decision); otherwise the user can pick Pixelify Sans in Personalización.

## 11. Manual checks

- [ ] `▲`/`▼`/`OK`, touch and keyboard move the cursor and play in place.
- [ ] Marquee scrolls only for long names and stops when hidden or with reduced motion.
- [ ] `◀◀`/`▶▶` change favourite; `▶||` plays and stops.
