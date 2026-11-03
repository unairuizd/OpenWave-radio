# Theme brief: Casete portátil (Walkman)

## 1. Concept

A 1980s portable cassette player: a brushed blue-metal body, a window showing the tape and its two reels, chunky
mechanical keys on the top edge (REW, PLAY, STOP, FF) and orange foam headphones. Stations are tapes: Inicio is a
shoebox of cassettes seen by their spines, the player shows the tape turning, and favourites are a hand-written
mixtape J-card with side A and side B.

The UI name is generic (`Casete portátil`) and no brand or logo is drawn. See the trade-mark note in README.md.

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `casete` |
| `DisplayName` | `Casete portátil` (or `Walkman`, your call) |
| Palettes | `metal` "Metal azul" (`Dark`) |
| Layout class | `Resources/Themes/CaseteLayout.xaml` (+ `.cs`) |
| Palette classes | `CaseteMetalPalette.xaml` (+ `.cs`) |

A single palette: `Variante` is hidden and the Moderno variant is kept (Phase 6 decision 9).

## 3. Palette

`SelectedItemBackgroundColor` role: **outline** (orange outline of the current tape).

| Key | Value | Role |
|---|---|---|
| `PageBackgroundColor` | `#1F3A5F` | blue metal body |
| `SurfaceColor` | `#2A4A73` | lighter brushed panel, information panel |
| `InputBackgroundColor` | `#0E1A2B` | cassette window, inputs |
| `DividerColor` | `#4D6A90` | brushed lines, key gaps, tape edges |
| `SelectedItemBackgroundColor` | `#FF8C1A` | outline of the current tape |
| `PrimaryTextColor` | `#F2F6FB` | text |
| `SecondaryTextColor` | `#C3D1E3` | characteristics, spine captions |
| `ErrorTextColor` | `#FFB4A8` | errors |
| `AccentColor` | `#FF8C1A` | orange foam: PLAY key, buttons |
| `AccentTextColor` | `#2A1400` | text on orange |
| `FavoriteIconColor` | `#FFB24D` | hearts |
| `TabBarBackgroundColor` | `#152A45` | darker edge strip with the keys |
| `TabBarSelectedColor` | `#FFB24D` | marked bar item |
| `TabBarUnselectedColor` | `#D5DEEA` | brushed silver: keys, J-card paper, unmarked items |

Measured: Primary/Page 10.58, Primary/Surface 8.32, Primary/Input 16.11, Secondary/Page 7.41,
Secondary/Surface 5.83, Error/Page 6.75, Error/Surface 5.31, AccentText/Accent 7.52, Favorite/Page 6.41,
Selected/Page 4.93, TabSelected/TabBar 8.09, TabUnselected/TabBar 10.67.

Note: `AccentColor` on `SurfaceColor` is 3.88, fine for button shapes but not for orange text on the panel.
J-card text is `InputBackgroundColor`-dark text on silver paper: use `AccentTextColor` (dark) on
`TabBarUnselectedColor`.

## 4. Pages

### Header and bottom bar
- Header: the top edge of the player with the title stamped in silver and the gear.
- Bottom bar: three silver mechanical keys; the marked one is pushed in (lower `TranslationY`, orange label).

### Station item (`StationCardTemplate`)
A cassette **spine**: a long strip with a coloured label band (`SurfaceColor`), the station name hand-written on
it, a small logo square at the left and the heart at the right. The current station has the orange outline.

### Inicio
Rows: header / search row / filter bar / shoebox / bottom bar.
- **Shoebox:** a vertical `CollectionView` of spines (virtualised by the platform), seen from above inside a box
  edge. Pressing a spine pulls it half out (selection) and shows a small "insert" panel under it: name,
  characteristics, heart and `Reproducir` (`PlayHereCommand`). Pressing `Reproducir` plays in place; a second
  press on a playing tape stops it.
- Only one spine is pulled out at a time.

### Favoritos
- The account row, then the **J-card** (`JCardList`): a silver paper card with two columns, `CARA A` and
  `CARA B`, 5 favourites per side, written as a numbered tracklist. Pressing a line plays in place; each line has
  a small remove button. More than 10 favourites: a `Dar la vuelta` button flips to the next 10 (page indicator
  `1/3`).

### Reproductor
Rows: header (house + gear) / player body / keys / J-card.
- **Player body:** the cassette window (`CassetteReels`): the tape outline, its label with the station logo and
  name, and the two reels that turn while playing.
- **Keys row:** `REW` (previous favourite), `PLAY` (orange, `ToggleCommand`), `STOP` (`ToggleCommand` only when
  playing, disabled otherwise) and `FF` (next favourite). The pressed key stays down while playing.
- Status line, error and `Reintentar` under the window; the heart on the label.
- A compact one-side J-card (5 favourites) at the bottom with `Dar la vuelta`; logged out it shows
  `Inicia sesión para ver tus favoritas` and opens login. No bottom bar.

### Ajustes and settings pages
- Ajustes: two big silver keys with stamped labels.
- Settings pages: a brushed panel (`SurfaceColor`) with rounded corners.

## 5. Function map

| Function | Where | Bound to |
|---|---|---|
| Search / clear / filters | search row and filter bar | Inicio commands |
| Choose and play | pull a spine, `Reproducir` | `PlayHereCommand` |
| Play / stop | `PLAY` and `STOP` keys | `ToggleCommand` |
| Status, error, `Reintentar` | under the window | `StatusText`, `RetryCommand` |
| Heart | insert panel, J-card lines (remove), tape label | `ToggleFavoriteCommand`, `RemoveCommand` |
| Favourites from Reproductor | `REW`/`FF` and the J-card | `PreviousFavoriteCommand`, `NextFavoriteCommand`, `SelectFavoriteCommand` |
| Account row | Favoritos | `LogOutCommand`, `LogInCommand` |
| Ajustes | gear | `AppNavigation.OpenAjustesCommand` |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|
| `CassetteReels` | new, reusable (Boombox) | `IsSpinning`, `LogoSource`, `Title` | Two reel groups; reuses the ω-integration timer of `VinylDisc` |
| `JCardList` | new | `ItemsSource`, `LinesPerSide`, `Sides`, `SelectCommand`, `RemoveCommand`, `LogInCommand`, `IsLoggedIn` | Paging like `RecordCrate`; `Sides` = 2 in Favoritos, 1 in Reproductor |
| `PageAwareView`, `MotionSettings` | reused | — | — |

## 7. Motion

- Reels: target 180°/s while playing, speed up in 0.8 s, slow down in 1.0 s; both reels share one timer.
- Keys: a 120 ms press-down; `PLAY` stays down while `IsActive`.
- Spine pull-out: 200 ms `TranslationX`.
- J-card flip: 300 ms `RotationY` 0 → 90°, swap the page, 90 → 0°.
- Reduced motion: no reel rotation, instant keys and flips.

## 8. Input and accessibility

- Windows: Up/Down in the shoebox, Enter pulls out / plays; Left/Right on the J-card switches side.
- Spanish names: `Rebobinar: favorita anterior`, `Reproducir`, `Detener`, `Avanzar: favorita siguiente`,
  `Dar la vuelta a la cinta`, `Cara A, pista {n}: {nombre}`, `Quitar {nombre} de favoritas`.
- The tape window and reels are decorative; the label text is read through the station name.

## 9. View-model needs

None beyond Phase 6. `STOP` reuses `ToggleCommand` with `IsEnabled` bound to `IsActive`.

## 10. Open questions and risks

- `CollectionView` selection to pull out a spine: if it fights with the heart and `Reproducir` buttons inside the
  item, the pull-out becomes a button on the spine instead of selection.
- Hand-written look: without a theme font it is just the normal font slightly rotated. A hand-writing font is an
  optional extra (needs a font key and asset; decision).

## 11. Manual checks

- [ ] Spines: one pulled out at a time, `Reproducir` plays in place, the orange outline follows the current station.
- [ ] Reproductor: reels turn only while playing, `REW`/`FF` change favourite, `STOP` disabled when stopped.
- [ ] J-card: 12 favourites → A/B with 5 + 5, `Dar la vuelta` shows the other 2; removing clamps the page.
- [ ] Reduced motion and hidden page stop the reels.
