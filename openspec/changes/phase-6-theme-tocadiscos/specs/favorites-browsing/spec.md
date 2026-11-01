# Spec Delta

## MODIFIED Requirements

### Requirement: Favourite toggle on stations

Every station shown in `Inicio` and the selected station in `Reproductor` SHALL have a heart toggle: on each card in `Moderno`, and next to the name in the information panel in `Tocadiscos`. It SHALL appear as a filled heart when the station is one of the logged-in user's stored favourites, and as an outline heart when it is not or when no user is logged in. Its accessible name SHALL be `Quitar de favoritas` when filled and `Añadir a favoritas` when outlined.

#### Scenario: Favourites loaded after login

- **WHEN** a user with stored favourites logs in and opens `Inicio`
- **THEN** each listed station that is a favourite shows a filled heart
- **AND** every other station shows an outline heart

#### Scenario: Logged out

- **WHEN** no user is logged in
- **THEN** every heart in `Inicio` and `Reproductor` is an outline heart

### Requirement: Favoritos list

For a logged-in user, `Favoritos` SHALL show their favourite stations newest first, in the same presentation as `Inicio`: cards in a grid in `Moderno`, the sleeve stack in `Tocadiscos`. While loading it SHALL show a loading indicator, and when the user has none it SHALL show `Aún no tienes emisoras favoritas.` The logged-in email and `Cerrar sesión` SHALL be shown in one compact row directly under the header.

#### Scenario: User with favourites

- **WHEN** a logged-in user with favourites opens `Favoritos`
- **THEN** a loading indicator is shown while the list loads
- **AND** the favourites are shown newest first with their names and filled hearts
- **AND** the email and `Cerrar sesión` are shown above them

#### Scenario: User without favourites

- **WHEN** a logged-in user with no favourites opens `Favoritos`
- **THEN** the page shows `Aún no tienes emisoras favoritas.`

### Requirement: Playing a favourite

Playing a station from `Favoritos` SHALL behave exactly as selecting a station in `Inicio` does in the same theme: in `Moderno` a card or its play button selects it, opens `Reproductor` and starts its playback; in `Tocadiscos` the focused sleeve starts it and the page stays on `Favoritos`.

#### Scenario: Playing from Favoritos

- **WHEN** a logged-in user taps a station card in `Favoritos` in `Moderno`
- **THEN** `Reproductor` opens with that station's name and country
- **AND** its playback starts

#### Scenario: Playing from Favoritos in Tocadiscos

- **WHEN** a logged-in user presses the focused sleeve in `Favoritos` in `Tocadiscos`
- **THEN** its playback starts and `Favoritos` stays on screen

### Requirement: Removing a favourite from Favoritos

The filled heart of a station in `Favoritos` (on its card, or in the information panel in `Tocadiscos`) SHALL remove the station immediately, without confirmation, and update the list. If removal fails, the station SHALL stay in the list and the app SHALL show `No se pudo actualizar tus favoritas.`

#### Scenario: Removing a favourite

- **WHEN** the user activates the filled heart of a station in `Favoritos`
- **THEN** the station disappears from the list without a confirmation dialog
- **AND** it is no longer stored as a favourite

#### Scenario: Removing the last favourite

- **WHEN** the user removes their only favourite
- **THEN** `Favoritos` shows `Aún no tienes emisoras favoritas.`
