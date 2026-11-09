# Theme brief: Mesa de DJ

## 1. Concept

A club DJ booth: a dark controller with a track browser screen, one deck with a big jog wheel, a mixer strip and
a grid of glowing performance pads. Inicio is the browser of a DJ program (a dense, sortable-looking list), the
player is the deck (the jog wheel spins while the station plays) and the favourites are the 8 pads.

Only one deck is drawn: a second deck or a crossfader would suggest mixing two stations, which the app cannot do.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `mesadj` |
| `DisplayName` | `Mesa de DJ` |
| Palettes | `club` "Club" (`Dark`) |
| Layout class | `Resources/Themes/MesaDjLayout.xaml` (+ `.cs`) |
| Palette classes | `MesaDjClubPalette.xaml` (+ `.cs`) |

## 3. Palette

`SelectedItemBackgroundColor` role: **background** (the highlighted browser row, like DJ software).

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#0D0F12` | controller body |
| `SurfaceColor` | `#1A1E24` | browser screen, panels |
| `InputBackgroundColor` | `#050608` | jog wheel platter, pads off, inputs |
| `DividerColor` | `#2E343D` | grid lines, browser row separators |
| `SelectedItemBackgroundColor` | `#12343A` | loaded/current row background |
| `PrimaryTextColor` | `#EEF2F6` | text |
| `SecondaryTextColor` | `#A7B0BC` | browser columns (country, codec, bitrate) |
| `ErrorTextColor` | `#FF7A7A` | errors |
| `AccentColor` | `#00D1E0` | cyan: PLAY, jog ring, lit pads |
| `AccentTextColor` | `#00191C` | text on cyan |
| `FavoriteIconColor` | `#FF8A2B` | orange: hearts, the current pad |
| `TabBarBackgroundColor` | `#08090B` | bottom strip |
| `TabBarSelectedColor` | `#00D1E0` | marked item |
| `TabBarUnselectedColor` | `#8D96A3` | unmarked items, pad captions |

Measured: Primary/Page 17.06, Primary/Surface 14.87, Primary/Selected 11.83, Secondary/Page 8.75,
Secondary/Surface 7.63, Error/Page 7.60, Error/Surface 6.63, AccentText/Accent 9.69, Favorite/Page 8.15,
TabSelected/TabBar 10.62, TabUnselected/TabBar 6.66.

## 4. Pages

### Header and bottom bar

> **Phase 8:** the bottom bar is now the same icon bar in every theme (`Views/BottomBar.cs`: house, radio and star in the palette's `TabBar*` colours). Only where the theme shows it still comes from this brief; the bar look described here is no longer drawn.

- Header: a thin top strip like a DJ program's title bar: title left, gear right, a small `LIVE` tag that lights
  (`AccentColor`) while a station plays.
- Bottom bar: three flat pads with a cyan underline for the marked section.

### Station item (`StationCardTemplate`)
A browser **row**: logo (small), name, and columns for country, codec and bitrate from the characteristics, plus
the heart. The current station row uses `SelectedItemBackgroundColor` and a cyan bar on its left edge.

### Inicio
Rows: header / search row (styled as the browser's search box) / filter bar (as browser tabs) / column headings /
browser list / bottom bar.
- The column headings are labels only (no sorting: the API already decides the order).
- Each row has a `LOAD` button (`AccentColor`) that plays in place (`PlayHereCommand`); pressing the row itself
  does the same. On narrow screens the columns collapse into one secondary line.

### Favoritos
- The account row, then the browser filtered to favourites (heart removes). Heading `FAVORITAS` in the browser's
  left "playlists" strip on wide screens.

### Reproductor
Rows: header (house + gear) / deck / pads.
- **Deck:** a big **jog wheel** (reuse `VinylDisc` restyled through a template key: a platter ring in
  `AccentColor`, the station logo in the centre). Beside it: the name, characteristics, status line, error and
  `Reintentar`, a vertical `LevelMeter` (decorative) and the heart.
- **Transport:** `CUE` (previous favourite, as on a deck's cue/back button), a big round `PLAY/PAUSE`
  (`ToggleCommand`) and `SYNC` is **not** drawn (it would do nothing). Next favourite is a `▶|` button.
- **Pads:** `PresetKeys` in a 4×2 grid, one favourite per pad, the current one in `FavoriteIconColor`; arrows and
  `1/2` with more than 8. Logged out: pads dark and a cover button opens login. No bottom bar.

### Ajustes and settings pages

> **Phase 8:** superseded. `Ajustes`, `Personalización`, `Preferencias` and the filter pages are shared by every theme and only take the palette colours (see the README, "Ajustes and Personalización"). The settings look this section described is no longer drawn.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | browser search box and tabs | Inicio commands |
| Choose and play | `LOAD` or row press | `PlayHereCommand` |
| Play / stop | `PLAY/PAUSE`; `LOAD` on the playing row stops | `ToggleCommand`, `PlayHereCommand` |
| Status, error, `Reintentar` | deck info | `StatusText`, `RetryCommand` |
| Heart | row, deck | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | `CUE` / `▶|` and the pads | `PreviousFavoriteCommand`, `NextFavoriteCommand`, `SelectFavoriteCommand` |
| Account row | Favoritos | `LogOutCommand`, `LogInCommand` |
| Ajustes | gear | `AppNavigation.OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `VinylDisc` | reused | `LogoSource`, `IsSpinning` | Needs a way to restyle: either a `ControlTemplate` key per theme or a `Variant` property. Decide in the change |
| `LevelMeter` | reused (Boombox) | `Orientation` = vertical | Decorative |
| `PresetKeys` | reused | `KeysPerPage` = 8, `Columns` = 4 | Needs a `Columns` property if Phase 7 built it as one row |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- Jog wheel: the same speed-up/slow-down as the record in Tocadiscos.
- Pads: the current pad glows (opacity 0.7 ↔ 1, 1 s) while playing.
- `LIVE` tag: instant on/off, no blinking.
- Reduced motion: no rotation, no glow; colours still show the state.

## 8. Input and accessibility

- Windows: Up/Down in the browser, Enter = `LOAD`; arrow keys move between pads, Enter selects.
- Spanish names: `Cargar y reproducir {nombre}`, `Reproducir`, `Pausar`, `Favorita anterior`,
  `Favorita siguiente`, `Pad {n}: {nombre}`.
- Avoid English-only labels in the UI: `LOAD`, `CUE`, `LIVE` are styling, but their accessible names are Spanish;
  if the rule "every string shown in Spanish" applies to these too, use `CARGAR`, `ATRÁS`, `EN DIRECTO`
  (decision).

## 9. View-model needs

None beyond Phase 6. Bitrate and codec come from the characteristics already shown; if they are only one
combined string today, the columns become one line (no new DTO fields).

## 10. Open questions and risks

- English DJ terms vs. the Spanish-only rule (section 8).
- `VinylDisc` restyling (section 6).
- Dense rows must still give 44–48 units touch height on Android.

## 11. Manual checks

- [ ] Browser rows show the columns on Windows and collapse on Android; `LOAD` plays in place.
- [ ] Jog wheel spins only while playing; pads page with 10 favourites; current pad orange.
- [ ] Keyboard: browser and pads usable without a mouse.
