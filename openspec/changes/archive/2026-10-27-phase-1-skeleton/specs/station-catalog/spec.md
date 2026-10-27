# Spec Delta

## Purpose

Supplies the app with the catalogue of radio stations to display. In this phase the catalogue is fixed test data served by the API, so the app can be built and demonstrated without external dependencies.

## ADDED Requirements

### Requirement: Station list endpoint

The API SHALL expose `GET /api/stations` and return HTTP 200 with a JSON array of at least 10 stations. Each station SHALL expose `id`, `name` and `streamUrl` as non-empty strings, and `country`, `language`, `tags` and `logoUrl` as optional values. `tags` SHALL be a list of strings when present.

#### Scenario: Requesting the station list

- **WHEN** the API is running and `GET /api/stations` is requested
- **THEN** the response status is 200
- **AND** the body contains a JSON array with at least 10 stations
- **AND** every station has a non-empty `id`, `name` and `streamUrl`

#### Scenario: Stable and unique station identity

- **WHEN** `GET /api/stations` is requested more than once
- **THEN** each station keeps the same `id` between requests
- **AND** no two stations share the same `id`

#### Scenario: Catalogue available without external services

- **WHEN** the API has no internet access and no database configured
- **THEN** `GET /api/stations` still returns the full test catalogue with status 200

### Requirement: Plausible test station data

The test stations SHALL use plausible names, countries, languages and tags so the list is useful for manual verification. Stream URLs MAY be placeholders and are not required to be playable in this phase.

#### Scenario: Reading a test station

- **WHEN** the station list is returned
- **THEN** each station has a recognisable name and a country and language value
- **AND** at least some stations expose tags
