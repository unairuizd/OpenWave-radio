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

The app SHALL send every backend request through the configured base URL. No screen, view model or service SHALL use a hard-coded host, and the app SHALL NOT call any external catalogue, database or other host in this phase.

#### Scenario: Observing app network traffic

- **WHEN** the user navigates the app and loads the station list
- **THEN** every outgoing HTTP request targets the configured base URL
- **AND** no request targets Radio Browser, MySQL or any other host
