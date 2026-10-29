# Spec Delta

## MODIFIED Requirements

### Requirement: All backend access goes through the configured base URL

The app SHALL send every OpenWave API request through the configured base URL. No screen, view model or service SHALL use a hard-coded API host. All station catalogue and search access SHALL go through the OpenWave API; the app SHALL NOT call Radio Browser or any other catalogue host directly. Audio playback MAY connect directly to the stream URL supplied by the API.

#### Scenario: Observing app network traffic

- **WHEN** the user navigates the app, loads the station list or searches for a station
- **THEN** every OpenWave API request targets the configured base URL
- **AND** no request targets Radio Browser, MySQL or any other catalogue host

#### Scenario: Playing a station

- **WHEN** the user starts playback of a station
- **THEN** the audio player connects to the stream URL returned by the API
- **AND** the app does not contact Radio Browser to resolve the station
