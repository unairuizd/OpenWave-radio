# Catalog Options Specification

## Purpose

Supplies the lists of countries, languages, genres and content types that users choose from when they set their default station filters, so the app can offer only values the station search understands.

## Requirements

### Requirement: Option format

Every catalogue option list SHALL be returned as HTTP 200 with a JSON array of options. Each option SHALL have a non-empty `value` (the value to send to the station search), a non-empty `label` (the text to show) and an optional `stationCount`. No list SHALL contain two options with the same `value`.

#### Scenario: Reading an option

- **WHEN** any catalogue option list is requested and the catalogue is available
- **THEN** every element has a non-empty `value` and `label`
- **AND** no two elements share the same `value`

### Requirement: Country options

The API SHALL expose `GET /api/catalog/countries`, built from the Radio Browser country list counted without broken stations. Each option's `value` and `label` SHALL be the country name. Countries with an empty name or with zero stations SHALL be left out. The list SHALL be sorted by `label`, ignoring case.

#### Scenario: Listing countries

- **WHEN** `GET /api/catalog/countries` is requested
- **THEN** the response status is 200
- **AND** the list includes an option whose `value` and `label` are `Spain`
- **AND** no option has an empty name or a `stationCount` of 0
- **AND** the options are in alphabetical order of `label`, ignoring case

### Requirement: Language options

The API SHALL expose `GET /api/catalog/languages` with at most the 100 languages that have the most stations, counted without broken stations. Each option's `value` SHALL be the language name exactly as the catalogue returns it, and its `label` SHALL be the same name with the first letter in upper case. The list SHALL be sorted by `label`, ignoring case.

#### Scenario: Listing languages

- **WHEN** `GET /api/catalog/languages` is requested
- **THEN** the response status is 200 with at most 100 options sorted by `label`
- **AND** the list includes the option with `value` `spanish` and `label` `Spanish`

### Requirement: Genre options

The API SHALL expose `GET /api/catalog/genres` with at most the 100 tags that have the most stations, counted without broken stations. Each option's `value` SHALL be the tag exactly as the catalogue returns it, and its `label` SHALL be the same text with the first letter in upper case. Empty tags and tags of a single character SHALL be left out. The list SHALL be sorted by `label`, ignoring case.

#### Scenario: Listing genres

- **WHEN** `GET /api/catalog/genres` is requested
- **THEN** the response status is 200
- **AND** it returns at most 100 options, each with a `value` and a `label`, sorted by `label`
- **AND** no option has a `value` shorter than 2 characters

### Requirement: Content type options

The API SHALL expose `GET /api/catalog/content-types`, which returns exactly these four options in this order without contacting Radio Browser: `music` → `Música`, `news` → `Noticias`, `talk` → `Charlas`, `sports` → `Deportes`. Each `value` is also the catalogue tag that identifies that content type.

#### Scenario: Listing content types

- **WHEN** `GET /api/catalog/content-types` is requested
- **THEN** the response status is 200
- **AND** the body contains exactly the options `music`/`Música`, `news`/`Noticias`, `talk`/`Charlas` and `sports`/`Deportes`

#### Scenario: Content types without the external catalogue

- **WHEN** every Radio Browser mirror is unavailable and `GET /api/catalog/content-types` is requested
- **THEN** the response status is 200 with the same four options

### Requirement: Cached option lists

The API SHALL keep the country, language and genre lists in memory for 60 minutes after they are first loaded successfully, and SHALL answer repeated requests within that time without contacting Radio Browser. A failed load SHALL NOT be cached: the next request SHALL try Radio Browser again.

#### Scenario: Repeated request

- **WHEN** `GET /api/catalog/genres` succeeds and is requested again within 60 minutes
- **THEN** the second response contains the same options
- **AND** Radio Browser is not contacted for the second response

#### Scenario: Retrying after a failure

- **WHEN** a request for a list fails because Radio Browser is unavailable, and the catalogue becomes available again
- **THEN** the next request for that list contacts Radio Browser and returns 200

### Requirement: Catalogue unavailable for option lists

When every Radio Browser mirror fails while loading the country, language or genre list, the API SHALL respond with HTTP 502 and a ProblemDetails `detail` of exactly `No se pudo obtener el catálogo de emisoras.`, SHALL log the technical cause, and SHALL keep running.

#### Scenario: Radio Browser unavailable

- **WHEN** every configured mirror fails and `GET /api/catalog/genres` is requested
- **THEN** the response status is 502
- **AND** the `detail` is `No se pudo obtener el catálogo de emisoras.`
