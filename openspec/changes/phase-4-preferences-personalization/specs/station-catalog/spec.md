# Spec Delta

## MODIFIED Requirements

### Requirement: Live station catalogue endpoint

The API SHALL expose `GET /api/stations` and return HTTP 200 with a JSON array of stations from the Radio Browser catalogue. The endpoint SHALL accept optional `name`, `country`, `language`, `tag` and `contentType` query filters and an optional `limit` query parameter. `limit` SHALL default to 50 and SHALL be capped at 100. The API SHALL request the catalogue with `hidebroken=true`, `order=clickcount` and `reverse=true`. Each station SHALL expose `id`, `name` and `streamUrl` as non-empty strings, and `country`, `language`, `tags` and `logoUrl` as optional values. `tags` SHALL be a list of strings when present. Stations without a usable stream URL SHALL be omitted and names SHALL be trimmed.

`name` SHALL match any station whose name contains it, ignoring case. `country`, `language` and `tag` SHALL match only stations whose country, language or one of whose tags is equal to the given value, ignoring case. `contentType` SHALL be one of `music`, `news`, `talk` or `sports`, and SHALL match only stations that have that value as one of their tags. Any other non-empty `contentType` SHALL be rejected with HTTP 400 and a ProblemDetails `detail` of exactly `Tipo de contenido no válido.` When several filters are given, every returned station SHALL satisfy all of them.

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

- **WHEN** `GET /api/stations?country=spain&language=spanish&tag=pop` is requested
- **THEN** the response status is 200
- **AND** every returned station's country is `Spain` and its language is `spanish`
- **AND** every returned station has `pop` as one of its tags, and a station whose only related tag is `k-pop` or `pop rock` is not returned

#### Scenario: Genre and content type together

- **WHEN** `GET /api/stations?tag=jazz&contentType=music` is requested
- **THEN** the response status is 200
- **AND** every returned station has both `jazz` and `music` among its tags, compared ignoring case

#### Scenario: Invalid content type

- **WHEN** `GET /api/stations?contentType=podcast` is requested
- **THEN** the response status is 400
- **AND** the `detail` is `Tipo de contenido no válido.`

#### Scenario: Name search with filters

- **WHEN** `GET /api/stations?name=radio&country=Spain&contentType=news` is requested
- **THEN** every returned station's name contains `radio`, its country is `Spain` and it has the `news` tag

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
