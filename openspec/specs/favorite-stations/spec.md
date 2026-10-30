# Favorite Stations Specification

## Purpose

Stores each user's favourite radio stations in the remote database through the API, keeping a copy of the station data so favourites keep working when the live catalogue changes.

## Requirements

### Requirement: Listing a user's favourites

The API SHALL expose `GET /api/users/{userId}/favorites`. For an existing user it SHALL respond with HTTP 200 and a JSON array of that user's favourite stations in the station format used by the catalogue (`id`, `name`, `streamUrl`, `country`, `language`, `tags`, `logoUrl`), newest favourite first. For an unknown user it SHALL respond with HTTP 404.

#### Scenario: User with favourites

- **WHEN** a user added station A and then station B, and `GET /api/users/{userId}/favorites` is requested
- **THEN** the response status is 200
- **AND** the array contains B first and A second

#### Scenario: User without favourites

- **WHEN** an existing user has no favourites
- **THEN** the response status is 200 with an empty array

#### Scenario: Unknown user

- **WHEN** `GET /api/users/999999/favorites` is requested and no user has that id
- **THEN** the response status is 404
- **AND** the `detail` is `El usuario no existe.`

### Requirement: Adding a favourite

The API SHALL expose `POST /api/users/{userId}/favorites` with a station body. For an existing user and a valid station it SHALL store the favourite and respond with HTTP 201 and the stored station. A station is valid when `id`, `name` and `streamUrl` are non-empty. An invalid station SHALL return HTTP 400 with `Los datos de la emisora no son válidos.`, and an unknown user SHALL return HTTP 404.

#### Scenario: Adding a new favourite

- **WHEN** an existing user posts a valid station that is not yet a favourite
- **THEN** the response status is 201
- **AND** the station appears in that user's favourites list

#### Scenario: Missing stream URL

- **WHEN** a station without `streamUrl` is posted
- **THEN** the response status is 400
- **AND** the `detail` is `Los datos de la emisora no son válidos.`

#### Scenario: Adding for an unknown user

- **WHEN** a valid station is posted for a user id that does not exist
- **THEN** the response status is 404
- **AND** nothing is stored

### Requirement: Favourites keep a copy of the station data

The API SHALL store the station's name, stream URL, logo URL, country, language and tags together with the favourite, and SHALL return favourites from that stored copy without consulting the live catalogue.

#### Scenario: Catalogue unavailable

- **WHEN** a user has favourites and the Radio Browser catalogue cannot be reached
- **THEN** `GET /api/users/{userId}/favorites` still returns the favourites with their names and stream URLs

### Requirement: No duplicate favourites per user

A station SHALL appear at most once in each user's favourites. Adding a station that is already a favourite of that user SHALL return HTTP 409 with a ProblemDetails `detail` of exactly `La emisora ya está en tus favoritas.` and SHALL leave a single stored favourite, including when two identical requests arrive at the same time.

#### Scenario: Adding the same station twice

- **WHEN** the same user posts the same station twice
- **THEN** the first response is 201 and the second is 409
- **AND** the second `detail` is `La emisora ya está en tus favoritas.`
- **AND** only one favourite with that station id exists for that user

#### Scenario: Same station for two users

- **WHEN** two different users add the same station
- **THEN** both requests return 201
- **AND** each user's list contains the station once

### Requirement: Removing a favourite

The API SHALL expose `DELETE /api/users/{userId}/favorites/{stationId}`. When the station is a favourite of that user, it SHALL remove it and respond with HTTP 204. When the user does not exist or the station is not one of their favourites, it SHALL respond with HTTP 404 and `La emisora no está en tus favoritas.` or `El usuario no existe.` as appropriate.

#### Scenario: Removing an existing favourite

- **WHEN** a user deletes a station that is in their favourites
- **THEN** the response status is 204
- **AND** the station no longer appears in their favourites list

#### Scenario: Removing a station that is not a favourite

- **WHEN** a user deletes a station id that is not in their favourites
- **THEN** the response status is 404
- **AND** the `detail` is `La emisora no está en tus favoritas.`

### Requirement: Favourites persisted in the remote database

The API SHALL store favourites in the remote MySQL database, linked to an existing user, so that they remain available after the API restarts.

#### Scenario: Favourites after an API restart

- **WHEN** a user adds favourites, the API is restarted, and the user logs in again
- **THEN** `GET /api/users/{userId}/favorites` returns the same favourites in the same order

### Requirement: Database unavailable for favourites

When the database cannot be reached while listing, adding or removing favourites, the API SHALL respond with HTTP 503 and a ProblemDetails `detail` of exactly `No se pudo acceder a la base de datos.`, and SHALL log the technical cause.

#### Scenario: Database is unreachable

- **WHEN** the database cannot be reached and `GET /api/users/1/favorites` is requested
- **THEN** the response status is 503
- **AND** the `detail` is `No se pudo acceder a la base de datos.`
