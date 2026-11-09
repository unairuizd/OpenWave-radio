# Theme Valvulas Specification

## Purpose

Defines the `Radio de válvulas` theme: a 1940s–50s valve table radio in which stations sit along a glass tuning dial, the player is the radio cabinet with a magic eye and a power knob, and favourites are ivory preset keys, while every function of the app behaves exactly as in `Moderno`.

## Requirements

### Requirement: Radio de válvulas theme definition

`Radio de válvulas` SHALL be made of two palettes, one layout and one theme registry entry, with display name `Radio de válvulas` and the palettes `Baquelita` (light) and `Nogal` (dark). Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting Radio de válvulas

- **WHEN** the user selects `Radio de válvulas` in `Personalización`
- **THEN** every themed page, the header, the bottom bar and the native controls change to the valve radio look at once
- **AND** the `Variante` choice offers `Baquelita` and `Nogal`

#### Scenario: Layout independence

- **WHEN** the `Radio de válvulas` layout is reviewed
- **THEN** it defines every template key itself and does not merge or reference another layout

### Requirement: Radio de válvulas colours

`Baquelita` SHALL use a cream bakelite cabinet, dark brown text, a bottle-green accent and a dark wooden strip for the header and bottom bar. `Nogal` SHALL use dark walnut, ivory text and a glowing green accent. Every text SHALL reach 4.5:1 against its background; text on the wooden strip SHALL use the bar colours, never the body text colour.

#### Scenario: Measuring the palettes

- **WHEN** every text and background pair actually used by the templates is measured in both palettes
- **THEN** each pair is at least 4.5:1

### Requirement: Radio de válvulas header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show a wooden header strip with the page title in upper case centred and the gear `Ajustes` on the right; in `Reproductor` it SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette; `Reproductor` SHALL NOT show it.

#### Scenario: Header in Reproductor

- **WHEN** the user opens `Reproductor` in `Radio de válvulas`
- **THEN** the header shows the house button, `REPRODUCTOR` centred and the gear
- **AND** no bottom bar is shown

#### Scenario: Bar in Inicio

- **WHEN** the user opens `Inicio` in `Radio de válvulas`
- **THEN** the bottom bar shows the three icons with `Inicio` marked, in the palette's colours

### Requirement: Tuning dial

In `Inicio`, `Radio de válvulas` SHALL show the stations along a horizontal glass dial with a printed scale and a fixed needle in the middle. The focused station SHALL sit under the needle in a lit window, with about two neighbours visible on each side. A caption SHALL show `emisora {n} de {total}`. Moving the focus SHALL go one station at a time.

#### Scenario: Turning the dial

- **WHEN** the user swipes the dial, turns the tuning knob, turns the mouse wheel over the dial or presses Left or Right on Windows
- **THEN** the next or previous station moves under the needle
- **AND** the information panel shows that station

#### Scenario: New search

- **WHEN** the user searches or the applied preferences change
- **THEN** the dial focuses the first station

### Requirement: Tuning knob

Next to the dial, `Radio de válvulas` SHALL show a round tuning knob that moves the focus one station per step and turns with each step. It SHALL be the dial's focusable control, with the accessible name `Sintonizar, emisora {n} de {total}: {nombre}`.

#### Scenario: Knob with a screen reader

- **WHEN** a screen reader focuses the knob while the third of 48 stations is focused
- **THEN** it announces `Sintonizar, emisora 3 de 48:` followed by the station name

### Requirement: Valve radio information panel

Under the dial, on the cloth speaker grille, `Inicio` and `Favoritos` SHALL show an information panel with the focused station's name, characteristics, heart, its play state when it is the current station, and a button `Escuchar` that plays the station in place. Pressing the focused station on the dial SHALL do the same. While that station plays, the button SHALL read `Apagar` and stop it.

#### Scenario: Playing from the dial

- **WHEN** the user activates `Escuchar` for a station that is not playing
- **THEN** that station plays, `Inicio` stays on screen and the panel shows `Cargando…` and then `Reproduciendo`

#### Scenario: Stopping from the panel

- **WHEN** the focused station is playing and the user activates `Apagar`
- **THEN** playback stops and the page stays on screen

### Requirement: Current station on the dial

The current station's label on the dial and its entry in `Favoritos` SHALL be marked with the lit window background (`Baquelita`) or an outline (`Nogal`). The mark SHALL move when another station becomes current.

#### Scenario: Marking the current station

- **WHEN** a station becomes the current station
- **THEN** its dial label is marked and no other label is

### Requirement: Radio log in Favoritos

`Favoritos` SHALL show the compact email and `Cerrar sesión` row, then the favourites as a vertical list of dial labels on the cloth grille, each with its logo, name, characteristics and heart. Pressing a label SHALL play it in place; its heart SHALL remove it. The loading, error, empty and logged-out states SHALL replace the list, centred, with the same texts and buttons as in `Moderno`.

#### Scenario: Playing from the log

- **WHEN** a logged-in user presses a favourite's label
- **THEN** it plays and `Favoritos` stays on screen

### Requirement: Radio cabinet

In `Reproductor`, `Radio de válvulas` SHALL show the cabinet: a cloth grille, the dial with only the current station under the needle, the magic eye, the station logo, characteristics, heart, and the status line with the error message and `Reintentar` when playback fails. With no station selected, the dial SHALL be empty and the page SHALL show `Selecciona una emisora en Inicio o en tus favoritas`.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the dial shows its name under the needle and the panel shows its logo or fallback and characteristics

### Requirement: Power knob

The power knob SHALL be the play/stop control of `Radio de válvulas`: its accessible name SHALL be `Reproducir` when stopped or in error and `Detener` while buffering or playing, and pressing it SHALL start or stop playback. It SHALL be disabled when no station is selected.

#### Scenario: Turning the radio on

- **WHEN** a station is stopped and the user presses the power knob
- **THEN** the station plays, the dial window lights up and the magic eye closes

### Requirement: Preset keys

At the bottom of `Reproductor`, `Radio de válvulas` SHALL show five ivory preset keys holding the user's favourites in the `Favoritos` order, each with its number and the station name on one line ending with `…`. The current station's key SHALL be pressed down. With more than five favourites, two arrow keys and a page indicator such as `1/2` SHALL change the page. Pressing a key SHALL select and play that favourite.

#### Scenario: Seven favourites

- **WHEN** a logged-in user with seven favourites opens `Reproductor`
- **THEN** the keys show the first five favourites and the indicator `1/2`
- **AND** the next arrow shows the other two with three empty keys

#### Scenario: Logged out presets

- **WHEN** a logged-out user presses any preset key
- **THEN** `Iniciar sesión` opens and nothing is added to the favourites

#### Scenario: No favourites

- **WHEN** a logged-in user without favourites opens `Reproductor`
- **THEN** the keys are blank and the page shows `Aún no tienes emisoras favoritas.`

### Requirement: Radio de válvulas motion

When playback starts, the dial window SHALL brighten over about 1.5 s and the magic eye SHALL close over about 0.8 s; when it stops, both SHALL return in about 0.6 s. The needle SHALL move to a new focus in about 250 ms. A page shown while a station plays SHALL show the lit state at once.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion and the user turns the radio on
- **THEN** the dial is lit and the magic eye closed at once

### Requirement: Radio de válvulas accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Escuchar`, `Apagar`, `Presintonía {n}: {nombre}`, `Página anterior de presintonías` and `Página siguiente de presintonías`. The grille, scale numbers and magic eye SHALL be hidden from screen readers. On Windows, Left/Right on the knob SHALL move the focus, Enter or Space SHALL play, and Tab SHALL follow the visual order.

#### Scenario: Keyboard only

- **WHEN** a Windows user tabs to the knob, presses Right twice and then Enter
- **THEN** the station two places further plays
