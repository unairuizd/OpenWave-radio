# Spec Delta

## ADDED Requirements

### Requirement: Live station catalogue endpoint

The API SHALL expose `GET /api/stations` and return HTTP 200 with a JSON array of stations from the Radio Browser catalogue. The endpoint SHALL accept optional `name`, `country`, `language` and `tag` query filters and an optional `limit` query parameter. `limit` SHALL default to 50 and SHALL be capped at 100. The API SHALL request the catalogue with `hidebroken=true`, `order=clickcount` and `reverse=true`. Each station SHALL expose `id`, `name` and `streamUrl` as non-empty strings, and `country`, `language`, `tags` and `logoUrl` as optional values. `tags` SHALL be a list of strings when present. Stations without a usable stream URL SHALL be omitted and names SHALL be trimmed.

#### Scenario: Requesting the station list

- **WHEN** the API is running and `GET /api/stations` is requested
- **THEN** the response status is 200
- **AND** the body contains a JSON array of mapped stations
- **AND** every station has a non-empty `id`, `name` and `streamUrl`

#### Scenario: Filtering by name

- **WHEN** `GET /api/stations?name=jazz` is requested
- **THEN** the response status is 200
- **AND** every returned station's name contains `jazz`, ignoring case

#### Scenario: Forwarding optional filters

- **WHEN** `GET /api/stations?country=Spain&language=spanish&tag=jazz` is requested
- **THEN** the response status is 200
- **AND** the `country`, `language` and `tag` filters are applied to the Radio Browser search

#### Scenario: Limiting the result count

- **WHEN** `GET /api/stations` is requested with `limit=5`
- **THEN** at most 5 stations are returned
- **WHEN** `limit` is omitted
- **THEN** at most 50 stations are returned
- **WHEN** `limit` is greater than 100
- **THEN** at most 100 stations are returned

#### Scenario: Stable and unique station identity

- **WHEN** `GET /api/stations` is requested more than once
- **THEN** each station keeps the same `id` between requests
- **AND** no two stations share the same `id`

#### Scenario: Mapping a Radio Browser station

- **WHEN** a Radio Browser station has `stationuuid`, `name`, `url_resolved`, `country`, `language`, `tags` and `favicon`
- **THEN** `id` is the `stationuuid`, `name` is trimmed, `streamUrl` is `url_resolved`, `country` and `language` are copied, `tags` is a list split from the comma-separated value, and `logoUrl` is `favicon`

#### Scenario: Missing stream URL

- **WHEN** a Radio Browser station has neither `url_resolved` nor `url`
- **THEN** that station is omitted from the response

### Requirement: Radio Browser mirror fallback

The API SHALL read an ordered list of Radio Browser mirror servers from configuration `RadioBrowser:Servers`. It SHALL try the mirrors in order with a 5-second timeout per attempt and use the first successful response. Every request to a mirror SHALL send the header `User-Agent: OpenWave/1.0`.

#### Scenario: First mirror is unavailable

- **WHEN** the first configured mirror times out or returns an error and a later mirror succeeds
- **THEN** the API returns HTTP 200 with the stations from the successful mirror

#### Scenario: All mirrors are unavailable

- **WHEN** every configured mirror times out or returns an error
- **THEN** the API returns HTTP 502 with a ProblemDetails response
- **AND** the `detail` value is exactly `No se pudo obtener el catálogo de emisoras.`
- **AND** the technical cause is logged by the API

## REMOVED Requirements

### Requirement: Station list endpoint

**Reason**: The fixed test catalogue endpoint is replaced by the live Radio Browser catalogue endpoint.

**Migration**: Use `GET /api/stations` with the new optional filters and limit; the response now comes from Radio Browser.

### Requirement: Plausible test station data

**Reason**: The hard-coded test catalogue is replaced by the live Radio Browser catalogue.

**Migration**: No action for clients; real catalogue data replaces the test data.
