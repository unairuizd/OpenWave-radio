# Theme Mp3 Specification

## Purpose

Defines the `MP3` theme: a mid-2000s MP3 stick with a blue back-lit LCD in which stations are a folder browsed with a cursor, the player shows a scrolling station name and a small equaliser, and favourites are another folder, while every function of the app behaves exactly as in `Moderno`.

## Requirements

### Requirement: MP3 theme definition

`MP3` SHALL be made of one palette, one layout and one theme registry entry, with display name `MP3` and a single dark palette `Azul`. Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting MP3

- **WHEN** the user selects `MP3` in `Personalización`
- **THEN** every themed page changes to the MP3 player look at once
- **AND** no `Variante` choice is shown

### Requirement: MP3 colours

The palette SHALL use a dark plastic body, a blue back-lit LCD with light blue text, an inverted cursor row, a light blue accent and mint hearts. Every text SHALL reach 4.5:1 against its background, including text on the cursor row.

#### Scenario: Measuring the palette

- **WHEN** every text and background pair actually used by the templates is measured
- **THEN** each pair is at least 4.5:1

### Requirement: MP3 header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show the title in upper case centred on the LCD status line and the gear `Ajustes` on the right; in `Reproductor` it SHALL also show a house button `Inicio` on the left. The status line SHALL also show a decorative battery icon. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette, the current section marked; `Reproductor` SHALL NOT show it.

#### Scenario: Header in Inicio

- **WHEN** the user opens `Inicio` in `MP3`
- **THEN** the LCD status line shows `INICIO` centred and the gear on the right
- **AND** the bottom bar shows the three icons with `Inicio` marked

### Requirement: LCD browser

In `Inicio`, the stations SHALL be shown on the LCD as a list under the path `/EMISORAS/`, one line per station with its name and heart, and a counter `{n}/{total}` at the bottom. A cursor SHALL mark one line, show `▶` before it and show its characteristics under it. A new search SHALL put the cursor on the first line; returning to the page SHALL keep it.

#### Scenario: Moving the cursor

- **WHEN** the user presses `▼`
- **THEN** the cursor moves to the next line and the counter changes

### Requirement: Playing from the LCD

Under the LCD, `▲`, `OK` and `▼` SHALL move the cursor and play the station under it in place. Touching a line SHALL move the cursor there; touching the cursor line again SHALL act as `OK`. `OK` on the current station SHALL toggle between playing and stopped. The current station's line SHALL show its play state.

#### Scenario: Playing with OK

- **WHEN** the cursor is on a station that is not playing and the user presses `OK`
- **THEN** it plays and `Inicio` stays on screen

#### Scenario: Touching a line twice

- **WHEN** the user touches a line, then touches it again
- **THEN** the first touch moves the cursor and the second plays the station

### Requirement: MP3 Favoritos

`Favoritos` SHALL show the compact email and `Cerrar sesión` row on the LCD, then the favourites under the path `/FAVORITAS/` with the same cursor and buttons; the cursor line's heart SHALL remove it. After removing the cursor line the cursor SHALL move to the next line, or to the previous one when it was the last. The loading, error, empty and logged-out states SHALL replace the list on the LCD.

#### Scenario: Removing the cursor line

- **WHEN** the user removes the favourite under the cursor and it was not the last
- **THEN** the cursor moves to the next favourite

### Requirement: Now-playing screen

In `Reproductor`, the LCD SHALL show the station logo in a framed square, its name, its characteristics, a decorative equaliser and the status line with the error message and `Reintentar`. A name wider than the screen SHALL scroll sideways. With no station selected, the LCD SHALL show `Selecciona una emisora en Inicio o en tus favoritas`.

#### Scenario: Long name

- **WHEN** the current station's name does not fit on the LCD
- **THEN** it scrolls sideways, pausing at each end

### Requirement: MP3 button pad

Under the LCD, `Reproductor` SHALL show `◀◀` (`Favorita anterior`), `▶❙❙` (`Reproducir` / `Detener`), `▶▶` (`Favorita siguiente`) and the heart. There SHALL be no other favourites control on this page.

#### Scenario: Play and stop

- **WHEN** a station is stopped and the user presses `▶❙❙`
- **THEN** it plays and the button's name becomes `Detener`

### Requirement: MP3 motion

The name SHALL scroll at about 40 units per second, pausing about 1 s at each end, only while the page is visible. The equaliser SHALL move with decorative levels while a station is buffering or playing, and show empty when stopped.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion
- **THEN** the name does not scroll and wraps to two lines ending with `…`, and the equaliser shows a fixed level while playing

### Requirement: MP3 accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Subir`, `Bajar` and `Aceptar: reproducir {nombre}`. The scrolling name SHALL be read once as the full name, and the battery icon and equaliser SHALL be hidden from screen readers. On Windows, Up/Down SHALL move the cursor and Enter SHALL act as `OK`.

#### Scenario: Keyboard only

- **WHEN** a Windows user presses Down twice and Enter in `Inicio`
- **THEN** the station two lines below plays
