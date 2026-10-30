# Spec Delta

## ADDED Requirements

### Requirement: Account and favourite data only through the API

The app SHALL read and change account and favourite data only through the OpenWave API at the configured base URL. The app SHALL NOT connect to the MySQL database or contain database credentials.

#### Scenario: Logging in and changing favourites

- **WHEN** the user logs in, creates an account, or adds, lists or removes favourites
- **THEN** every request targets the OpenWave API at the configured base URL
- **AND** the app opens no connection to the MySQL database

#### Scenario: Inspecting the app configuration

- **WHEN** the app's source and settings are inspected
- **THEN** they contain no database host, user or password
