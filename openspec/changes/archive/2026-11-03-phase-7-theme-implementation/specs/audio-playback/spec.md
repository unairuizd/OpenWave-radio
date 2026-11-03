# Spec Delta

## MODIFIED Requirements

### Requirement: Selected station on the player

`Reproductor` SHALL show the selected station's logo (or a local fallback image when no logo is available or it cannot be loaded), its name and its characteristics (country, language and tags), and its heart. In `Moderno` the logo is a large image with the name and characteristics under it on the left and the heart on the right; in `Tocadiscos` the logo is the label of the record; in every other theme its theme specification says where the logo is. When no station has been selected, the page SHALL say so instead of showing station details, and the play/stop control SHALL be disabled.

#### Scenario: No station selected

- **WHEN** the user opens `Reproductor` before selecting any station
- **THEN** the page shows `Selecciona una emisora en Inicio` in `Moderno`, or `Selecciona una emisora en Inicio o en tus favoritas` in every other theme
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

`Reproductor` SHALL play the selected station's stream and SHALL provide a control that starts and stops playback: in `Moderno` a central button showing a play icon named `Reproducir` when stopped or in error and a stop icon named `Detener` while buffering or playing; in `Tocadiscos` the tonearm; in every other theme the control its theme specification names, whose accessible name SHALL be `Reproducir` when stopped or in error and `Detener` while buffering or playing. The status text SHALL be `Detenido` when stopped, `Cargando…` while buffering, `Reproduciendo` while audio is playing and `Error` after a failure, and SHALL reflect actual playback rather than a simulated flag.

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

#### Scenario: Play and stop in another theme

- **WHEN** a station is selected in `Boombox` and the user activates the key named `Reproducir`, then the same key, now named `Detener`
- **THEN** the station plays and then stops, with the same status texts as in `Moderno`

### Requirement: Previous and next favourite

In `Moderno`, `Reproductor` SHALL offer `Favorita anterior` and `Favorita siguiente` on either side of the play/stop control. They SHALL move through the logged-in user's favourites in the `Favoritos` order (newest first), wrapping from the last to the first and from the first to the last, and SHALL select and play the chosen station, stopping the previous one. `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` and `Clásico de rueda` SHALL offer the same two controls, with the same accessible names and behaviour, where their theme specifications place them. `Tocadiscos` SHALL offer the record crate instead, and `Radio de válvulas` the preset keys.

#### Scenario: Next favourite

- **WHEN** the current station is the second of three favourites and the user activates `Favorita siguiente`
- **THEN** the third favourite is selected and starts playing

#### Scenario: Next on the last favourite

- **WHEN** the current station is the last favourite and the user activates `Favorita siguiente`
- **THEN** the first favourite is selected and starts playing

#### Scenario: Previous on the first favourite

- **WHEN** the current station is the first favourite and the user activates `Favorita anterior`
- **THEN** the last favourite is selected and starts playing

#### Scenario: Previous and next in another theme

- **WHEN** the current station is the first of three favourites in `Casete portátil` and the user activates the key named `Favorita siguiente` (labelled `FF`)
- **THEN** the second favourite is selected and starts playing
