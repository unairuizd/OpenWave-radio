# Spec Delta

## MODIFIED Requirements

### Requirement: Common header

Every themed page SHALL show a header row with the page title in upper case, centred on the full width of the page, and a gear button `Ajustes` on its right edge. The titles SHALL be `INICIO`, `REPRODUCTOR` and `FAVORITOS`. A theme MAY add a house button `Inicio` on the left of the `Reproductor` header. `Ajustes`, `Personalización`, `Preferencias`, `Curiosidades` and the filter selection pages SHALL show the shared settings header in every theme: a back arrow on the left, the page title centred as written (`Ajustes`, `Personalización`, `Preferencias`, `Curiosidades`, `País`, `Idioma`, `Género`) and the gear on the right. `Iniciar sesión` and `Crear cuenta` SHALL show the same shared header without the gear (the back arrow and the title only) and SHALL NOT show the platform navigation bar, so each has a single back arrow drawn with the theme's colours, font and text size.

#### Scenario: Header in Inicio

- **WHEN** the user opens `Inicio`
- **THEN** the header shows `INICIO` centred on the page and the gear on the right

#### Scenario: House button in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos` and activates the house button
- **THEN** `Inicio` is shown and its bottom bar button is marked

#### Scenario: Header in Personalización

- **WHEN** the user opens `Personalización` in any theme
- **THEN** the header shows the back arrow on the left, `Personalización` centred and the gear on the right

#### Scenario: Header in Iniciar sesión

- **WHEN** the user opens `Iniciar sesión` in any theme
- **THEN** the header shows the back arrow on the left and `Iniciar sesión` centred, with no gear and no platform bar above it
- **AND** the arrow, the title and the divider use that theme's colours, font and text size

#### Scenario: Header in Crear cuenta

- **WHEN** the user opens `Crear cuenta`
- **THEN** the header shows the back arrow on the left and `Crear cuenta` centred, with no gear and no platform bar

### Requirement: Back arrow on the settings pages

The back arrow of `Ajustes` SHALL return to the page `Ajustes` was opened from, such as `Inicio`, `Reproductor` or `Favoritos`, with its state kept. The back arrow of `Personalización`, `Preferencias` and `Curiosidades` SHALL return to `Ajustes` when opened from it, that of a filter selection page to `Preferencias`, that of `Iniciar sesión` to the page that opened it, and that of `Crear cuenta` to `Iniciar sesión`. The Android back button SHALL do the same. The arrow SHALL have the accessible name `Atrás`. Every page of the app SHALL show either the bottom bar or a header with this back arrow.

#### Scenario: Ajustes opened from Reproductor

- **WHEN** the user opens `Ajustes` with the gear in `Reproductor` and activates the back arrow
- **THEN** `Reproductor` is shown again and playback is unchanged

#### Scenario: Back from Personalización

- **WHEN** the user is in `Personalización` and activates the back arrow
- **THEN** `Ajustes` is shown

#### Scenario: Back from a filter selection page

- **WHEN** the user is in the `Género` selection page and activates the back arrow
- **THEN** `Preferencias` is shown with its unsaved changes kept

#### Scenario: Back from Iniciar sesión

- **WHEN** the user opened `Iniciar sesión` from `Favoritos` and activates the back arrow without logging in
- **THEN** `Favoritos` is shown again, logged out

#### Scenario: Back from Crear cuenta

- **WHEN** the user is in `Crear cuenta` and activates the back arrow
- **THEN** `Iniciar sesión` is shown with the email typed so far kept

#### Scenario: Android back button on the account pages

- **WHEN** the user presses the Android back button in `Crear cuenta`
- **THEN** `Iniciar sesión` is shown, and pressing it again returns to the page that opened the login flow

#### Scenario: No page without an exit

- **WHEN** the user visits each page of the app in any theme
- **THEN** each one shows the bottom bar or a header with the back arrow
