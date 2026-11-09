# Theme brief: Boombox

## 1. Concept

A 1980s street boombox: a black plastic body with chrome trim, two big speakers on the sides, a central digital
tuner display, a row of preset buttons, a cassette door and a telescopic antenna. The player page is the whole
boombox: the speakers pulse while a station plays and a segment level meter dances on the display. Inicio is the
tuner: the display shows the station being tuned and the list scrolls between the speakers.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `boombox` |
| `DisplayName` | `Boombox` |
| Palettes | `cromado` "Cromado" (`Dark`) |
| Layout class | `Resources/Themes/BoomboxLayout.xaml` (+ `.cs`) |
| Palette classes | `BoomboxCromadoPalette.xaml` (+ `.cs`) |

## 3. Palette

`SelectedItemBackgroundColor` role: **outline** (pink outline of the current station).

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#18181B` | black plastic body |
| `SurfaceColor` | `#2A2B30` | gunmetal panels, display bezel, information panel |
| `InputBackgroundColor` | `#0B0B0D` | speaker cones, display glass, inputs |
| `DividerColor` | `#4A4C54` | grille holes, panel seams |
| `SelectedItemBackgroundColor` | `#FF5FA2` | outline of the current station |
| `PrimaryTextColor` | `#F3F4F6` | text |
| `SecondaryTextColor` | `#B8BCC6` | characteristics |
| `ErrorTextColor` | `#FF8A80` | errors |
| `AccentColor` | `#FF4F9A` | hot pink: PLAY, buttons, level meter peak |
| `AccentTextColor` | `#1A0010` | text on pink |
| `FavoriteIconColor` | `#FFD23F` | hearts, the "FM" lamp |
| `TabBarBackgroundColor` | `#0F0F11` | bottom chassis |
| `TabBarSelectedColor` | `#3FE0D0` | cyan display digits, marked bar item, meter segments |
| `TabBarUnselectedColor` | `#C9CCD3` | chrome trim, preset buttons, unmarked items |

Measured: Primary/Page 16.10, Primary/Surface 12.84, Secondary/Page 9.32, Secondary/Surface 7.43, Error/Page 7.76,
Error/Surface 6.19, AccentText/Accent 6.50, Favorite/Page 12.27, Accent/Surface 4.60, Selected/Page 6.25,
TabSelected/TabBar 11.66, TabUnselected/TabBar 11.91.

## 4. Pages

### Header and bottom bar

> **Phase 8:** the bottom bar is now the same icon bar in every theme (`Views/BottomBar.cs`: house, radio and star in the palette's `TabBar*` colours). Only where the theme shows it still comes from this brief; the bar look described here is no longer drawn.

- Header: the carrying handle as a chrome bar across the top, with the title and the gear; the antenna is a thin
  decorative line rising from the right.
- Bottom bar: the chassis edge with three square rubber buttons; the marked one has a cyan LED above it.

### Station item (`StationCardTemplate`)
A row on the display: logo square, name in display digits colour (`TabBarSelectedColor`), characteristics,
heart. The current station has the pink outline and a small pink "▶".

### Inicio
Rows: header / search row / filter bar / boombox front / bottom bar.
- **Boombox front:** a `Grid` with columns `1*,3*,1*` on wide screens: left speaker, centre display, right
  speaker. On narrow screens (Android portrait) the speakers become two short strips above and below the display.
- **Centre display:** a `CollectionView` of station rows (virtualised). Pressing a row plays in place
  (`PlayHereCommand`); pressing the playing one stops it. A `TUNING` caption with the count `{n} emisoras` sits on
  the display bezel.
- The speakers are decorative and pulse while a station plays.

### Favoritos
- The account row, then the same display listing only favourites (heart removes). The cassette door is drawn
  closed above the list with `MIS FAVORITAS` on its label.

### Reproductor
Rows: header (house + gear) / boombox / presets.
- **Boombox:** two big speakers (`SpeakerCone`) framing the display. The display shows the logo, the name in
  large digits, the characteristics, the status line (`StatusText`, error, `Reintentar`) and a `LevelMeter`
  (12 segments).
- **Transport keys** under the display: `◀◀` (previous favourite), `PLAY/STOP` (pink, `ToggleCommand`), `▶▶`
  (next favourite), and the heart.
- **Presets:** `PresetKeys` with 6 chrome buttons labelled `M1`–`M6` and a small caption with the station name
  under each; arrows when there are more than 6. Logged out: one cover button opens login. No bottom bar.

### Ajustes and settings pages

> **Phase 8:** superseded. `Ajustes`, `Personalización`, `Preferencias` and the filter pages are shared by every theme and only take the palette colours (see the README, "Ajustes and Personalización"). The settings look this section described is no longer drawn.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | search row and filter bar | Inicio commands |
| Choose and play | press a row on the display | `PlayHereCommand` |
| Play / stop | `PLAY/STOP` key; rows in Inicio and Favoritos | `ToggleCommand`, `PlayHereCommand` |
| Status, error, `Reintentar` | on the display | `StatusText`, `RetryCommand`, `PlaybackStatusText` |
| Heart | row, Reproductor transport row | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | `◀◀`/`▶▶` and the M1–M6 presets | `PreviousFavoriteCommand`, `NextFavoriteCommand`, `SelectFavoriteCommand` |
| Account row | Favoritos | `LogOutCommand`, `LogInCommand` |
| Ajustes | gear | `AppNavigation.OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `SpeakerCone` | new | `IsActive` | Concentric ellipses; the inner cone scales 1 → 1.04 in a loop while active |
| `LevelMeter` | new, reusable (Mesa de DJ, MP3) | `IsActive`, `Segments`, `Orientation` | Decorative pseudo-random levels; not real audio |
| `PresetKeys` | reused (from Radio de válvulas) | `KeysPerPage` = 6 | — |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- Speakers: a 450 ms in/out scale loop while `IsActive` (about 133 bpm), both speakers in phase.
- Level meter: one timer at 100 ms picks a new level per bar with smoothing; the top segment uses `AccentColor`.
- Stop: speakers settle to 1.0 and the meter falls to zero over 400 ms.
- Reduced motion: speakers still, meter shows a fixed mid level while playing and empty when stopped (state is
  still visible).

## 8. Input and accessibility

- Windows: Up/Down on the display list, Enter plays; Tab order header → search → filters → list → bar.
- Spanish names: `Favorita anterior`, `Reproducir`, `Detener`, `Favorita siguiente`, `Memoria {n}: {nombre}`.
- Speakers, antenna, handle and meter are decorative and hidden from accessibility.
- Flashing: the meter changes at 10 Hz but with small areas and no full-screen flashes; still, it must follow
  reduced motion.

## 9. View-model needs

None beyond Phase 6.

## 10. Open questions and risks

- Two pulsing speakers plus the meter cost two animations and one timer while Reproductor is visible: measure
  on the Android emulator.
- A "digital display" font would improve the look but needs a font key and asset (decision).

## 11. Manual checks

- [ ] Wide and narrow layouts: speakers beside the display on Windows, above/below it on Android portrait.
- [ ] Rows play in place and the pink outline follows the current station.
- [ ] Speakers and meter only move while playing; they stop when the page is hidden and with reduced motion.
- [ ] Presets: 8 favourites → M1–M6 + arrows; logged out opens login.
