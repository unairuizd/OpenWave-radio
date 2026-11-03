# Spec Delta

## MODIFIED Requirements

### Requirement: Station list in Inicio

When the station list loads successfully, `Inicio` SHALL show every station returned by the API, one entry per station: in `Moderno` as a grid of station cards with at least two columns on a phone and more columns on wider windows, in `Tocadiscos` as the stack of record sleeves, and in every other theme as its theme specification describes. Without an active search, `Inicio` SHALL request and show the top stations from the catalogue.

#### Scenario: API is running

- **WHEN** the API is running and the user opens `Inicio`
- **THEN** the page lists the stations returned by the API as cards in `Moderno`, as sleeves in `Tocadiscos`, or as the entries of the selected theme
- **AND** each visible card, the focused sleeve's information panel, or each visible entry of the other themes shows the station name

#### Scenario: Loading the list

- **WHEN** the station request is in progress
- **THEN** `Inicio` shows a loading indicator
- **AND** no error message is shown

#### Scenario: Empty catalogue

- **WHEN** the API returns an empty station list
- **THEN** `Inicio` shows `No se encontraron emisoras.`

#### Scenario: Wide window

- **WHEN** the Windows window is widened in `Moderno`
- **THEN** the grid shows more columns, about one for every 170 units of width, and never fewer than two

#### Scenario: Up to a hundred stations

- **WHEN** the API returns 100 stations and any theme other than `Moderno` is selected
- **THEN** the user can reach every station and the page stays responsive

### Requirement: Selecting a station

Selecting a station in `Inicio` SHALL select it and start its stream, stopping the previous one. In `Moderno`, tapping a station card or its play button SHALL also open `Reproductor`. In `Tocadiscos`, pressing the focused sleeve SHALL play the station and stay in `Inicio`, and pressing it again while it is the current station SHALL toggle play and stop. In every other theme, the play action its theme specification names SHALL behave as the focused sleeve does in `Tocadiscos`.

#### Scenario: Selecting a station

- **WHEN** the user taps a station card or its play button in `Moderno`
- **THEN** `Reproductor` is shown
- **AND** it displays the name and country of that station
- **AND** playback of that station starts

#### Scenario: Selecting a station in Tocadiscos

- **WHEN** the user presses the focused sleeve of a station that is not playing in `Tocadiscos`
- **THEN** playback of that station starts
- **AND** `Inicio` stays on screen and its information panel shows `Cargando…` and then `Reproduciendo`

#### Scenario: Selecting a station in another theme

- **WHEN** the user activates the play action of a station that is not playing in `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` or `Clásico de rueda`
- **THEN** playback of that station starts and `Inicio` stays on screen
- **AND** activating the same action again while it plays stops it

### Requirement: Selected station is identifiable

`Inicio` SHALL visibly mark the station currently selected in the app: in `Moderno` with the card highlight, in `Tocadiscos` with the peeking disc and outline on its sleeve, and in every other theme with the mark its theme specification describes. The mark SHALL move when another station is selected and SHALL be restored when `Inicio` is shown again. `Reproductor` SHALL identify the same station by showing its name and country.

#### Scenario: Highlighting the selected station

- **WHEN** the user selects a station in `Inicio`
- **THEN** that station's list entry is visually distinguished from the other entries
- **AND** `Reproductor` shows the same station's name and country

#### Scenario: Moving the highlight

- **WHEN** the user selects a different station
- **THEN** the highlight moves to the new station's entry
- **AND** the previous entry is no longer highlighted

#### Scenario: Returning to Inicio

- **WHEN** the user leaves `Inicio` and returns to it
- **THEN** the currently selected station is still highlighted if it is present in the list
