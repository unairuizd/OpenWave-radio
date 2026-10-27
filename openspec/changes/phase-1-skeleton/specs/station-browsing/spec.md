# Spec Delta

## Purpose

Lets the user see the available stations in Inicio and understand what the app is doing while the list loads or when the backend cannot be reached.

## ADDED Requirements

### Requirement: Station list in Inicio

When the station list loads successfully, `Inicio` SHALL show the stations returned by the API, with at least the station name visible for each entry. The list SHALL contain every station returned by the API.

#### Scenario: API is running

- **WHEN** the API is running and the user opens `Inicio`
- **THEN** the page lists at least 10 stations
- **AND** each visible entry shows the station name

#### Scenario: Loading the list

- **WHEN** the station request is in progress
- **THEN** `Inicio` shows a loading indicator
- **AND** no error message is shown

### Requirement: Error state with retry

When the station request fails because the API is unreachable, `Inicio` SHALL show an error message in Spanish and a `Reintentar` button, and the app SHALL remain usable without crashing. Activating `Reintentar` SHALL request the stations again.

#### Scenario: API is stopped

- **WHEN** the API is stopped and the user opens `Inicio`
- **THEN** the page shows an error message in Spanish
- **AND** it shows a `Reintentar` button
- **AND** the app does not crash and the other destinations remain reachable

#### Scenario: Retry succeeds

- **WHEN** the error state is shown and the user activates `Reintentar` while the API is running
- **THEN** the request is sent again
- **AND** the error message disappears and the station list is shown

### Requirement: Search placeholder

`Inicio` SHALL show a visible search input placeholder. In this phase the input SHALL NOT filter or reorder the station list.

#### Scenario: Using the search box

- **WHEN** the user types text into the search input
- **THEN** the visible station list is unchanged

### Requirement: Selecting a station

Tapping a station in `Inicio` SHALL select that station and navigate to `Reproductor`, which SHALL display the selected station.

#### Scenario: Selecting a station

- **WHEN** the user taps a station in the list
- **THEN** `Reproductor` is shown
- **AND** it displays the name and country of the tapped station
