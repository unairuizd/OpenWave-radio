# Spec Delta

## Purpose

Defines the `Casete portátil` theme: a 1980s portable cassette player in which stations are cassette spines in a shoebox, the player is the cassette window with turning reels and mechanical keys, and favourites are a hand-written mixtape card, while every function of the app behaves exactly as in `Moderno`.

## ADDED Requirements

### Requirement: Casete portátil theme definition

`Casete portátil` SHALL be made of one palette, one layout and one theme registry entry, with display name `Casete portátil` and a single dark palette `Metal azul`. It SHALL show no brand name or logo. Its layout SHALL provide every page template, the header, the bottom bar and the station item, and SHALL NOT merge or reference another theme's layout.

#### Scenario: Selecting Casete portátil

- **WHEN** the user selects `Casete portátil` in `Personalización`
- **THEN** every themed page changes to the cassette player look at once
- **AND** no `Variante` choice is shown

### Requirement: Casete portátil colours

The palette SHALL use a blue metal body, a darker cassette window, light text, an orange accent and brushed silver for keys and the J-card paper. Every text SHALL reach 4.5:1 against its background; text on the silver J-card SHALL be dark, and orange SHALL NOT be used for text on the lighter blue panels.

#### Scenario: Measuring the palette

- **WHEN** every text and background pair actually used by the templates is measured
- **THEN** each pair is at least 4.5:1

### Requirement: Casete portátil header and bottom bar

Every themed page SHALL show the title in upper case centred and the gear `Ajustes` on the right; in `Reproductor` the header SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show a bottom bar of three silver mechanical keys `INICIO`, `REPRODUCTOR` and `FAVORITAS`, the current one pushed in; `Reproductor` SHALL NOT show it.

#### Scenario: Marked key

- **WHEN** the user opens `Favoritos`
- **THEN** the `FAVORITAS` key is pushed in and screen readers announce it as selected

### Requirement: Shoebox of cassettes

In `Inicio`, the stations SHALL be shown as a vertical, scrollable list of cassette spines inside a box, each with its logo, its name on the label band and its heart. Pressing a spine SHALL pull it half out and show an insert under it with the characteristics, the play state and a `Reproducir` button; only one spine SHALL be pulled out at a time. A new search SHALL scroll back to the first spine.

#### Scenario: Pulling out a spine

- **WHEN** the user presses a spine
- **THEN** it is pulled out with its insert and any other pulled-out spine goes back

### Requirement: Playing a cassette

The insert's `Reproducir` button SHALL play the station in place: a station that is not the current one SHALL be selected and played; for the current station it SHALL read `Detener` and stop it.

#### Scenario: Playing from the shoebox

- **WHEN** the user activates `Reproducir` in the insert of a station that is not playing
- **THEN** the station plays and `Inicio` stays on screen

### Requirement: Current cassette

The current station's spine, and its line on the J-card, SHALL be marked with the orange outline or orange text. The mark SHALL move when another station becomes current.

#### Scenario: Marking the current station

- **WHEN** a station becomes the current station
- **THEN** only its spine has the orange outline

### Requirement: Mixtape card in Favoritos

`Favoritos` SHALL show the compact email and `Cerrar sesión` row, then a silver J-card titled `Mis favoritas` with two columns, `CARA A` and `CARA B`, of five numbered favourites each. Pressing a line SHALL play it in place; each line SHALL have a remove button. With more than ten favourites, `Dar la vuelta` SHALL show the next ten, with a page indicator. The loading, error, empty and logged-out states SHALL replace the card.

#### Scenario: Twelve favourites

- **WHEN** a logged-in user with twelve favourites opens `Favoritos`
- **THEN** the card shows ten favourites and `1/2`
- **AND** `Dar la vuelta` shows the other two

#### Scenario: Removing the last favourite of a page

- **WHEN** the user removes the only favourite on the last page
- **THEN** the card shows the previous page

### Requirement: Cassette window

In `Reproductor`, `Casete portátil` SHALL show the cassette window with the tape, its label with the station name, and the station logo, characteristics, heart and status line (with the error message and `Reintentar`). With no station selected, the label SHALL be blank and the page SHALL show `Selecciona una emisora en Inicio o en tus favoritas`.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the tape label shows its name and the panel its characteristics

### Requirement: Transport keys

Along the top edge of `Reproductor`, the theme SHALL show four keys: `REW` (`Favorita anterior`), `PLAY` (`Reproducir`), `STOP` (`Detener`, enabled only while buffering or playing) and `FF` (`Favorita siguiente`). `PLAY` SHALL stay pushed in while a station plays.

#### Scenario: Stopping with STOP

- **WHEN** a station plays and the user presses `STOP`
- **THEN** playback stops, `PLAY` pops up and `STOP` becomes disabled

### Requirement: Side A in Reproductor

Under the cassette window, `Reproductor` SHALL show one side of the J-card with five favourites and `Dar la vuelta`. Pressing a line SHALL select and play that favourite. A logged-out user SHALL see `Inicia sesión para ver tus favoritas`, and pressing the card SHALL open `Iniciar sesión`.

#### Scenario: Playing from side A

- **WHEN** the user presses the second line
- **THEN** that favourite plays and its line is marked

### Requirement: Casete portátil motion

While a station is buffering or playing, both reels SHALL turn at about half a turn per second, speeding up over about 0.8 s and slowing down over about 1 s from their current angle. `Dar la vuelta` SHALL flip the card in about 0.3 s. A page shown while a station plays SHALL show the reels already turning.

#### Scenario: Reduced motion

- **WHEN** the operating system asks for reduced motion
- **THEN** the reels do not turn, the card changes side at once, and `PLAY` still shows the state

### Requirement: Casete portátil Ajustes and settings pages

`Ajustes` SHALL show its two entries as big silver keys with stamped labels. `Personalización` and `Preferencias` SHALL show their content on a rounded brushed panel and keep every control usable.

#### Scenario: Opening a settings entry

- **WHEN** the user presses the `Preferencias` key
- **THEN** `Preferencias` opens

### Requirement: Casete portátil accessibility and keyboard

Every control SHALL have a Spanish accessible name, including `Dar la vuelta a la cinta`, `Cara A, pista {n}: {nombre}` and `Quitar {nombre} de favoritas`. The reels and tape window SHALL be hidden from screen readers. On Windows, Up/Down SHALL move between spines, Enter SHALL pull out a spine or play it, and Left/Right on the J-card SHALL change side.

#### Scenario: Keyboard only

- **WHEN** a Windows user moves to a spine with Down and presses Enter twice
- **THEN** the spine is pulled out and then its station plays
