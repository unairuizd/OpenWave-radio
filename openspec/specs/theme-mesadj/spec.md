# Theme Mesadj Specification

## Purpose

Defines the `Mesa de DJ` theme: a club DJ booth in which stations are listed in a track browser, the player is one deck with a jog wheel, and favourites are eight performance pads, while every function of the app behaves exactly as in `Moderno`.

## Requirements

### Requirement: Mesa de DJ theme definition

`Mesa de DJ` SHALL be made of one palette, one layout and one theme registry entry, with display name `Mesa de DJ` and a single dark palette `Club`. It SHALL draw only one deck and no mixing controls that would do nothing. Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting Mesa de DJ

- **WHEN** the user selects `Mesa de DJ` in `Personalización`
- **THEN** every themed page changes to the DJ booth look at once
- **AND** no `Variante` choice is shown

### Requirement: Mesa de DJ colours

The palette SHALL use a near-black controller body, dark screen panels, light text, a cyan accent and orange hearts and current pad. The current browser row SHALL use a dark teal background. Every text SHALL reach 4.5:1 against its background.

#### Scenario: Measuring the palette

- **WHEN** every text and background pair actually used by the templates is measured
- **THEN** each pair is at least 4.5:1

### Requirement: Spanish booth labels

Every visible label of the theme SHALL be in Spanish, including `CARGAR`, `ATRÁS`, `SIGUIENTE` and `EN DIRECTO`. No English DJ term SHALL be shown.

#### Scenario: Reviewing the labels

- **WHEN** every page of `Mesa de DJ` is reviewed
- **THEN** no visible label is in English

### Requirement: Mesa de DJ header and bottom bar

Every themed page SHALL show a thin title bar with the title in upper case centred, the gear `Ajustes` on the right and a tag `EN DIRECTO` lit while a station is buffering or playing; in `Reproductor` it SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show a flat bottom bar with a cyan underline under the current section; `Reproductor` SHALL NOT show it.

#### Scenario: Live tag

- **WHEN** a station starts playing
- **THEN** the `EN DIRECTO` tag lights up on every page without blinking

### Requirement: Track browser

In `Inicio`, the stations SHALL be listed in a dense, scrollable browser with column headings (`NOMBRE`, `PAÍS`, `FORMATO`), one row per station with its logo, name, characteristics, heart and a `CARGAR` button. The headings SHALL be labels only. On narrow screens the columns SHALL collapse into one secondary line. Rows SHALL keep a touch height of at least 48 units on Android.

#### Scenario: Wide window

- **WHEN** `Inicio` is shown on a wide Windows window
- **THEN** country and format appear in their own columns

### Requirement: Loading a station

`CARGAR`, or pressing the row, SHALL play the station in place: a station that is not the current one SHALL be selected and played; for the current station it SHALL toggle between playing and stopped. The current row SHALL have the teal background and a cyan bar on its left edge.

#### Scenario: Loading a row

- **WHEN** the user activates `CARGAR` on a station that is not playing
- **THEN** it plays, `Inicio` stays on screen and its row is marked

### Requirement: Mesa de DJ Favoritos

`Favoritos` SHALL show the compact email and `Cerrar sesión` row, a list tab `Favoritas ({n})`, and the same browser listing only the favourites, with `CARGAR` and a heart that removes. The loading, error, empty and logged-out states SHALL replace the browser.

#### Scenario: Loading a favourite

- **WHEN** a logged-in user activates `CARGAR` on a favourite
- **THEN** it plays and `Favoritos` stays on screen

### Requirement: Deck

In `Reproductor`, `Mesa de DJ` SHALL show a jog wheel with the station logo in its centre, a decorative vertical level meter, the station name, characteristics, heart and status line with the error message and `Reintentar`. With no station selected, the centre SHALL be blank and the page SHALL show `Selecciona una emisora en Inicio o en tus favoritas`.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the jog wheel shows its logo or fallback in the centre

### Requirement: Deck transport

Under the deck, the theme SHALL show `ATRÁS` (`Favorita anterior`), a round play/pause button (`Reproducir` / `Detener`) and `SIGUIENTE` (`Favorita siguiente`).

#### Scenario: Pausing

- **WHEN** a station plays and the user presses the round button
- **THEN** playback stops and the button shows the play icon

### Requirement: Pads

At the bottom of `Reproductor`, the theme SHALL show eight pads in a 4×2 grid holding the favourites in the `Favoritos` order, each with its number and the station name ending with `…`; the current station's pad SHALL be orange. With more than eight favourites, arrows and a page indicator SHALL change the page. Pressing a pad SHALL select and play that favourite; a logged-out user SHALL be taken to `Iniciar sesión`.

#### Scenario: Ten favourites

- **WHEN** a logged-in user with ten favourites opens `Reproductor`
- **THEN** the pads show the first eight and `1/2`

### Requirement: Mesa de DJ motion

The jog wheel SHALL turn like the Tocadiscos record while a station is buffering or playing, speeding up and slowing down from its current angle. The current pad SHALL glow slowly while playing. The level meter SHALL move with decorative levels. A page shown while a station plays SHALL show the moving state at once.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion and a station plays
- **THEN** the jog wheel does not turn, the pad does not glow, and the play/pause button and `EN DIRECTO` still show the state

### Requirement: Mesa de DJ Ajustes and settings pages

`Ajustes` SHALL show its two entries as large pads with labels. `Personalización` and `Preferencias` SHALL show their content on a screen-like panel and keep every control usable.

#### Scenario: Opening a settings entry

- **WHEN** the user presses the `Preferencias` pad
- **THEN** `Preferencias` opens

### Requirement: Mesa de DJ accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Cargar y reproducir {nombre}` and `Pad {n}: {nombre}`. The jog wheel and level meter SHALL be hidden from screen readers. On Windows, Up/Down SHALL move between browser rows, Enter SHALL load, and arrow keys SHALL move between pads.

#### Scenario: Keyboard only

- **WHEN** a Windows user moves to a pad with the arrow keys and presses Enter
- **THEN** that favourite plays
