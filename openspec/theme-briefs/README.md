# Theme briefs

One brief per future theme, written against the theme engine of Phase 5 and the patterns of Phase 6 (Tocadiscos).
A brief is **not** an OpenSpec change: nothing here is implemented. To build a theme, start a change from its brief:

```
/opsx:propose phase-N-theme-<id> using openspec/theme-briefs/<file>.md
```

Each future change then gets its own proposal, design, specs and tasks, like `phase-6-theme-tocadiscos`.

## Theme list

| Theme the user asked for | Brief | Id | Spanish name | Status |
|---|---|---|---|---|
| Estilo reproductor moderno | — | `moderno` | Moderno | Done (Phase 5), the reference layout |
| Tocadiscos | `openspec/changes/phase-6-theme-tocadiscos/` | `tocadiscos` | Tocadiscos | In progress (Phase 6) |
| Radio de válvulas (antigua) | [valvulas.md](valvulas.md) | `valvulas` | Radio de válvulas | Brief |
| Walkman | [casete-portatil.md](casete-portatil.md) | `casete` | Casete portátil | Brief |
| Boombox (retro) | [boombox.md](boombox.md) | `boombox` | Boombox | Brief |
| Mesa de DJ | [mesa-dj.md](mesa-dj.md) | `mesadj` | Mesa de DJ | Brief |
| MP3 genérico | [mp3.md](mp3.md) | `mp3` | MP3 | Brief |
| iPod clásico | [clasico-rueda.md](clasico-rueda.md) | `rueda` | Clásico de rueda | Brief |

**Trade-mark names.** "iPod" and "Walkman" are registered brand names. The briefs use generic Spanish names in the
UI (`Clásico de rueda`, `Casete portátil`) and draw generic devices without logos. Using the brand names in a
course project is your call; changing them is only the `DisplayName` string.

## Suggested order

Ordered so each phase reuses controls from the previous ones and the hardest input (the click wheel) comes last.

| # | Theme | Reuses | Introduces | Effort |
|---|---|---|---|---|
| 7 | Radio de válvulas | `PageAwareView`, `MotionSettings`, `RecordCrate` paging logic | `TuningDial`, `PresetKeys` | Medium |
| 8 | Casete portátil | `VinylDisc` spin timer (as reels), `PresetKeys` | `CassetteReels`, `JCardList` | Medium |
| 9 | Boombox | `CassetteReels`, `PresetKeys` | `SpeakerCone`, `LevelMeter` | Medium |
| 10 | Mesa de DJ | `VinylDisc` (as jog wheel), `LevelMeter`, `PresetKeys` (as pads) | — | Low–medium |
| 11 | MP3 | `LevelMeter` | `MarqueeLabel` | Low |
| 12 | Clásico de rueda | — | `ClickWheel` | High |

## The contract every theme follows

These come from the code as it is today (`ThemeCatalog.cs`, `ModernoLayout.xaml`, `ModernoLightPalette.xaml`,
`TocadiscosLayout.xaml`). A brief that needs to break one of them must say so and stop for a decision.

### Palette: exactly 14 colour keys

`PageBackgroundColor`, `SurfaceColor`, `InputBackgroundColor`, `DividerColor`, `SelectedItemBackgroundColor`,
`PrimaryTextColor`, `SecondaryTextColor`, `ErrorTextColor`, `AccentColor`, `AccentTextColor`, `FavoriteIconColor`,
`TabBarBackgroundColor`, `TabBarSelectedColor`, `TabBarUnselectedColor`.

- Shared styles (`Styles.xaml`) use `SurfaceColor` as a background for entries, pickers and cards, and
  `TabBarUnselectedColor` as a text colour, so both must work in those roles in every theme.
- The Personalización preview draws `PrimaryTextColor` on `SurfaceColor`.
- `SelectedItemBackgroundColor` is used only by layouts, so each brief states its role: **background** (text on it
  must reach 4.5:1) or **outline** (3:1 against the page).
- Text pairs reach 4.5:1, icons and outlines 3:1. Every palette in these briefs was measured with the WCAG
  formula; the numbers are in each brief and must be re-measured on the real templates.
- Adding a colour key is a breaking change to the contract (every palette must define it). Briefs avoid it.

### Layout: the 8 keys, defined by the theme itself

`HeaderTemplate`, `BottomBarTemplate`, `StationCardTemplate`, `InicioTemplate`, `FavoritosTemplate`,
`ReproductorTemplate`, `AjustesTemplate`, `SettingsPageTemplate`, plus any theme-only keys (for example
Tocadiscos' `ReproductorHeaderTemplate`). A layout never merges or references another layout.

### Same functions in every theme

Every theme must offer, with Spanish accessible names:

- search with `Buscar` and `Limpiar búsqueda`, and the preference filters;
- choose a station and play it (by opening Reproductor, or in place as Tocadiscos does);
- play/stop, the status line (`StatusText`, error, `Reintentar`);
- the heart (add/remove, opens login when logged out);
- Favoritos with the account row (email + `Cerrar sesión`), the logged-out text and `Iniciar sesión`;
- reach the user's favourites from Reproductor (previous/next favourite, or a crate/bank like Tocadiscos);
- Ajustes, Personalización and Preferencias;
- the loading, error, empty and "no results with your preferences" states.

### Rules carried over from Phase 6

- Art is drawn with MAUI shapes and palette colours through `DynamicResource`. No raster art, no multi-colour SVG,
  no literal colours, font families or font sizes outside the palettes and `Styles.xaml`.
- Custom controls live in `Views/Controls/`, are pure views of bindable state and never know a service.
- Motion only observes playback state, stops when the page is hidden (`PageAwareView`) and is instant with reduced
  motion (`MotionSettings`).
- **No real audio analysis.** Meters, speaker pulses and equalisers are decorative loops that run while
  `IsActive` is true. `MediaElement` does not expose levels and adding that is out of scope.
- Decorative parts (speaker grilles, knobs, faders that do nothing) are not focusable and are hidden from
  accessibility (`SemanticProperties` / `AutomationProperties.IsInAccessibleTree="False"`). A control that looks
  pressable must either work or be clearly decorative.
- No service, API or database change. View-model members are additive only.
- Fonts: the font comes from the user's Personalización choice. A theme-specific font (for example a pixel font
  for MP3) would need a new resource key and a new asset; the briefs list it as an optional extra that needs a
  decision.

## Files

- [_TEMPLATE.md](_TEMPLATE.md): the empty template, for any other theme.
- One brief per theme, listed above.
