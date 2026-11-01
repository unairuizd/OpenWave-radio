# Spec Delta

## MODIFIED Requirements

### Requirement: Station list in Inicio

When the station list loads successfully, `Inicio` SHALL show the stations returned by the API as a grid of station cards, one card per station, with at least two columns on a phone and more columns on wider windows. The list SHALL contain every station returned by the API. Without an active search, `Inicio` SHALL request and show the top stations from the catalogue.

#### Scenario: API is running

- **WHEN** the API is running and the user opens `Inicio`
- **THEN** the page lists the stations returned by the API as cards
- **AND** each visible card shows the station name

#### Scenario: Loading the list

- **WHEN** the station request is in progress
- **THEN** `Inicio` shows a loading indicator
- **AND** no error message is shown

#### Scenario: Empty catalogue

- **WHEN** the API returns an empty station list
- **THEN** `Inicio` shows `No se encontraron emisoras.`

#### Scenario: Wide window

- **WHEN** the Windows window is widened
- **THEN** the grid shows more columns, about one for every 170 units of width, and never fewer than two

### Requirement: Selecting a station

Tapping a station card in `Inicio`, or its play button, SHALL select that station, navigate to `Reproductor`, and start its stream. If another station was playing, the previous stream SHALL stop.

#### Scenario: Selecting a station

- **WHEN** the user taps a station card or its play button
- **THEN** `Reproductor` is shown
- **AND** it displays the name and country of that station
- **AND** playback of that station starts

### Requirement: Station search by name

`Inicio` SHALL provide a search input that takes the remaining width of its row and SHALL send the entered name to the OpenWave API, together with the saved preference filters while they are applied. The search SHALL run when the user submits the input from the keyboard or activates the search icon button `Buscar`. A clear icon button `Limpiar búsqueda` SHALL clear the input and restore the top stations, still filtered by the applied preferences. The app SHALL NOT call Radio Browser or any other catalogue host directly.

#### Scenario: Searching for a station

- **WHEN** the user enters `jazz` and submits the search
- **THEN** `Inicio` requests the station list from the OpenWave API with the `jazz` name filter
- **AND** the list shows the stations returned for that search

#### Scenario: Clearing the search

- **WHEN** the user activates the clear button
- **THEN** the search input is empty
- **AND** `Inicio` requests and shows the top stations again

#### Scenario: No search results

- **WHEN** no preference filter is applied and the search returns no stations
- **THEN** `Inicio` shows `No se encontraron emisoras.`
- **AND** the search input remains available

## ADDED Requirements

### Requirement: Station cards

Each station card SHALL be taller than it is wide and show, from top to bottom: the station logo (or the fallback image when it is missing or fails), the name (bold, at most two lines, ending with `…`), the characteristics (country · language, then up to three tags; at most two lines, ending with `…`), and a row with a play button on the left and a heart on the right.

#### Scenario: Card content

- **WHEN** a station from Spain in Spanish with the tags `pop`, `rock`, `news` and `talk` is listed
- **THEN** its card shows the logo or fallback, the name, `Spain · Spanish` and the tags `pop`, `rock` and `news`, a play button and a heart

#### Scenario: Heart on a card

- **WHEN** a logged-in user activates the heart of a card
- **THEN** the favourite is added or removed as for any heart
- **AND** the station is not selected and playback does not change
