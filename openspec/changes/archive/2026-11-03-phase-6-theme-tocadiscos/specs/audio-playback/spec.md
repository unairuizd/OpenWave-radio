# Spec Delta

## ADDED Requirements

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

## MODIFIED Requirements

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

## REMOVED Requirements

### Requirement: Playback is scoped to the player page

**Reason**: Audio must keep playing while the user browses (needed by `Tocadiscos`, which plays stations from `Inicio` and `Favoritos`, and by theme changes, which happen in `Personalización`).

**Migration**: Replaced by "Playback continues across pages". Leaving `Reproductor` no longer stops the audio in any theme; the app going to the background still does.
