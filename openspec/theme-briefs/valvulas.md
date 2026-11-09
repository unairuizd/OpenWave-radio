# Theme brief: Radio de válvulas

## 1. Concept

A 1940s–50s valve (tube) table radio: a cabinet of bakelite or walnut, a cloth speaker grille, a long glass dial
with a frequency scale and a red needle, two big round knobs, a row of ivory preset keys and the green "magic eye"
tuning indicator. Stations sit along the dial: turning the tuning knob moves the needle from one station to the
next. The radio "warms up" when it starts playing: the dial lamp glows in and the magic eye closes.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `valvulas` |
| `DisplayName` | `Radio de válvulas` |
| Palettes | `baquelita` "Baquelita" (`Light`), `nogal` "Nogal" (`Dark`) |
| Layout class | `Resources/Themes/ValvulasLayout.xaml` (+ `.cs`) |
| Palette classes | `ValvulasBaquelitaPalette.xaml`, `ValvulasNogalPalette.xaml` (+ `.cs`) |

Two palettes, so `Variante` is shown for this theme. Their ids are new (`baquelita`, `nogal`), so Phase 6's
variant memory (decision 9) applies: switching here from Moderno uses the first palette and keeps the stored
Moderno variant only if the theme has a single palette. **Decision needed:** whether a two-palette theme with
different ids may overwrite `appearance_theme_variant` (today's code does) or should map `light`→`baquelita`,
`dark`→`nogal`. Mapping is simpler for the user and needs a small `AppearanceService` change.

## 3. Palette

### `baquelita` (Light): cream bakelite, dark brown text, bottle-green magic eye

`SelectedItemBackgroundColor` role: **background** (the lit dial window behind the focused station).

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#EFE3C8` | cream bakelite cabinet |
| `SurfaceColor` | `#F8F0DD` | dial glass, information panel |
| `InputBackgroundColor` | `#E2D3B2` | search field, knob faces |
| `DividerColor` | `#B89F78` | scale ticks, grille threads, separators |
| `SelectedItemBackgroundColor` | `#DCCDA8` | lit dial window behind the focused station |
| `PrimaryTextColor` | `#2B1B0E` | dark brown text |
| `SecondaryTextColor` | `#5E4630` | scale numbers, characteristics |
| `ErrorTextColor` | `#A3261B` | errors |
| `AccentColor` | `#2F6B45` | magic eye, buttons |
| `AccentTextColor` | `#FFFFFF` | text on green buttons |
| `FavoriteIconColor` | `#9C2F1F` | hearts, the needle |
| `TabBarBackgroundColor` | `#3A2616` | wooden base strip (bottom bar, header) |
| `TabBarSelectedColor` | `#F2C46D` | marked bar item, header title |
| `TabBarUnselectedColor` | `#E8DCC2` | ivory: preset keys, unmarked bar items |

Measured: Primary/Page 13.03, Primary/Surface 14.61, Primary/Selected 10.54, Secondary/Page 6.89,
Secondary/Surface 7.73, Error/Page 5.79, Error/Surface 6.49, AccentText/Accent 6.34, Favorite/Page 5.81,
TabSelected/TabBar 8.78, TabUnselected/TabBar 10.52.

**Rule for this palette:** text on `TabBarBackgroundColor` uses `TabBarSelectedColor` or `TabBarUnselectedColor`,
never `PrimaryTextColor` (1.16:1). Preset key captions are `PrimaryTextColor` on ivory.

### `nogal` (Dark): walnut cabinet, warm ivory text, glowing green eye

`SelectedItemBackgroundColor` role: **outline**.

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#24170E` | dark walnut |
| `SurfaceColor` | `#352418` | lit dial glass (warm), panel |
| `InputBackgroundColor` | `#0F0A06` | inputs, knob faces |
| `DividerColor` | `#5A4330` | scale ticks, grille |
| `SelectedItemBackgroundColor` | `#7BD49A` | outline of the focused station on the dial |
| `PrimaryTextColor` | `#F1E4C8` | ivory text |
| `SecondaryTextColor` | `#CDB892` | scale numbers |
| `ErrorTextColor` | `#FF9E85` | errors |
| `AccentColor` | `#7BD49A` | glowing magic eye, buttons |
| `AccentTextColor` | `#0D2416` | text on green |
| `FavoriteIconColor` | `#F2B35B` | hearts, needle |
| `TabBarBackgroundColor` | `#170F09` | base strip |
| `TabBarSelectedColor` | `#8FE0AA` | marked bar item |
| `TabBarUnselectedColor` | `#E3D5B8` | ivory keys, unmarked items |

Measured: Primary/Page 13.85, Primary/Surface 11.77, Secondary/Page 9.03, Secondary/Surface 7.67, Error/Page 8.71,
Error/Surface 7.40, AccentText/Accent 9.15, Favorite/Page 9.45, Selected/Page 9.74, TabSelected/TabBar 12.10,
TabUnselected/TabBar 13.06.

## 4. Pages

### Header and bottom bar

> **Phase 8:** the bottom bar is now the same icon bar in every theme (`Views/BottomBar.cs`: house, radio and star in the palette's `TabBar*` colours). Only where the theme shows it still comes from this brief; the bar look described here is no longer drawn.

- Header: a wooden strip with the title engraved (`TabBarSelectedColor`) and the gear.
- Bottom bar: the wooden base of the cabinet with three ivory "feet" buttons (Inicio, Reproductor, Favoritos);
  the marked one is lit in `TabBarSelectedColor`.

### Station item (`StationCardTemplate`)
A small dial label: the station name in a lit window (`SurfaceColor`), the logo as a round badge and the heart.
Used in the dial strip and in the Favoritos list.

### Inicio
Rows: header / search row / filter bar / **dial** / information panel / bottom bar.
- **Dial (`TuningDial`):** a horizontal glass strip with a printed scale; stations are placed along it at equal
  spacing, about 5 visible at a time. A red needle (`FavoriteIconColor`) stands on the focused station, which is
  drawn larger and in the lit window. The dial scrolls under the needle; the needle stays centred.
- **Tuning knob:** a round knob at the right of the dial. Dragging it, swiping the dial, the mouse wheel or
  Left/Right moves the focus one station. It is the dial's main accessible control (`Sintonizar`).
- **Information panel** below, on the cloth grille: name, characteristics, heart, and a `Escuchar` button
  (`AccentColor`) that plays in place (`PlayHereCommand`). Pressing the focused station on the dial does the same.

### Favoritos
- The account row, then the favourites shown as a vertical list of dial labels on cloth (a radio log book).
- Pressing a label plays in place; the heart removes. Logged out and empty states centred on the grille.

### Reproductor
Rows: header (house button to Inicio + gear, like Tocadiscos' `ReproductorHeaderTemplate`) / cabinet / presets.
- **Cabinet:** the lit dial with only the current station under the needle; to its right the **magic eye** (a
  circle with a green fan that closes when playing) and the volume-looking **power knob**, which is the
  play/stop button (`ToggleCommand`, `Encender` / `Apagar`).
- Status line under the dial: `StatusText`, error and `Reintentar`.
- The heart sits on the cabinet, under the dial.
- **Preset keys (`PresetKeys`):** five ivory keys under the cabinet, one favourite each, the current one pressed
  down. More than five favourites: two small arrow keys at the ends change the page (`1/2`). Logged out: the keys
  are blank and pressing any one opens login. No bottom bar on this page.

### Ajustes and settings pages

> **Phase 8:** superseded. `Ajustes`, `Personalización`, `Preferencias` and the filter pages are shared by every theme and only take the palette colours (see the README, "Ajustes and Personalización"). The settings look this section described is no longer drawn.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | search row and filter bar above the dial | `SearchCommand`, `ClearSearchCommand`, filter commands |
| Choose and play | knob/dial focus, then `Escuchar` or press the focused station | `PlayHereCommand` |
| Play / stop | power knob in Reproductor; `Escuchar`/`Apagar` in Inicio and Favoritos | `ToggleCommand`, `PlayHereCommand` |
| Status, error, `Reintentar` | under the dial (Reproductor), in the panel (Inicio) | `StatusText`, `RetryCommand`, `PlaybackStatusText` |
| Heart | panel (Inicio), labels (Favoritos), cabinet (Reproductor) | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | preset keys | `Favorites`, `SelectFavoriteCommand`, `LogInCommand` |
| Account row | Favoritos | `LogOutCommand`, `LogInCommand` |
| Ajustes | gear | `AppNavigation.OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `TuningDial` | new | `ItemsSource`, `ItemTemplate`, `FocusedItem`, `ActivateCommand`, `FocusOnReset` | Same focus and reset rules as `SleeveStack`; draws only the visible labels (about 7 views) |
| `TuningKnob` | new (part of `TuningDial`) | `StepCommand` | Rotation follows each step; it is the focusable element |
| `MagicEye` | new | `IsActive` | Two `Path` sectors whose sweep angle animates |
| `PresetKeys` | new, reusable | `ItemsSource`, `KeysPerPage`, `SelectCommand`, `LogInCommand`, `IsLoggedIn` | Paging logic as in `RecordCrate`; reused by Casete, Boombox and Mesa de DJ |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- **Warm-up:** when `IsActive` becomes true, the dial window brightens over 1.5 s (opacity of a lit overlay
  0.3 → 1) and the magic eye closes over 0.8 s. When it stops, both fade back in 0.6 s.
- **Needle:** moves to the new focus in 250 ms (`CubicOut`); the knob turns 30° per step.
- Reduced motion: every change is instant. Hidden page: animations cancelled, state snapped on `Appearing`.

## 8. Input and accessibility

- Touch: swipe the dial, drag the knob, press keys. Windows: wheel and Left/Right on the dial, Enter/Space plays,
  Tab order header → search → filters → knob → panel → bar.
- Spanish names: `Sintonizar, emisora {n} de {total}: {nombre}`, `Escuchar`, `Encender`, `Apagar`,
  `Presintonía {n}: {nombre}`, `Página anterior de presintonías`, `Página siguiente de presintonías`.
- The cloth grille, scale numbers and the magic eye are decorative and hidden from accessibility (the eye's state
  is already in `StatusText`).

## 9. View-model needs

None beyond Phase 6 (`PlayHereCommand`, `PlaybackStatusText`, `Favorites`, `SelectFavoriteCommand`,
`LogInCommand`, `IsLoggedIn`).

## 10. Open questions and risks

- Variant id mapping (section 2).
- Rotating-knob drag needs a `PanGestureRecognizer` and angle maths; if it is unreliable on Android, the swipe on
  the dial is enough and the knob becomes a two-button stepper.
- 100 stations on a dial: only visible labels are created, like `SleeveStack`.

## 11. Manual checks

- [ ] Both palettes in Personalización; switching palettes while playing keeps the audio.
- [ ] Dial: swipe, knob, wheel and Left/Right move one station; the needle follows; a new search focuses the first.
- [ ] `Escuchar` plays in place; the warm-up runs once; reduced motion makes it instant.
- [ ] Presets: 7 favourites → 5 + `1/2`, the current key pressed, logged out opens login.
- [ ] `Pequeño` and `Enorme` text: labels and keys truncate, the dial height does not depend on text size.
