# App Navigation Specification

## Purpose

Defines the three top-level destinations of the app and how the user moves between them, so every later feature has a stable place to live.

## Requirements

### Requirement: Top-level destinations

The app SHALL present exactly three top-level sections, in this order: `Inicio`, `Reproductor` and `Favoritos`, reachable on Android and Windows from a bottom bar on the themed pages. `Ajustes` SHALL NOT be a section of that bar. A theme MAY leave the bar out of `Reproductor` only if that page offers its own way back to `Inicio` and to `Ajustes`. The app SHALL open on `Inicio`.

#### Scenario: App start

- **WHEN** the app starts
- **THEN** the `Inicio` page is shown
- **AND** the bottom bar shows `Inicio`, `Reproductor` and `Favoritos` in that order, and no `Ajustes` entry

#### Scenario: Switching destinations

- **WHEN** the user selects `Reproductor` or `Favoritos` in the bottom bar
- **THEN** the corresponding page is shown without restarting the app
- **AND** the user can return to any other section from the same bar, or from `Reproductor` with its own buttons when the theme shows no bar there

#### Scenario: Returning to a destination

- **WHEN** the user leaves a section and comes back to it
- **THEN** the section is shown again with its current state, without requiring an app restart

### Requirement: Ajustes page

The `Ajustes` page SHALL open from the header's gear button and SHALL show two cards, `PERSONALIZACIÓN` and `PREFERENCIAS`, one above the other at full width. Activating a card SHALL open its page, and a back action SHALL return to `Ajustes`. The login state and the `Cerrar sesión` action SHALL stay in `Favoritos`.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Ajustes` and activates `PERSONALIZACIÓN`
- **THEN** the `Personalización` page is shown
- **WHEN** the user goes back, with the Android back button or the gear
- **THEN** `Ajustes` is shown again

#### Scenario: Opening Preferencias

- **WHEN** the user opens `Ajustes` and activates `PREFERENCIAS`
- **THEN** the `Preferencias` page is shown, and going back returns to `Ajustes`

### Requirement: Common header

Every themed page SHALL show a header row with the page title in upper case, centred on the full width of the page, and a gear button `Ajustes` on its right edge. The titles SHALL be `INICIO`, `REPRODUCTOR`, `FAVORITOS`, `AJUSTES`, `PERSONALIZACIÓN` and `PREFERENCIAS`. A theme MAY add a house button `Inicio` on the left of the `Reproductor` header. `Iniciar sesión` and `Crear cuenta` SHALL keep the platform header with its back action.

#### Scenario: Header in Inicio

- **WHEN** the user opens `Inicio`
- **THEN** the header shows `INICIO` centred on the page and the gear on the right

#### Scenario: House button in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos` and activates the house button
- **THEN** `Inicio` is shown and its bottom bar button is marked

### Requirement: Gear opens Ajustes

The gear SHALL always lead to the main `Ajustes` page. From `Personalización` or `Preferencias` it SHALL return to `Ajustes` without opening a second `Ajustes` page. On `Ajustes` itself it SHALL do nothing.

#### Scenario: Gear in Preferencias

- **WHEN** the user is in `Preferencias` and activates the gear
- **THEN** the `Ajustes` page is shown
- **AND** going back from it does not show another `Ajustes` page

#### Scenario: Gear in Ajustes

- **WHEN** the user is in `Ajustes` and activates the gear
- **THEN** nothing changes

### Requirement: Bottom bar

Every themed page that shows a bottom bar SHALL show it fixed at the bottom, clear of the system's gesture and navigation areas, with the buttons `Inicio`, `Reproductor` and `Favoritos`, each with a label. The current section's button SHALL be marked, and screen readers SHALL announce which button is selected. On `Ajustes`, `Personalización` and `Preferencias` no button SHALL be marked. In `Moderno` and `Clásico de rueda` every themed page shows it; in `Tocadiscos`, `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ` and `MP3` every themed page except `Reproductor` does.

#### Scenario: Marked section

- **WHEN** the user is in `Inicio`
- **THEN** the `Inicio` button is marked and the other two are not

#### Scenario: No mark in settings

- **WHEN** the user is in `Ajustes`, `Personalización` or `Preferencias`
- **THEN** no bottom bar button is marked

#### Scenario: Reproductor without a bar

- **WHEN** the user opens `Reproductor` in `Tocadiscos`, `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ` or `MP3`
- **THEN** no bottom bar is shown
- **AND** the house button leads to `Inicio` and the gear to `Ajustes`

#### Scenario: Reproductor with the slim bar

- **WHEN** the user opens `Reproductor` in `Clásico de rueda`
- **THEN** the slim bottom bar under the wheel shows `Inicio`, `Reproductor` and `Favoritos` with `Reproductor` marked
### Requirement: Navigating with the bottom bar

Activating a bottom bar button SHALL show that section from any themed page, including the settings pages, which are left. Activating the button of the section already shown SHALL do nothing.

#### Scenario: Leaving settings

- **WHEN** the user is in `Personalización` and activates `Favoritos`
- **THEN** `Favoritos` is shown and its button is marked

#### Scenario: Current section

- **WHEN** the user is in `Reproductor` and activates `Reproductor`
- **THEN** nothing changes and playback continues
