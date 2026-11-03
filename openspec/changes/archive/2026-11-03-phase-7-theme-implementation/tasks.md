# Tasks

Each group ends with the three builds with 0 errors and a short summary, then stops for review:
- `dotnet build src/OpenWave.Api`
- `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`
- `dotnet build src/OpenWave.App -f net10.0-android -p:AndroidSdkDirectory=$env:LOCALAPPDATA\Android\Sdk`

Close the running app before building on Windows. A theme is registered in `ThemeCatalog` only in its own group, so the app stays consistent after any group.

## 1. Prerequisites and variant memory

- [x] 1.1 Close Phase 6's unticked checks:
  - `MotionSettings` reads Windows "animations off" and Android animator scale 0;
  - with them on, the Tocadiscos tonearm moves instantly and the record does not turn;
  - the Phase 6 README checklist passes in Moderno and Tocadiscos.

  Report any failure and stop before continuing.
- [x] 1.2 Remove every `// TEMP-DIAG` line and `Services/DiagLog.cs`; verify with a search that `DiagLog` no longer appears and that the builds pass
- [x] 1.3 `AppearanceService` (design decision 1): `SetTheme` and `ReadStoredTheme` choose the stored id, then the palette with the same `BaseTheme` as the stored id's palette, then the first palette. Add a helper on `ThemeCatalog` that finds a palette by id across all themes. Verify by search that every palette id in `ThemeCatalog` is unique
- [x] 1.4 Verify on Windows with the preferences file, using a temporary second two-palette entry that reuses the Moderno dictionaries (removed afterwards):
  - Moderno Oscuro → Tocadiscos → Moderno restores Oscuro;
  - an unknown stored variant gives the theme's first palette;
  - `Restablecer` gives Moderno Claro.
- [x] 1.5 Run the three builds with 0 errors

## 2. Shared controls

- [x] 2.1 `Views/Controls/SpinDriver.cs` (design decision 6). Move `VinylDisc` onto it without changing its public properties. Verify in Tocadiscos that:
  - the record speeds up and slows down from its current angle;
  - it is already spinning when Reproductor opens during playback;
  - it stops when the page is hidden and with reduced motion.
- [x] 2.2 `Views/Controls/PagedSlots` (design decision 3). Verify on a temporary page with 0, 5, 8 and 11 items, logged in and out, in 5×1, 3×2, 4×2 and column-major 2×5, that:
  - the page indicator and the next/previous commands work, as does swipe;
  - it opens on the current item's page;
  - it clamps after removing items;
  - logged out it opens login;
  - empty slots use `EmptySlotTemplate`.
- [x] 2.3 `Views/Controls/CursorList` (design decision 4). Verify on a temporary page with 100 items that:
  - `MoveCommand`, `PageMoveCommand`, a touch, a second touch, Up/Down, Enter and the Windows wheel behave as specified;
  - `FocusOnReset` `First` and `KeepPosition` behave as specified, including removing the cursor item and the last item.

  If selection fights with the tap gesture, switch to the `IsCursor` fallback of the design's risks and report it.
- [x] 2.4 `Views/Controls/LevelMeter` (design decision 7). Verify on the temporary page that:
  - it moves only while `IsActive`;
  - it empties when stopped;
  - it stops when the page is hidden;
  - with reduced motion it shows a fixed level;
  - it is skipped by Narrator.
- [x] 2.5 Remove the temporary page; run the three builds with 0 errors

## 3. Radio de válvulas

- [x] 3.1 Create `ValvulasBaquelitaPalette` and `ValvulasNogalPalette` (+ `.cs`) with the brief's values. Verify:
  - by search, that each has exactly the 14 keys of `ModernoLightPalette`;
  - with the contrast script, that every pair the templates use is at least 4.5:1.

  Create `Resources/Images/Themes/LICENSE.txt` and ship it as `Licences\Themes-LICENSE.txt` in the `.csproj`; verify by build.
- [x] 3.2 Create `ValvulasLayout.xaml` (+ `.cs`) from copies of every Moderno key. Register `valvulas` "Radio de válvulas" with `baquelita` "Baquelita" (Light) and `nogal` "Nogal" (Dark). Verify that:
  - Personalización lists it, shows `Variante` with both palettes, and keeps light/dark from Moderno in both directions;
  - the audio is not interrupted.
- [x] 3.3 `Views/Controls/TuningDial` with its knob (design decision 5) and `Views/Controls/MagicEye`. Verify with 100 stations that:
  - swipe, the knob, the wheel and Left/Right move one station at a time;
  - Narrator reads the knob's name;
  - the eye follows `IsActive` and is instant with reduced motion.
- [x] 3.4 Replace the header, the bottom bar (wooden strip, ivory buttons), `ReproductorHeaderTemplate`, the station item and the Inicio and Favoritos templates (dial, panel with `Escuchar`/`Apagar`, radio log, every state). Verify on Windows and Android every scenario of `theme-valvulas` for Inicio and Favoritos, including:
  - a new search focusing the first station;
  - the current-station mark in both palettes.
- [x] 3.5 Replace `ReproductorTemplate` with:
  - the cabinet: grille, dial with needle, magic eye and power knob;
  - the status line with `Reintentar`;
  - the heart;
  - the no-station text;
  - the 5×1 preset keys with the arrow keys and `1/2`.

  Verify the play/stop, failure, logged-out, no-favourites and seven-favourites scenarios on Windows and Android.
- [x] 3.6 Replace `AjustesTemplate` and `SettingsPageTemplate` (ivory keys, cloth panel). Verify that every Personalización and Preferencias control is usable at `Pequeño` and `Enorme`
- [x] 3.7 Verify that the layout:
  - does not reference `ModernoLayout` and defines the eight keys;
  - has no literal colour, font family or font size (search);
  - passes keyboard-only use on Windows (Tab order, Left/Right on the knob, Enter).
- [x] 3.8 Update the README:
  - the theme list;
  - the two contrast tables;
  - the new controls and licence note;
  - the Radio de válvulas checklist items.

  Run the three builds with 0 errors.

## 4. Casete portátil

- [x] 4.1 Create `CaseteMetalPalette` (+ `.cs`); verify the 14 keys by search and every pair used by the contrast script
- [x] 4.2 Create `CaseteLayout` from Moderno copies; register `casete` "Casete portátil" with `metal` "Metal azul" (Dark). Verify that:
  - `Variante` is hidden;
  - the stored Moderno variant is kept.
- [x] 4.3 `Views/Controls/CassetteReels` on `SpinDriver`. Verify that:
  - the reels turn only while active and continue from their angle;
  - they are already turning when Reproductor opens during playback;
  - they stop when the page is hidden and with reduced motion.
- [x] 4.4 Replace the header, the bottom bar (silver keys `INICIO`/`REPRODUCTOR`/`FAVORITAS`), `ReproductorHeaderTemplate` and Inicio (shoebox on `CursorList`, pull-out insert with `Reproducir`/`Detener`, every state). Verify on Windows and Android:
  - one spine pulled out at a time;
  - play in place;
  - the orange mark;
  - a new search;
  - Up/Down/Enter.
- [x] 4.5 Replace Favoritos (J-card on `PagedSlots` 2×5 column-major, `CARA A`/`CARA B`, `Dar la vuelta`, remove buttons, account row, every state). Verify:
  - with 12 favourites, 10 + `1/2`;
  - removing the only favourite of the last page shows the previous page;
  - Left/Right changes side on Windows.
- [x] 4.6 Replace Reproductor with:
  - the cassette window;
  - the label, panel, status line and `Reintentar`;
  - the REW/PLAY/STOP/FF keys bound as in the spec;
  - side A on `PagedSlots` 1×5;
  - the logged-out card.

  Verify every `theme-casete` Reproductor scenario and the accessible names with Narrator.
- [x] 4.7 Replace Ajustes and the settings pages. Verify that the layout:
  - has no Moderno reference and no literal values;
  - every page works at `Pequeño` and `Enorme`.
- [x] 4.8 Update the README (theme list, contrast table, checklist); run the three builds with 0 errors

## 5. Boombox

- [x] 5.1 Create `BoomboxCromadoPalette` (+ `.cs`); verify the 14 keys and every pair used
- [x] 5.2 Create `BoomboxLayout` from Moderno copies; register `boombox` "Boombox" with `cromado` "Cromado" (Dark); verify in Personalización
- [x] 5.3 `Views/Controls/SpeakerCone`. Verify that:
  - it pulses only while active and settles on stop;
  - it is still with reduced motion;
  - it is hidden from Narrator.
- [x] 5.4 Replace the header (chrome trim, handle in Reproductor), the bottom bar (rubber buttons with LED) and Inicio and Favoritos:
  - the display between grilles, as columns on wide windows and strips on a phone;
  - rows that play in place, with the pink outline and `▶`;
  - the cassette door in Favoritos;
  - every state.

  Verify on Windows and Android.
- [x] 5.5 Replace Reproductor with:
  - the speakers;
  - the `LevelMeter` and display;
  - the status line and `Reintentar`;
  - the transport keys and heart;
  - `M1`–`M6` on `PagedSlots` 3×2 with `‹ 1/2 ›`.

  Verify every `theme-boombox` Reproductor scenario. On the Android emulator, verify that the page stays responsive while the speakers and meter move.
- [x] 5.6 Replace Ajustes and the settings pages. Verify:
  - no Moderno reference and no literal values;
  - keyboard use;
  - `Pequeño` and `Enorme`.
- [x] 5.7 Update the README (theme list, contrast table, checklist); run the three builds with 0 errors

## 6. Mesa de DJ

- [x] 6.1 Create `MesaDjClubPalette` (+ `.cs`); verify the 14 keys and every pair used
- [x] 6.2 Create `MesaDjLayout` from Moderno copies; register `mesadj` "Mesa de DJ" with `club` "Club" (Dark); verify in Personalización
- [x] 6.3 `Views/Controls/JogWheel` on `SpinDriver` (design decision 6). Verify the same motion rules as the Tocadiscos record
- [x] 6.4 Replace the header (title bar with `EN DIRECTO` tag), the bottom bar (cyan underline) and Inicio and Favoritos:
  - the browser with headings;
  - rows and `CARGAR` that play in place;
  - columns that collapse on a phone, with rows at least 48 units tall;
  - the `Favoritas ({n})` tab;
  - every state.

  Verify on Windows and Android, including that no visible label is in English.
- [x] 6.5 Replace Reproductor with:
  - the deck: jog wheel, vertical `LevelMeter`, name, characteristics, heart, status line and `Reintentar`;
  - `ATRÁS`, play/pause and `SIGUIENTE`;
  - the pads on `PagedSlots` 4×2.

  Verify every `theme-mesadj` Reproductor scenario with 10 favourites, and the arrow keys on the pads on Windows.
- [x] 6.6 Replace Ajustes and the settings pages. Verify:
  - no Moderno reference and no literal values;
  - `Pequeño` and `Enorme`.
- [x] 6.7 Update the README (theme list, contrast table, checklist); run the three builds with 0 errors

## 7. MP3

- [x] 7.1 Create `Mp3AzulPalette` (+ `.cs`); verify the 14 keys and every pair used, including text on the cursor row
- [x] 7.2 Create `Mp3Layout` from Moderno copies; register `mp3` "MP3" with `azul` "Azul" (Dark); verify in Personalización
- [x] 7.3 `Views/Controls/MarqueeLabel`. Verify that:
  - it scrolls only when the text is too wide, with pauses at the ends;
  - it stops when hidden;
  - Narrator reads the full name once;
  - with reduced motion it wraps to two lines.
- [x] 7.4 Replace the header (LCD status line with battery icon), the bottom bar (rubber buttons) and Inicio and Favoritos:
  - the LCD browser on `CursorList` with `/EMISORAS/` and `/FAVORITAS/`;
  - the counter;
  - `▲`/`OK`/`▼`;
  - the account row on the LCD;
  - every state.

  Verify the cursor, play-in-place, touch-twice and removal scenarios, and Up/Down/Enter on Windows.
- [x] 7.5 Replace Reproductor with:
  - the LCD now-playing screen: logo, marquee, characteristics, equaliser, status line and `Reintentar`;
  - the `◀◀`/`▶❙❙`/`▶▶`/heart pad.

  Verify every `theme-mp3` Reproductor scenario.
- [x] 7.6 Replace Ajustes (LCD menu) and the settings pages. Verify:
  - no Moderno reference and no literal values;
  - `Pequeño` and `Enorme`.
- [x] 7.7 Update the README (theme list, contrast table, checklist); run the three builds with 0 errors

## 8. Clásico de rueda

- [x] 8.1 Create `RuedaBlancoPalette` and `RuedaNegroPalette` (+ `.cs`); verify the 14 keys and every pair used, including text on the highlighted row in both palettes
- [x] 8.2 Create `RuedaLayout` from Moderno copies; register `rueda` "Clásico de rueda" with `blanco` "Blanco" (Light) and `negro` "Negro" (Dark). Verify the light/dark scenarios of `app-appearance`:
  - Moderno Oscuro → Radio de válvulas Nogal → Clásico de rueda Negro → Moderno Oscuro;
  - Clásico de rueda Blanco → Boombox → Moderno Claro.
- [x] 8.3 `Views/Controls/ClickWheel` (design decision 8). Verify on Windows and the Android emulator that:
  - a quarter turn moves about four rows;
  - the four buttons and the centre are real buttons with Spanish names;
  - the mouse wheel and the keys work.

  If the angle gesture is unreliable, apply the vertical-pan fallback and report it.
- [x] 8.4 Replace the header (screen title bar), the slim bottom bar and Inicio and Favoritos:
  - the device layout centred with a maximum width;
  - the Emisoras menu on `CursorList` with the search and filter rows;
  - the Favoritas menu with the account row;
  - every state;
  - `MENÚ` disabled in Inicio.

  Verify the cursor, centre, play/pause, `◀◀`/`▶▶`, touch-twice and removal scenarios.
- [x] 8.5 Replace Reproductor with:
  - the Reproduciendo screen: logo, name, characteristics, `En directo` bar, status line and `Reintentar`;
  - the wheel bound for Reproductor (`ToggleCommand`, previous/next favourite, heart on the centre, ring disabled);
  - `MENÚ` to Inicio;
  - the slim bar with `Reproductor` marked.

  Verify every `theme-rueda` Reproductor scenario.
- [x] 8.6 Replace Ajustes (screen menu) and the settings pages. Verify:
  - no Moderno reference and no literal values;
  - `Pequeño` and `Enorme` (the wheel keeps its size);
  - keyboard-only use (Escape, Space, Left/Right, Up/Down, Enter).
- [x] 8.7 Update the README (theme list, contrast tables, checklist); run the three builds with 0 errors

## 9. Full run

- [x] 9.1 On Windows and the Android emulator, with every page opened and a station playing, select the eight themes in turn. Verify that:
  - every page follows each time;
  - only one stream is heard;
  - the audio is never interrupted.
- [x] 9.2 In each new theme, run the "Functions in every new theme" scenario of `app-appearance`:
  - search, filters on and off;
  - play and stop, retry;
  - heart, previous/next or presets;
  - remove a favourite, log out.

  Then run the full README checklist. Report any item that fails.

## Workflow follow-up

- Archive the change with `/opsx:archive` after the developer has reviewed it.
