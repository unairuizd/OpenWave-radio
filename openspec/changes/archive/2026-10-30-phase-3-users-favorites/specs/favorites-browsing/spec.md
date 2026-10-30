# Spec Delta

## Purpose

Lets a logged-in user mark stations as favourites from `Inicio` and `Reproductor`, and list, play and remove them in `Favoritos`, with the favourite state always matching what is stored.

## ADDED Requirements

### Requirement: Favourite toggle on stations

Every station entry in `Inicio` and the selected station in `Reproductor` SHALL show a heart toggle. It SHALL appear filled (♥) when the station is one of the logged-in user's stored favourites, and empty (♡) when it is not or when no user is logged in.

#### Scenario: Favourites loaded after login

- **WHEN** a user with stored favourites logs in and opens `Inicio`
- **THEN** each listed station that is a favourite shows a filled heart
- **AND** every other station shows an empty heart

#### Scenario: Logged out

- **WHEN** no user is logged in
- **THEN** every heart in `Inicio` and `Reproductor` is empty

### Requirement: Toggling a favourite while logged in

When a logged-in user activates an empty heart, the app SHALL add the station to their favourites through the API and fill the heart. Activating a filled heart SHALL remove it and empty the heart. Activating the toggle SHALL NOT select or play the station.

#### Scenario: Adding from Inicio

- **WHEN** a logged-in user activates the empty heart of a station in `Inicio`
- **THEN** the heart becomes filled
- **AND** the station appears in `Favoritos`
- **AND** the current page and playback do not change

#### Scenario: Removing from Reproductor

- **WHEN** a logged-in user activates the filled heart in `Reproductor`
- **THEN** the heart becomes empty
- **AND** the station no longer appears in `Favoritos`

### Requirement: Favourite toggle while logged out

When no user is logged in, activating a heart SHALL open `Iniciar sesión`. If the user then logs in or creates an account, the app SHALL add that station to their favourites and return to the screen where the heart was activated. If the user goes back without logging in, nothing SHALL be added.

#### Scenario: Logging in from a heart

- **WHEN** a logged-out user activates the heart of a station in `Inicio` and then logs in successfully
- **THEN** the app returns to `Inicio`
- **AND** that station's heart is filled and the station is stored as a favourite

#### Scenario: Cancelling the login

- **WHEN** a logged-out user activates a heart and goes back from `Iniciar sesión` without logging in
- **THEN** no favourite is added and the heart stays empty

### Requirement: Consistent favourite state

The heart state SHALL match the stored favourites in every screen. A change made in one screen SHALL be visible in the others without reloading, and if saving a change fails the heart SHALL return to its previous state and the app SHALL show `No se pudo actualizar tus favoritas.`

#### Scenario: Removing in Favoritos

- **WHEN** a station's heart is filled in `Inicio` and the user removes it in `Favoritos`
- **THEN** returning to `Inicio` shows that station's heart empty

#### Scenario: Saving fails

- **WHEN** the API cannot be reached and a logged-in user activates an empty heart
- **THEN** the app shows `No se pudo actualizar tus favoritas.`
- **AND** the heart is empty again
- **AND** the app does not crash

### Requirement: Favoritos list

For a logged-in user, `Favoritos` SHALL list their favourite stations newest first, showing at least the name and country of each. While loading it SHALL show a loading indicator, and when the user has none it SHALL show `Aún no tienes emisoras favoritas.`

#### Scenario: User with favourites

- **WHEN** a logged-in user with favourites opens `Favoritos`
- **THEN** a loading indicator is shown while the list loads
- **AND** the favourites are listed newest first with their names

#### Scenario: User without favourites

- **WHEN** a logged-in user with no favourites opens `Favoritos`
- **THEN** the page shows `Aún no tienes emisoras favoritas.`

### Requirement: Favoritos errors with retry

When the favourites cannot be loaded, `Favoritos` SHALL show a Spanish error and a `Reintentar` button that loads them again. The message SHALL be `No hay conexión con el servidor.` when the API cannot be reached and `No se pudo acceder a la base de datos.` when the API reports that the database is unavailable.

#### Scenario: Database unavailable

- **WHEN** the API answers 503 while loading favourites
- **THEN** `Favoritos` shows `No se pudo acceder a la base de datos.` and `Reintentar`

#### Scenario: Retry succeeds

- **WHEN** the error is shown and the user activates `Reintentar` after the problem is solved
- **THEN** the favourites are listed and the error disappears

### Requirement: Favoritos while logged out

When no user is logged in, `Favoritos` SHALL show `Inicia sesión para ver tus favoritas.` and an `Iniciar sesión` button that opens the login page, instead of redirecting automatically.

#### Scenario: Opening Favoritos logged out

- **WHEN** a logged-out user opens `Favoritos`
- **THEN** the page shows `Inicia sesión para ver tus favoritas.` and `Iniciar sesión`
- **WHEN** the user activates `Iniciar sesión` and logs in
- **THEN** the app returns to `Favoritos` and lists the user's favourites

### Requirement: Playing a favourite

Activating a station in `Favoritos` SHALL select it, open `Reproductor` and start its playback, exactly as selecting a station in `Inicio` does.

#### Scenario: Playing from Favoritos

- **WHEN** a logged-in user taps a station in `Favoritos`
- **THEN** `Reproductor` opens with that station's name and country
- **AND** its playback starts

### Requirement: Removing a favourite from Favoritos

Each station in `Favoritos` SHALL offer a remove action that removes it immediately, without confirmation, and updates the list. If removal fails, the station SHALL stay in the list and the app SHALL show `No se pudo actualizar tus favoritas.`

#### Scenario: Removing a favourite

- **WHEN** the user activates remove on a station in `Favoritos`
- **THEN** the station disappears from the list without a confirmation dialog
- **AND** it is no longer stored as a favourite

#### Scenario: Removing the last favourite

- **WHEN** the user removes their only favourite
- **THEN** `Favoritos` shows `Aún no tienes emisoras favoritas.`
