# Simulated Playback Specification

## Purpose

Provides a working player screen whose state transitions are real from the user's point of view while audio output is simulated, so real playback can replace the simulation later without redesigning the screen.

## Requirements

### Requirement: Selected station on the player

`Reproductor` SHALL show the name and country of the currently selected station. When no station has been selected, it SHALL show `Selecciona una emisora en Inicio` instead.

#### Scenario: No station selected

- **WHEN** the user opens `Reproductor` before selecting any station
- **THEN** the page shows `Selecciona una emisora en Inicio`
- **AND** no station name or country is shown

#### Scenario: Station selected

- **WHEN** the user has selected a station in `Inicio`
- **THEN** `Reproductor` shows that station's name and country

### Requirement: Simulated play and stop

`Reproductor` SHALL provide a control that toggles playback and a status text. While playing, the status text SHALL read `Reproduciendo`; while stopped, it SHALL read `Detenido`. Playback SHALL be simulated: no audio is produced and no stream URL is requested in this phase.

#### Scenario: Starting playback

- **WHEN** a station is selected and the user activates play while stopped
- **THEN** the status text changes to `Reproduciendo`
- **AND** the control offers the stop action

#### Scenario: Stopping playback

- **WHEN** playback is running and the user activates stop
- **THEN** the status text changes to `Detenido`

#### Scenario: Simulated playback does not stream audio

- **WHEN** playback is started with a placeholder or unreachable stream URL
- **THEN** the status still changes to `Reproduciendo`
- **AND** no audio is heard and no request is made to the stream URL

### Requirement: Playback state survives navigation

The current station and the simulated playing state SHALL be kept outside the page so they survive moving between `Inicio`, `Reproductor` and `Favoritos` within the same app session.

#### Scenario: Leaving and returning to the player

- **WHEN** playback is running and the user switches to another destination and back to `Reproductor`
- **THEN** the same station is shown
- **AND** the status still reads `Reproduciendo`

#### Scenario: Selecting a different station while playing

- **WHEN** playback is running and the user selects another station in `Inicio`
- **THEN** `Reproductor` shows the newly selected station
- **AND** the status text reads `Detenido`
