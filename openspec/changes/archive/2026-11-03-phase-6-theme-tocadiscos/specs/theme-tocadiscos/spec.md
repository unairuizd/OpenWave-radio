# Spec Delta

## Purpose

Defines the `Tocadiscos` theme: a 1960s–70s record player look in which stations are record sleeves, the player is a spinning vinyl with a tonearm and favourites live in a record crate, while every function of the app behaves exactly as in `Moderno`.

## ADDED Requirements

### Requirement: Tocadiscos theme definition

`Tocadiscos` SHALL be made of one palette, one layout and one theme registry entry, with display name `Tocadiscos` and a single dark palette. Its layout SHALL provide every page template, the header, the bottom bar and the station item that `Moderno` provides, and SHALL work without the `Moderno` layout being present.

#### Scenario: Selecting Tocadiscos

- **WHEN** the user selects `Tocadiscos` in `Personalización`
- **THEN** every themed page, the header, the bottom bar and the native controls change to the record player look at once
- **AND** no `Variante` choice is shown

#### Scenario: Layout independence

- **WHEN** the `Tocadiscos` layout is reviewed
- **THEN** it defines every template key itself and does not merge or reference the `Moderno` layout

### Requirement: Tocadiscos colours

The `Tocadiscos` palette SHALL use dark walnut wood for page backgrounds, cream paper for record sleeves and settings cards, a brass accent, vinyl black for records and warm light text. Every text SHALL have a contrast of at least 4.5:1 with the background it is drawn on, including text over wood and text on cream cards.

#### Scenario: Measuring the palette

- **WHEN** every text and background pair of the `Tocadiscos` palette is measured
- **THEN** each pair is at least 4.5:1

### Requirement: Tocadiscos header

Every themed page in `Tocadiscos` SHALL show the page title in upper case centred on the full width of the page and the gear `Ajustes` on the right, behaving as in `Moderno`. In `Reproductor` the header SHALL also show a house button `Inicio` on the left that opens `Inicio`.

#### Scenario: Header in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos`
- **THEN** the header shows the house button on the left, `REPRODUCTOR` centred and the gear on the right
- **WHEN** the user activates the house button
- **THEN** `Inicio` is shown

### Requirement: Tocadiscos bottom bar

`Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` in `Tocadiscos` SHALL show a wooden bottom bar with brass icons `Inicio`, `Reproductor` and `Favoritos`, marking the current section as in `Moderno`. `Reproductor` SHALL NOT show the bottom bar; the record crate takes its place.

#### Scenario: Bar in Favoritos

- **WHEN** the user opens `Favoritos` in `Tocadiscos`
- **THEN** the bottom bar shows `Inicio`, `Reproductor` and `Favoritos` with `Favoritos` marked

#### Scenario: No bar in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos`
- **THEN** no bottom bar is shown and the record crate is at the bottom of the page

### Requirement: Sleeve stack

In `Tocadiscos`, `Inicio` and `Favoritos` SHALL show their stations as a vertical stack of square record sleeves with the station logo (or the fallback image). The focused station SHALL be shown big at the top; the next two SHALL be visible below it, smaller (about 80% and 65%), partly overlapping and dimmed. Moving through the stack SHALL go one station at a time.

#### Scenario: Scrolling the stack

- **WHEN** the user swipes up, turns the mouse wheel or presses the Down key on Windows
- **THEN** the next station becomes the focused big sleeve
- **AND** the information panel shows that station

#### Scenario: Pressing a smaller sleeve

- **WHEN** the user presses one of the smaller sleeves
- **THEN** that station becomes the focused sleeve
- **AND** playback does not change

### Requirement: Playing from a sleeve

Pressing the focused sleeve SHALL play its station without leaving the page: a station that is not the current one SHALL be selected and played, stopping the previous one; the current station SHALL toggle between playing and stopped.

#### Scenario: Playing a new station from Inicio

- **WHEN** the user presses the big sleeve of a station that is not playing
- **THEN** that station starts playing and the app stays in `Inicio`

#### Scenario: Stopping the current station

- **WHEN** the user presses the big sleeve of the station that is playing
- **THEN** playback stops and the app stays on the page

### Requirement: Current station on its sleeve

The sleeve of the current station SHALL show a vinyl disc peeking out of its edge and an accent outline. The marker SHALL be static and SHALL move when another station becomes current.

#### Scenario: Marking the current station

- **WHEN** a station becomes the current station
- **THEN** its sleeve shows the peeking disc and the outline
- **AND** no other sleeve does

### Requirement: Information panel

Next to the sleeve stack, and in `Reproductor` next to the record, `Tocadiscos` SHALL show an information panel with the station name (bold, up to three lines), its characteristics (country, language and up to three tags) and a heart next to the name. When the shown station is the current one, the panel SHALL also show its play state. The panel SHALL scroll when its text does not fit.

#### Scenario: Panel for the focused station

- **WHEN** the focused station in `Inicio` is playing
- **THEN** the panel shows its name, characteristics, heart and `Reproduciendo`

#### Scenario: Heart in the panel

- **WHEN** a logged-in user activates the outline heart in the panel
- **THEN** the station is added to the favourites and the heart is filled
- **AND** a logged-out user is taken to `Iniciar sesión` instead

### Requirement: Stack focus after a reload

When the search or the applied preferences reload `Inicio`, the stack SHALL focus the first station. Returning to the page without a reload SHALL keep the focused station. In `Favoritos`, after the focused station is removed, the focus SHALL move to the next station, or to the previous one when it was the last.

#### Scenario: New search

- **WHEN** the user searches `jazz`
- **THEN** the stack shows the results with the first one focused

#### Scenario: Removing the focused favourite

- **WHEN** the user removes the focused station in `Favoritos` and it was not the last one
- **THEN** the station leaves the stack and the next station is focused

### Requirement: Tocadiscos Inicio and Favoritos content

`Inicio` SHALL show, from top to bottom, the header, the search row (input, `Buscar` and `Limpiar búsqueda`), the active filters bar when it applies, the stack with its panel, and the bottom bar. `Favoritos` SHALL show the compact email and `Cerrar sesión` row instead of the search row. Their loading, error, empty and logged-out states SHALL replace the stack, centred, with the same texts and buttons as in `Moderno`.

#### Scenario: Inicio states

- **WHEN** the API cannot be reached in `Tocadiscos`
- **THEN** `Inicio` shows `No hay conexión con el servidor.` and `Reintentar` in place of the stack

#### Scenario: Favoritos logged out

- **WHEN** a logged-out user opens `Favoritos` in `Tocadiscos`
- **THEN** it shows `Inicia sesión para ver tus favoritas.` and `Iniciar sesión`

### Requirement: Turntable

In `Tocadiscos`, `Reproductor` SHALL show a large vinyl record, as big as the available space allows and never clipped, with grooves, a shine, a central label showing the station logo cropped to a circle (or the fallback image), a spindle, and a tonearm whose pivot is outside the record. Its size SHALL depend on the space, not on the text size.

#### Scenario: Station selected

- **WHEN** a station is selected and the user opens `Reproductor`
- **THEN** the record's label shows the station logo or the fallback image
- **AND** the information panel shows the station's name and characteristics

#### Scenario: No station selected

- **WHEN** no station has been selected
- **THEN** the record shows a blank label, the tonearm is raised and cannot be pressed
- **AND** the page shows `Selecciona una emisora en Inicio o en tus favoritas`

### Requirement: Tonearm control

The tonearm SHALL be the play/stop control of `Tocadiscos`: pressing it SHALL start playback when stopped or in error and stop it when buffering or playing. Its touch area SHALL be at least 48 units wide along the whole arm, and on Windows it SHALL be focusable and activated with Enter or Space.

#### Scenario: Pressing the tonearm while stopped

- **WHEN** a station is stopped and the user presses the tonearm
- **THEN** the station plays, the tonearm lowers onto the record and the record starts spinning

#### Scenario: Pressing the tonearm while playing

- **WHEN** a station is playing and the user presses the tonearm
- **THEN** playback stops, the tonearm lifts and the record slows to a stop

### Requirement: Turntable follows the playback state

The tonearm and the record SHALL only follow the playback state and SHALL never start, delay or block audio: when stopped or in error the tonearm SHALL be raised and the record still; while buffering or playing the tonearm SHALL rest on the record and the record SHALL spin at about 33 rpm.

#### Scenario: Stream failure

- **WHEN** the station fails to play
- **THEN** the tonearm lifts and the record stops
- **AND** the status line shows `Error`, `No se pudo reproducir esta emisora.` and `Reintentar`
- **AND** the app does not crash

#### Scenario: Opening Reproductor while playing

- **WHEN** a station started from `Inicio` is playing and the user opens `Reproductor`
- **THEN** the tonearm is already on the record and the record is already spinning at full speed, without a landing animation

### Requirement: Turntable motion

Lowering the tonearm SHALL take about 600 ms with a short settle on landing (under 300 ms), and raising it about 500 ms. The record SHALL speed up over about 1.2 s and slow down over about 1.8 s, always continuing from its current angle. All motion SHALL stop while the page is not visible and resume in the right state when it is shown again.

#### Scenario: Leaving Reproductor while playing

- **WHEN** a station is playing and the user goes to `Inicio`
- **THEN** the record and tonearm animations stop while `Reproductor` is hidden, and the audio continues
- **WHEN** the user returns to `Reproductor`
- **THEN** the record is spinning and the tonearm is on it

### Requirement: Reduced motion

When the operating system asks for reduced motion (animations turned off on Windows, animator duration scale 0 on Android), `Tocadiscos` SHALL skip the tonearm swing and settle and the record rotation: the tonearm SHALL change position at once and the record SHALL stay still, while the tonearm position still shows the state.

#### Scenario: Animations turned off on Windows

- **WHEN** Windows animations are turned off and the user presses the tonearm while stopped
- **THEN** the tonearm moves onto the record at once, the record does not rotate and the station plays

### Requirement: Record crate

At the bottom of `Reproductor`, `Tocadiscos` SHALL show a wooden crate split into two compartments of three records each, six per page, holding the user's favourites in the `Favoritos` order. Each record SHALL show its sleeve with the station logo and the station name on one line ending with `…`. Empty slots SHALL be shown as empty.

#### Scenario: Eight favourites

- **WHEN** a logged-in user with eight favourites opens `Reproductor`
- **THEN** the crate shows the first six favourites, arrows and the indicator `1/2`
- **WHEN** the user activates `Favoritas siguientes` or swipes left
- **THEN** the crate shows the other two favourites and four empty slots, and the indicator `2/2`

#### Scenario: Six favourites or fewer

- **WHEN** the user has four favourites
- **THEN** the crate shows four records and two empty slots, with no arrows and no indicator

### Requirement: Playing from the crate

Pressing a record in the crate SHALL select and play that station, stopping the previous one, and the record's label and the tonearm SHALL follow. The record of the current station SHALL be raised and outlined, and when the page opens the crate SHALL show the page that contains the current station, or the first page.

#### Scenario: Pressing a crate record

- **WHEN** a station is playing and the user presses another record in the crate
- **THEN** the previous station stops and the pressed station plays
- **AND** its record is raised and outlined

#### Scenario: Removed favourites

- **WHEN** the crate shows page 2 and favourites are removed so that page 2 no longer exists
- **THEN** the crate shows the last page

### Requirement: Crate without favourites

When no user is logged in, every slot SHALL be empty, the crate SHALL show `Inicia sesión para ver tus favoritas` and pressing the crate SHALL open `Iniciar sesión`. A logged-in user without favourites SHALL see `Aún no tienes emisoras favoritas.`

#### Scenario: Logged out

- **WHEN** a logged-out user presses the crate
- **THEN** `Iniciar sesión` opens and nothing is added to the favourites

### Requirement: Tocadiscos Ajustes and settings pages

`Ajustes` in `Tocadiscos` SHALL keep the `Moderno` structure: the header, the cards `PERSONALIZACIÓN` and `PREFERENCIAS` at full width, drawn as cream record sleeves with a vinyl peeking out and a brass border, and the bottom bar with nothing marked. `Personalización` and `Preferencias` SHALL keep their content and behaviour on a rounded wood panel.

#### Scenario: Opening a settings card

- **WHEN** the user activates the `PREFERENCIAS` card in `Tocadiscos`
- **THEN** `Preferencias` opens with its usual pickers and buttons inside the Tocadiscos frame

### Requirement: Tocadiscos accessible names

Every interactive element of `Tocadiscos` SHALL have a Spanish accessible name: the tonearm `Aguja del tocadiscos. Reproducir` or `Aguja del tocadiscos. Detener` following the state, the focused sleeve `{nombre}, emisora {n} de {total}. Pulsa para reproducir`, the smaller sleeves `Mostrar {nombre}`, crate records `Reproducir {nombre}, favorita {n} de {total}`, the crate arrows `Favoritas anteriores` and `Favoritas siguientes`, and the house `Inicio`.

#### Scenario: Screen reader on the tonearm

- **WHEN** a screen reader focuses the tonearm while a station is playing
- **THEN** it announces `Aguja del tocadiscos. Detener`

#### Scenario: Reaching every sleeve

- **WHEN** a screen reader user activates the second sleeve, `Mostrar {nombre}`
- **THEN** that station becomes the focused sleeve, so every station can be reached one by one

### Requirement: Tocadiscos keyboard use on Windows

On Windows the Tab order in `Tocadiscos` SHALL follow the visual order. In the sleeve stack Up and Down SHALL move the focus and Enter or Space SHALL play; in the crate Left and Right SHALL move between records and Enter or Space SHALL play. The tonearm and the crate arrows SHALL be focusable, and the focused element SHALL be visibly indicated.

#### Scenario: Playing from the crate with the keyboard

- **WHEN** the user tabs to a crate record, presses Right and then Enter
- **THEN** the next record's station plays
