# Spec Delta

## Purpose

Defines the `Boombox` theme: a 1980s street boombox in which stations are listed on a tuner display between two speakers, the player shows pulsing speakers and a level meter, and favourites are the `M1`–`M6` memories, while every function of the app behaves exactly as in `Moderno`.

## ADDED Requirements

### Requirement: Boombox theme definition

`Boombox` SHALL be made of one palette, one layout and one theme registry entry, with display name `Boombox` and a single dark palette `Cromado`. Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting Boombox

- **WHEN** the user selects `Boombox` in `Personalización`
- **THEN** every themed page changes to the boombox look at once
- **AND** no `Variante` choice is shown

### Requirement: Boombox colours

The palette SHALL use a black plastic body, gunmetal panels, chrome trim, a hot pink accent, cyan display text and yellow hearts. Every text SHALL reach 4.5:1 against its background.

#### Scenario: Measuring the palette

- **WHEN** every text and background pair actually used by the templates is measured
- **THEN** each pair is at least 4.5:1

### Requirement: Boombox header and bottom bar

Every themed page SHALL show the title in upper case centred under a chrome trim line and the gear `Ajustes` on the right; in `Reproductor` the header SHALL also show a house button `Inicio` on the left and a decorative carrying handle above it. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show a bottom bar of three rubber buttons with labels, the current one with a lit cyan LED; `Reproductor` SHALL NOT show it.

#### Scenario: Marked button

- **WHEN** the user opens `Inicio`
- **THEN** the `INICIO` button's LED is lit and the other two are not

### Requirement: Tuner display

In `Inicio`, the stations SHALL be listed on a dark display between two speaker grilles, one row per station with its logo, name, characteristics and heart, under a caption `TUNING` with the number of stations. On wide windows the speakers SHALL be beside the display; on narrow screens they SHALL be short strips above and below it. The list SHALL scroll and show every station.

#### Scenario: Narrow screen

- **WHEN** `Inicio` is shown on an Android phone in portrait
- **THEN** the speaker grilles are above and below the display and every row is fully visible

### Requirement: Playing from the display

Pressing a row SHALL play its station in place: a station that is not the current one SHALL be selected and played; pressing the current station's row SHALL toggle between playing and stopped. The current row SHALL be marked with a pink outline and a `▶` before its name.

#### Scenario: Playing a row

- **WHEN** the user presses the row of a station that is not playing
- **THEN** it plays, `Inicio` stays on screen and its row gets the pink outline

### Requirement: Boombox Favoritos

`Favoritos` SHALL show the compact email and `Cerrar sesión` row, a closed cassette door labelled `MIS FAVORITAS`, and the same display listing only the favourites, where pressing a row plays it in place and the heart removes it. The loading, error, empty and logged-out states SHALL replace the display.

#### Scenario: Removing from the display

- **WHEN** the user activates a favourite's filled heart
- **THEN** the row disappears without confirmation

### Requirement: Boombox player

In `Reproductor`, `Boombox` SHALL show two large speakers framing a level meter, and a display with the station logo, its name in large display style, characteristics and the status line with the error message and `Reintentar`. With no station selected, the display SHALL show `Selecciona una emisora en Inicio o en tus favoritas` and the speakers SHALL stay still.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the display shows its logo or fallback, name and characteristics

### Requirement: Boombox transport keys

Under the display, `Reproductor` SHALL show the keys `◀◀` (`Favorita anterior`), the pink play/stop key (`Reproducir` / `Detener`, labelled `▶ PLAY` or `■ STOP`), `▶▶` (`Favorita siguiente`) and the heart.

#### Scenario: Next favourite from the keys

- **WHEN** the user presses `▶▶`
- **THEN** the next favourite plays, as with `Favorita siguiente` in `Moderno`

### Requirement: Boombox memories

At the bottom of `Reproductor`, `Boombox` SHALL show six chrome memory buttons `M1`–`M6` holding the favourites in the `Favoritos` order, each with the station name on one line ending with `…`; the current station's button SHALL be lit cyan. With more than six favourites, arrows and a page indicator SHALL change the page. A logged-out user pressing the memories SHALL be taken to `Iniciar sesión`.

#### Scenario: Eight favourites

- **WHEN** a logged-in user with eight favourites opens `Reproductor`
- **THEN** `M1`–`M6` show the first six and the indicator shows `1/2`

### Requirement: Boombox motion

While a station is buffering or playing, both speaker cones SHALL pulse gently in step and the level meter SHALL move with decorative levels that do not come from the audio. When playback stops, the speakers SHALL settle and the meter SHALL fall to empty in about 0.4 s. A page shown while a station plays SHALL show the moving state at once.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion and a station plays
- **THEN** the speakers are still and the meter shows a fixed middle level, and it shows empty when stopped

### Requirement: Boombox Ajustes and settings pages

`Ajustes` SHALL show its two entries as wide rubber buttons on gunmetal. `Personalización` and `Preferencias` SHALL show their content on a gunmetal panel with a chrome border and keep every control usable.

#### Scenario: Opening a settings entry

- **WHEN** the user presses the `Personalización` button
- **THEN** `Personalización` opens

### Requirement: Boombox accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Memoria {n}: {nombre}`. The speakers, handle, antenna, grilles and level meter SHALL be hidden from screen readers. On Windows, Up/Down SHALL move between display rows and Enter SHALL play, and arrow keys SHALL move between memories.

#### Scenario: Keyboard only

- **WHEN** a Windows user moves to the third row with Down and presses Enter
- **THEN** that station plays
