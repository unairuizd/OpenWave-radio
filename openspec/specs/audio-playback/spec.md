# Audio Playback Specification

## Purpose

Provides real streaming playback for the selected radio station, with clear Spanish status and failure messages, so the user can listen to the catalogue and recover from unreachable streams without the app crashing.

## Requirements

### Requirement: Selected station on the player

`Reproductor` SHALL show the selected station's logo (or a local fallback image when no logo is available or it cannot be loaded), its name and its characteristics (country, language and tags), and its heart. In `Moderno` the logo is a large image with the name and characteristics under it on the left and the heart on the right; in `Tocadiscos` the logo is the label of the record. When no station has been selected, the page SHALL say so instead of showing station details, and the play/stop control SHALL be disabled.

#### Scenario: No station selected

- **WHEN** the user opens `Reproductor` before selecting any station
- **THEN** the page shows `Selecciona una emisora en Inicio` in `Moderno`, or `Selecciona una emisora en Inicio o en tus favoritas` in `Tocadiscos`
- **AND** no station name, country or logo is shown
- **AND** the play/stop control is disabled

#### Scenario: Station selected

- **WHEN** the user has selected a station in `Inicio`
- **THEN** `Reproductor` shows that station's name, country, language and tags
- **AND** it shows the station logo or the fallback image

#### Scenario: Logo cannot be loaded

- **WHEN** the selected station has no logo URL or its logo cannot be loaded
- **THEN** the fallback image remains visible
- **AND** the app does not crash

### Requirement: Real play and stop

`Reproductor` SHALL play the selected station's stream and SHALL provide a control that starts and stops playback: in `Moderno` a central button showing a play icon named `Reproducir` when stopped or in error and a stop icon named `Detener` while buffering or playing; in `Tocadiscos` the tonearm. The status text SHALL be `Detenido` when stopped, `Cargando…` while buffering, `Reproduciendo` while audio is playing and `Error` after a failure, and SHALL reflect actual playback rather than a simulated flag.

#### Scenario: Starting playback

- **WHEN** a station is selected and its stream is reachable
- **THEN** the status text changes to `Cargando…`
- **AND** it then changes to `Reproduciendo`
- **AND** audio from the station is heard
- **AND** the control shows the stop icon `Detener` in `Moderno`, or the tonearm rests on the record in `Tocadiscos`

#### Scenario: Stopping playback

- **WHEN** playback is running and the user activates `Detener` or the tonearm
- **THEN** audio stops
- **AND** the status text changes to `Detenido`
- **AND** the control shows the play icon `Reproducir` in `Moderno`, or the tonearm is raised in `Tocadiscos`

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

### Requirement: Previous and next favourite

In `Moderno`, `Reproductor` SHALL offer `Favorita anterior` and `Favorita siguiente` on either side of the play/stop control. They SHALL move through the logged-in user's favourites in the `Favoritos` order (newest first), wrapping from the last to the first and from the first to the last, and SHALL select and play the chosen station, stopping the previous one. `Tocadiscos` SHALL offer the record crate instead.

#### Scenario: Next favourite

- **WHEN** the current station is the second of three favourites and the user activates `Favorita siguiente`
- **THEN** the third favourite is selected and starts playing

#### Scenario: Next on the last favourite

- **WHEN** the current station is the last favourite and the user activates `Favorita siguiente`
- **THEN** the first favourite is selected and starts playing

#### Scenario: Previous on the first favourite

- **WHEN** the current station is the first favourite and the user activates `Favorita anterior`
- **THEN** the last favourite is selected and starts playing

### Requirement: Previous and next from a station that is not a favourite

When no station is selected or the current station is not one of the user's favourites, `Favorita siguiente` SHALL choose the first favourite and `Favorita anterior` the last one. When the only favourite is the current station, nothing SHALL change.

#### Scenario: Current station is not a favourite

- **WHEN** the current station is not a favourite and the user activates `Favorita anterior`
- **THEN** the last favourite is selected and starts playing

#### Scenario: Only favourite is playing

- **WHEN** the user's only favourite is the current station and the user activates `Favorita siguiente`
- **THEN** the same station stays selected and its playback is not restarted

### Requirement: Previous and next without favourites

When no user is logged in, `Favorita anterior` and `Favorita siguiente` SHALL open `Iniciar sesión`, and nothing SHALL be added to the favourites. When the logged-in user has no favourites, they SHALL change nothing and the status line SHALL show `Aún no tienes emisoras favoritas.`

#### Scenario: Logged out

- **WHEN** a logged-out user activates `Favorita siguiente`
- **THEN** `Iniciar sesión` opens

#### Scenario: No favourites

- **WHEN** a logged-in user with no favourites activates `Favorita anterior`
- **THEN** the selected station and playback do not change
- **AND** the status line shows `Aún no tienes emisoras favoritas.`

### Requirement: Playback continues across pages

The app SHALL keep playing the current station while the user moves between pages, opens `Ajustes` or its sub-pages, or changes the theme. Playback SHALL stop only when the user stops it, selects another station, the stream fails, or the app goes to the background. There SHALL still be exactly one audio player and no background playback.

#### Scenario: Leaving the player

- **WHEN** audio is playing in `Reproductor` and the user switches to `Inicio` or `Favoritos`
- **THEN** the audio keeps playing
- **AND** the playback state stays `Reproduciendo`

#### Scenario: Returning to the player

- **WHEN** the user returns to `Reproductor` while the station plays
- **THEN** the same station is shown with the status `Reproduciendo` and its stop control

#### Scenario: App sent to the background

- **WHEN** a station is playing and the app goes to the background
- **THEN** the audio stops and the state is `Detenido`
