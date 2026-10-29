# Spec Delta

## MODIFIED Requirements

### Requirement: Station list in Inicio

When the station list loads successfully, `Inicio` SHALL show the stations returned by the API, with at least the station name visible for each entry. The list SHALL contain every station returned by the API. Without an active search, `Inicio` SHALL request and show the top stations from the catalogue.

#### Scenario: API is running

- **WHEN** the API is running and the user opens `Inicio`
- **THEN** the page lists the stations returned by the API
- **AND** each visible entry shows the station name

#### Scenario: Loading the list

- **WHEN** the station request is in progress
- **THEN** `Inicio` shows a loading indicator
- **AND** no error message is shown

#### Scenario: Empty catalogue

- **WHEN** the API returns an empty station list
- **THEN** `Inicio` shows `No se encontraron emisoras.`

### Requirement: Error state with retry

When the station request fails, `Inicio` SHALL show an error message in Spanish and a `Reintentar` button, and the app SHALL remain usable without crashing. If the API cannot be reached, the message SHALL be `No hay conexión con el servidor.` If the API reports that the catalogue is unavailable, the message SHALL be `No se pudo obtener el catálogo de emisoras.` Activating `Reintentar` SHALL repeat the last request, including the current search term when one is active.

#### Scenario: API is stopped

- **WHEN** the API is stopped and the user opens `Inicio`
- **THEN** the page shows `No hay conexión con el servidor.`
- **AND** it shows a `Reintentar` button
- **AND** the app does not crash and the other destinations remain reachable

#### Scenario: Catalogue is unavailable

- **WHEN** the API returns HTTP 502 for the station request
- **THEN** `Inicio` shows `No se pudo obtener el catálogo de emisoras.`
- **AND** it shows a `Reintentar` button

#### Scenario: Retry succeeds

- **WHEN** the error state is shown and the user activates `Reintentar` while the API is running
- **THEN** the last request is sent again
- **AND** the error message disappears and the station list is shown

### Requirement: Selecting a station

Tapping a station in `Inicio` SHALL select that station, navigate to `Reproductor`, and start its stream. If another station was playing, the previous stream SHALL stop.

#### Scenario: Selecting a station

- **WHEN** the user taps a station in the list
- **THEN** `Reproductor` is shown
- **AND** it displays the name and country of the tapped station
- **AND** playback of that station starts

## ADDED Requirements

### Requirement: Station search by name

`Inicio` SHALL provide a search input and SHALL send the entered name to the OpenWave API. The search SHALL run when the user submits the input from the keyboard or the visible search button. A visible clear button SHALL clear the input and restore the top stations. The app SHALL NOT call Radio Browser or any other catalogue host directly.

#### Scenario: Searching for a station

- **WHEN** the user enters `jazz` and submits the search
- **THEN** `Inicio` requests the station list from the OpenWave API with the `jazz` name filter
- **AND** the list shows the stations returned for that search

#### Scenario: Clearing the search

- **WHEN** the user activates the clear button
- **THEN** the search input is empty
- **AND** `Inicio` requests and shows the top stations again

#### Scenario: No search results

- **WHEN** the search returns no stations
- **THEN** `Inicio` shows `No se encontraron emisoras.`
- **AND** the search input remains available

### Requirement: Selected station is identifiable

`Inicio` SHALL visibly highlight the station currently selected in the app. The highlight SHALL move when another station is selected and SHALL be restored when `Inicio` is shown again. `Reproductor` SHALL identify the same station by showing its name and country.

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

## REMOVED Requirements

### Requirement: Search placeholder

**Reason**: The search input now filters the station list by name instead of being a non-functional placeholder.

**Migration**: Use the search input and submit it to search; clear it to return to the top stations.
