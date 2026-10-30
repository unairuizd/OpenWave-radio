# API Connectivity Specification

## Purpose

Guarantees that the app has exactly one place to configure where the backend lives, so the same build works on Windows and on the Android emulator and can be pointed at another host without code changes.

## Requirements

### Requirement: Configurable backend base URL

The app SHALL obtain the backend base URL from a single configuration source. The default SHALL be `http://localhost:5080` on Windows and `http://10.0.2.2:5080` on Android. The value SHALL be overridable through the user-level preference `api_base_url`, and the override SHALL apply without rebuilding the app.

#### Scenario: Default URL on Windows

- **WHEN** the app runs on Windows and no override is configured
- **THEN** backend requests are sent to `http://localhost:5080`

#### Scenario: Default URL on the Android emulator

- **WHEN** the app runs on the Android emulator and no override is configured
- **THEN** backend requests are sent to `http://10.0.2.2:5080`

#### Scenario: Overriding the base URL

- **WHEN** the preference `api_base_url` holds another absolute HTTP(S) URL
- **THEN** subsequent backend requests are sent to that URL instead of the platform default

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

### Requirement: Account and favourite data only through the API

The app SHALL read and change account and favourite data only through the OpenWave API at the configured base URL. The app SHALL NOT connect to the MySQL database or contain database credentials.

#### Scenario: Logging in and changing favourites

- **WHEN** the user logs in, creates an account, or adds, lists or removes favourites
- **THEN** every request targets the OpenWave API at the configured base URL
- **AND** the app opens no connection to the MySQL database

#### Scenario: Inspecting the app configuration

- **WHEN** the app's source and settings are inspected
- **THEN** they contain no database host, user or password
