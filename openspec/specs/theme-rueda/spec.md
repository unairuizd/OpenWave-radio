# Theme Rueda Specification

## Purpose

Defines the `Clásico de rueda` theme: a mid-2000s pocket player with a screen of menus and a round click wheel, in which stations and favourites are menus moved through with the wheel and the player is a "now playing" screen, while every function of the app behaves exactly as in `Moderno`.

## Requirements

### Requirement: Clásico de rueda theme definition

`Clásico de rueda` SHALL be made of two palettes, one layout and one theme registry entry, with display name `Clásico de rueda` and the palettes `Blanco` (light) and `Negro` (dark). It SHALL show no brand name or logo. Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting Clásico de rueda

- **WHEN** the user selects `Clásico de rueda` in `Personalización`
- **THEN** every themed page changes to the wheel player look at once
- **AND** the `Variante` choice offers `Blanco` and `Negro`

### Requirement: Clásico de rueda colours

`Blanco` SHALL use a white body, a light grey-blue screen, near-black text and a blue highlighted row with white text. `Negro` SHALL use a black body, a dark screen, near-white text and a dark blue highlighted row. Every text SHALL reach 4.5:1 against its background, including text on the highlighted row.

#### Scenario: Measuring the palettes

- **WHEN** every text and background pair actually used by the templates is measured in both palettes
- **THEN** each pair is at least 4.5:1

### Requirement: Device layout

`Inicio`, `Reproductor` and `Favoritos` SHALL each show the screen on top (about the upper half), the click wheel under it, and a slim bottom bar under the wheel. On wide windows the device SHALL be centred with a maximum width and keep its proportions. The screen content SHALL scroll when it does not fit; the wheel SHALL keep its size at every text size.

#### Scenario: Enorme on a phone

- **WHEN** `Enorme` is selected on an Android phone
- **THEN** the menu rows truncate or scroll and the wheel and bar keep their size

### Requirement: Clásico de rueda header and bottom bar

The screen's title bar SHALL show the page title in upper case centred, a play-state glyph on the left and the gear `Ajustes` on the right. Every themed page, including `Reproductor`, SHALL show the slim bottom bar with `Inicio`, `Reproductor` and `Favoritos`, the current one marked.

#### Scenario: Bar in Reproductor

- **WHEN** the user opens `Reproductor` in `Clásico de rueda`
- **THEN** the slim bar is shown with `Reproductor` marked

### Requirement: Click wheel

The wheel SHALL have four buttons at its edge, `MENÚ` at the top, `◀◀` on the left, `▶▶` on the right and play/pause at the bottom, and a centre button. Sliding a finger around the ring SHALL move the menu cursor one row about every 24°. `MENÚ` SHALL open `Inicio` from `Reproductor` and `Favoritos` and SHALL be disabled in `Inicio`.

#### Scenario: Turning the wheel

- **WHEN** the user slides a finger clockwise a quarter of a turn on the ring in `Inicio`
- **THEN** the cursor moves about four rows down

#### Scenario: MENÚ in Reproductor

- **WHEN** the user presses `MENÚ` in `Reproductor`
- **THEN** `Inicio` is shown

### Requirement: Emisoras menu

In `Inicio`, the screen SHALL show the search row (input, `Buscar` and `Limpiar búsqueda`) and the active filters bar when it applies as the first rows, then one menu row per station with its name, heart and `›`. One row SHALL be highlighted as the cursor. A new search SHALL put the cursor on the first station; returning to the page SHALL keep it. The loading, error, empty and "no results with your preferences" states SHALL replace the station rows.

#### Scenario: New search

- **WHEN** the user searches `rock`
- **THEN** the menu lists the results with the cursor on the first one

### Requirement: Playing from a menu

In `Inicio` and `Favoritos`, the centre button SHALL play the cursor's station in place, and on the current station SHALL toggle between playing and stopped. Play/pause SHALL do the same for the current station. `◀◀` and `▶▶` SHALL move the cursor one screen of rows. Touching a row SHALL move the cursor; touching the cursor row again SHALL act as the centre button.

#### Scenario: Playing with the centre button

- **WHEN** the cursor is on a station that is not playing and the user presses the centre button
- **THEN** it plays and the page stays on screen

### Requirement: Current station in a menu

The current station's row SHALL show a play-state glyph before its name, in addition to the cursor highlight when the cursor is on it. The mark SHALL move when another station becomes current.

#### Scenario: Marking the current station

- **WHEN** a station becomes the current station and the cursor is elsewhere
- **THEN** its row shows the glyph and is not highlighted

### Requirement: Favoritas menu

`Favoritos` SHALL show the account row (email and `Cerrar sesión`) as the first row, then one row per favourite with the same cursor behaviour; the heart on a row SHALL remove it. After removing the cursor's favourite the cursor SHALL move to the next row, or to the previous one when it was the last. Logged out, the screen SHALL show `Inicia sesión para ver tus favoritas.` and an `Iniciar sesión` row.

#### Scenario: Logging out from the menu

- **WHEN** the user activates `Cerrar sesión` on the account row
- **THEN** the session ends as in `Moderno` and the screen shows the logged-out text

### Requirement: Reproduciendo screen

In `Reproductor`, the screen SHALL show the station logo large and square, its name, its characteristics, a full bar labelled `En directo`, and the status line with the error message and `Reintentar`. With no station selected, it SHALL show `Selecciona una emisora en Inicio o en tus favoritas`.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the screen shows its logo or fallback, name, characteristics and `En directo`

### Requirement: Wheel in Reproductor

In `Reproductor`, play/pause SHALL be the play/stop control (`Reproducir` / `Detener`), `◀◀` and `▶▶` SHALL be `Favorita anterior` and `Favorita siguiente`, and the centre button SHALL be the heart (`Añadir a favoritas` / `Quitar de favoritas`). Sliding around the ring SHALL do nothing on this page, and the ring SHALL NOT be focusable.

#### Scenario: Heart from the centre

- **WHEN** a logged-in user presses the centre button on a station that is not a favourite
- **THEN** it is added to the favourites and the centre shows a filled heart

### Requirement: Clásico de rueda motion

Moving the cursor SHALL change the highlighted row at once and scroll the list in about 120 ms to keep it visible. The wheel buttons SHALL give a short press feedback of about 80 ms. No sound SHALL be played.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion
- **THEN** the list jumps to keep the cursor visible and the buttons give no press animation

### Requirement: Clásico de rueda Ajustes and settings pages

`Ajustes` SHALL show a screen menu with `Preferencias` and `Personalización`, each a control of its own. `Personalización` and `Preferencias` SHALL show their content in the screen area, scrolling, and keep every control usable. These pages SHALL keep the header and the slim bottom bar.

#### Scenario: Opening a settings entry

- **WHEN** the user touches `Preferencias` in the `Ajustes` menu
- **THEN** `Preferencias` opens

### Requirement: Clásico de rueda accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Menú`, `Seleccionar`, `Reproducir o pausar`, `Anterior`, `Siguiente` and `Rueda: desliza en círculo para moverte por la lista`. Every row SHALL be reachable by touch or by screen reader without the circular gesture. On Windows, the mouse wheel and Up/Down SHALL move the cursor, Enter SHALL act as the centre button, Space as play/pause, Left/Right as `◀◀`/`▶▶`, and Escape as `MENÚ`.

#### Scenario: Keyboard only

- **WHEN** a Windows user in `Inicio` presses Down three times and Enter
- **THEN** the station three rows below the first plays
