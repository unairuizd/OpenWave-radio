# Audio Playback Specification

## Purpose

Provides real streaming playback for the selected radio station, with clear Spanish status and failure messages, so the user can listen to the catalogue and recover from unreachable streams without the app crashing.

## Requirements

### Requirement: Selected station on the player

`Reproductor` SHALL show the name and country of the currently selected station and its logo when a logo URL is available. When no logo is available or it cannot be loaded, the page SHALL show a local fallback image. When no station has been selected, the page SHALL show `Selecciona una emisora en Inicio` instead of station details.

#### Scenario: No station selected

- **WHEN** the user opens `Reproductor` before selecting any station
- **THEN** the page shows `Selecciona una emisora en Inicio`
- **AND** no station name, country or playback control is shown

#### Scenario: Station selected

- **WHEN** the user has selected a station in `Inicio`
- **THEN** `Reproductor` shows that station's name and country
- **AND** it shows the station logo or the fallback image

#### Scenario: Logo cannot be loaded

- **WHEN** the selected station has no logo URL or its logo cannot be loaded
- **THEN** the fallback image remains visible
- **AND** the app does not crash

### Requirement: Real play and stop

`Reproductor` SHALL play the selected station's stream and SHALL provide a control that stops playback. The status text SHALL be `Detenido` when stopped, `Cargando…` while buffering, `Reproduciendo` while audio is playing and `Error` after a failure. The control SHALL read `Reproducir` when stopped and `Detener` while buffering or playing. The status SHALL reflect actual playback rather than a simulated flag.

#### Scenario: Starting playback

- **WHEN** a station is selected and its stream is reachable
- **THEN** the status text changes to `Cargando…`
- **AND** it then changes to `Reproduciendo`
- **AND** audio from the station is heard
- **AND** the control offers `Detener`

#### Scenario: Stopping playback

- **WHEN** playback is running and the user activates `Detener`
- **THEN** audio stops
- **AND** the status text changes to `Detenido`
- **AND** the control offers `Reproducir`

### Requirement: Playback failure and retry

When the stream cannot be opened or fails during playback, `Reproductor` SHALL enter the `Error` state and show `No se pudo reproducir esta emisora.` The app SHALL remain usable and SHALL NOT crash. The page SHALL offer `Reintentar`, which retries the same station, and the user SHALL also be able to choose another station in `Inicio`.

#### Scenario: Stream is unreachable

- **WHEN** the selected station's stream is unreachable
- **THEN** the status text is `Error`
- **AND** the page shows `No se pudo reproducir esta emisora.`
- **AND** the app does not crash

#### Scenario: Retrying after a failure

- **WHEN** the error state is shown and the user activates `Reintentar` while the stream is reachable
- **THEN** playback is attempted again
- **AND** the status text changes to `Cargando…` and then `Reproduciendo`

#### Scenario: Choosing another station after a failure

- **WHEN** the error state is shown and the user selects another station in `Inicio`
- **THEN** `Reproductor` shows the new station
- **AND** playback of the new station starts

### Requirement: Switching stations

Selecting a different station while audio is playing SHALL stop the previous stream before starting the new one. `Reproductor` SHALL show the newly selected station and its playback state.

#### Scenario: Selecting another station while playing

- **WHEN** a station is playing and the user selects a different station
- **THEN** the previous stream stops
- **AND** `Reproductor` shows the newly selected station
- **AND** the new stream starts

#### Scenario: Selecting the same station

- **WHEN** the user selects the station that is already playing
- **THEN** that station continues playing
- **AND** no second playback starts

### Requirement: Playback is scoped to the player page

Playback SHALL be tied to the `Reproductor` page in this phase. Leaving the page SHALL stop audio and return the playback state to `Detenido`. There SHALL be no mini-player and no background playback.

#### Scenario: Leaving the player

- **WHEN** audio is playing and the user switches to another destination
- **THEN** audio stops
- **AND** the playback state is `Detenido`

#### Scenario: Returning to the player

- **WHEN** the user returns to `Reproductor` after leaving it
- **THEN** the same station is shown
- **AND** the status text is `Detenido`
- **AND** the user can start playback again
