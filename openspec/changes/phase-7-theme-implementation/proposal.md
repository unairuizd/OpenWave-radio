# Proposal

## Why

Phases 5 and 6 built the theme engine and two themes (`Moderno` and `Tocadiscos`). The developer has six more themes designed: briefs in `openspec/theme-briefs/` and a visual mock-up canvas. Each one turns the app into a different audio device without changing what the app does. Shipping them completes the personalisation story of the course project. It also proves that the engine and the Phase 6 patterns (UI-only controls, motion that only observes the playback state, playback that continues across pages) scale beyond one custom theme.

**Prerequisite:** `phase-6-theme-tocadiscos` is archived, and the deltas below are written against the specs as it left them. Three of its manual checks were never ticked: `MotionSettings` (3.2), reduced motion (6.3) and the full run (7.1). This change relies on `MotionSettings`, so its first task closes them.

## What Changes

- **Six new themes**, registered in `ThemeCatalog` after `Moderno` and `Tocadiscos`, each with its own palette dictionaries and its own layout dictionary that defines every layout key without referencing another layout:
  - **`Radio de válvulas`** (`valvulas`). Two palettes, `Baquelita` (light) and `Nogal` (dark): a glass tuning dial with a needle and a tuning knob, a magic eye, a power knob, and ivory preset keys holding the favourites.
  - **`Casete portátil`** (`casete`). Palette `Metal azul`: a shoebox of cassette spines, the cassette window with turning reels, the REW/PLAY/STOP/FF keys, and a mixtape card (`CARA A` / `CARA B`) holding the favourites.
  - **`Boombox`** (`boombox`). Palette `Cromado`: a tuner display between pulsing speakers, a decorative level meter, transport keys, and `M1`–`M6` memories holding the favourites.
  - **`Mesa de DJ`** (`mesadj`). Palette `Club`: a track-browser list with `CARGAR`, one deck with a jog wheel, and 8 pads holding the favourites. All visible labels are in Spanish (`CARGAR`, `ATRÁS`, `SIGUIENTE`, `EN DIRECTO`).
  - **`MP3`** (`mp3`). Palette `Azul`: a back-lit LCD file browser with a cursor and `▲`/`OK`/`▼` buttons, a scrolling station name, a decorative equaliser, and `◀◀`/`▶▶` to change favourite.
  - **`Clásico de rueda`** (`rueda`). Two palettes, `Blanco` (light) and `Negro` (dark): screen menus and a click wheel (`MENÚ`, `◀◀`, `▶▶`, play/pause, centre button). A slim bottom bar stays under the wheel on every page.
- **Play in place.** In every new theme, `Inicio` and `Favoritos` play a station without leaving the page, as `Tocadiscos` does. Only `Moderno` opens `Reproductor`.
- **Variant memory by light/dark.** Every palette is the light or the dark palette of its theme; its existing `BaseTheme` already says which. Switching theme keeps the user's light/dark choice when the new theme has both (`Claro` ↔ `Baquelita` ↔ `Blanco`, `Oscuro` ↔ `Nogal` ↔ `Negro`). Single-palette themes keep the stored variant, as in Phase 6.
- **Reusable UI-only controls** in `Views/Controls/`: tuning dial, magic eye, preset keys (shared by Válvulas, Boombox and Mesa de DJ), cassette reels, J-card list, speaker cone, level meter, LCD browser, marquee label, click wheel and wheel menu. They are drawn with MAUI shapes and palette colours, and their animations only observe the playback state, stop when the page is hidden and respect reduced motion. `VinylDisc` gains a jog-wheel style for Mesa de DJ.
- **Shared rules generalised.** Requirements that Phase 6 stated per theme ("in `Moderno`… in `Tocadiscos`…") gain a clause for the other themes, so each new theme's own spec says where its controls are. A theme-wide requirement covers motion, reduced motion and decorative parts.
- **No new fonts.** The font stays the user's Personalización choice; the mock-up's display fonts are not shipped.
- **No service, API or database change, and no new view-model members** beyond Phase 6 (`PlayHereCommand`, `PlaybackStatusText`, `Favorites`, `SelectFavoriteCommand`, `LogInCommand`, `IsLoggedIn`, previous/next favourite).
- README: the theme list, the new controls and their licence note, the contrast tables, and the manual checklist.

## Capabilities

### New Capabilities

- `theme-valvulas`: the Radio de válvulas look: its palettes, dial and tuning knob in Inicio, the radio log in Favoritos, the cabinet (magic eye, power knob, presets) in Reproductor, its Ajustes, motion, accessibility and keyboard rules.
- `theme-casete`: the Casete portátil look: its palette, the shoebox of spines in Inicio, the J-card in Favoritos and Reproductor, the cassette window and transport keys, motion, accessibility and keyboard.
- `theme-boombox`: the Boombox look: its palette, the tuner display and speakers in Inicio and Favoritos, the speakers, display, level meter, transport keys and memories in Reproductor, motion, accessibility and keyboard.
- `theme-mesadj`: the Mesa de DJ look: its palette, the track browser in Inicio and Favoritos, the deck (jog wheel, transport, level meter) and pads in Reproductor, motion, accessibility and keyboard.
- `theme-mp3`: the MP3 look: its palette, the LCD browser with cursor buttons in Inicio and Favoritos, the LCD now-playing screen and button pad in Reproductor, the marquee, motion, accessibility and keyboard.
- `theme-rueda`: the Clásico de rueda look: its palettes, the screen menus and click wheel on Inicio, Favoritos and Reproductor, the slim bottom bar, motion, accessibility and keyboard.

### Modified Capabilities

- `app-appearance`: eight themes instead of two; Personalización lists them and shows the variant selector for every theme with two palettes; the variant is remembered by light/dark across themes; "same functions in every theme" names how each theme changes favourite from Reproductor; a new theme-wide requirement for animations, reduced motion and decorative parts.
- `app-navigation`: the bottom bar rule lists, per theme, whether `Reproductor` shows the bar.
- `audio-playback`: the no-station text, the play/stop control and previous/next favourite are stated for every theme.
- `station-browsing`: the station list, selecting a station, the current-station mark and the station entries are stated for every theme (only `Moderno` opens `Reproductor`).
- `favorites-browsing`: the heart, the Favoritos list, playing and removing a favourite are stated for every theme.

## Impact

- **App code:**
  - New `Resources/Themes/` palette dictionaries: `ValvulasBaquelita`, `ValvulasNogal`, `CaseteMetal`, `BoomboxCromado`, `MesaDjClub`, `Mp3Azul`, `RuedaBlanco`, `RuedaNegro`.
  - New layout dictionaries: `ValvulasLayout`, `CaseteLayout`, `BoomboxLayout`, `MesaDjLayout`, `Mp3Layout`, `RuedaLayout` (each `.xaml` + `.cs`).
  - New controls in `Views/Controls/`.
  - Six `ThemeCatalog` entries, with palette ids unique across the catalogue; a small `AppearanceService` change for the variant memory.
  - The licence file for the new art, the README, and `.csproj` items for the licence file.
- **Unchanged:**
  - The `Moderno` and `Tocadiscos` layouts and palettes.
  - The Tocadiscos controls, except that `VinylDisc` hands its spin logic to a shared helper without changing its behaviour.
  - Every service except `AppearanceService`.
  - The view models, the API, the database and `database/schema.sql`. No new packages.
- **Manual verification only**, on Windows and the Android emulator, in every theme.
