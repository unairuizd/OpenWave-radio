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

The `Ajustes` page SHALL open from the header's gear button and SHALL look the same in every theme, changing only its colours with the theme's palette. It SHALL list three entries, `Personalización` first, `Preferencias` second and `Curiosidades` third, each a full-width row with an icon, its title, its description and `›`: `Personalización` with «Cambia el estilo de la app, el idioma, el tipo de letra y su tamaño.», `Preferencias` with «Establece filtros de búsqueda predeterminados: país, idioma de las emisoras, género y tipo.» and `Curiosidades` with «Un dato musical cada día y lo que pasó tal día como hoy.» Activating an entry SHALL open its page, and a back action SHALL return to `Ajustes`. The login state and the `Cerrar sesión` action SHALL stay in `Favoritos`.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Ajustes` and activates `Personalización`
- **THEN** the `Personalización` page is shown
- **WHEN** the user goes back, with the back arrow, the Android back button or the gear
- **THEN** `Ajustes` is shown again

#### Scenario: Opening Preferencias

- **WHEN** the user opens `Ajustes` and activates `Preferencias`
- **THEN** the `Preferencias` page is shown, and going back returns to `Ajustes`

#### Scenario: Opening Curiosidades

- **WHEN** the user opens `Ajustes` and activates `Curiosidades`
- **THEN** the `Curiosidades` page is shown, and going back returns to `Ajustes`

#### Scenario: Same order in every theme

- **WHEN** the user opens `Ajustes` in any of the eight themes
- **THEN** `Personalización` is the first entry, `Preferencias` the second and `Curiosidades` the third, each with its description
- **AND** only the colours differ from one theme to another

### Requirement: Common header

Every themed page SHALL show a header row with the page title in upper case, centred on the full width of the page, and a gear button `Ajustes` on its right edge. The titles SHALL be `INICIO`, `REPRODUCTOR` and `FAVORITOS`. A theme MAY add a house button `Inicio` on the left of the `Reproductor` header. `Ajustes`, `Personalización`, `Preferencias` and the filter selection pages SHALL show the shared settings header in every theme: a back arrow on the left, the page title centred as written (`Ajustes`, `Personalización`, `Preferencias`, `País`, `Idioma`, `Género`) and the gear on the right. `Iniciar sesión` and `Crear cuenta` SHALL keep the platform header with its back action.

#### Scenario: Header in Inicio

- **WHEN** the user opens `Inicio`
- **THEN** the header shows `INICIO` centred on the page and the gear on the right

#### Scenario: House button in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos` and activates the house button
- **THEN** `Inicio` is shown and its bottom bar button is marked

#### Scenario: Header in Personalización

- **WHEN** the user opens `Personalización` in any theme
- **THEN** the header shows the back arrow on the left, `Personalización` centred and the gear on the right

### Requirement: Gear opens Ajustes

The gear SHALL always lead to the main `Ajustes` page. From `Personalización`, `Preferencias` or a filter selection page it SHALL return to `Ajustes` without opening a second `Ajustes` page. On `Ajustes` itself it SHALL do nothing.

#### Scenario: Gear in Preferencias

- **WHEN** the user is in `Preferencias` and activates the gear
- **THEN** the `Ajustes` page is shown
- **AND** going back from it does not show another `Ajustes` page

#### Scenario: Gear in a filter selection page

- **WHEN** the user is in the `País` selection page and activates the gear
- **THEN** the `Ajustes` page is shown

#### Scenario: Gear in Ajustes

- **WHEN** the user is in `Ajustes` and activates the gear
- **THEN** nothing changes

### Requirement: Bottom bar

Every themed page that shows a bottom bar SHALL show the same bar in every theme, fixed at the bottom and clear of the system's gesture and navigation areas: three icon buttons, a house for `Inicio`, a radio for `Reproductor` and a star for `Favoritos`, coloured with the theme's palette. Each button SHALL have an accessible name and, on Windows, a tooltip. The current section's button SHALL be marked with the selected colour, and screen readers SHALL announce which button is selected. On `Ajustes`, `Personalización`, `Preferencias` and the filter selection pages no button SHALL be marked. In `Moderno` and `Clásico de rueda` every themed page shows it; in `Tocadiscos`, `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ` and `MP3` every themed page except `Reproductor` does. The settings pages show it in every theme.

#### Scenario: Marked section

- **WHEN** the user is in `Inicio`
- **THEN** the `Inicio` button is marked and the other two are not

#### Scenario: Same bar in every theme

- **WHEN** the user opens `Favoritos` in `Casete portátil` and then in `Boombox`
- **THEN** both show the same three icons, with `Favoritos` marked, only in the colours of each palette

#### Scenario: No mark in settings

- **WHEN** the user is in `Ajustes`, `Personalización`, `Preferencias` or a filter selection page
- **THEN** no bottom bar button is marked

#### Scenario: Reproductor without a bar

- **WHEN** the user opens `Reproductor` in `Tocadiscos`, `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ` or `MP3`
- **THEN** no bottom bar is shown
- **AND** the house button leads to `Inicio` and the gear to `Ajustes`

#### Scenario: Reproductor with the slim bar

- **WHEN** the user opens `Reproductor` in `Clásico de rueda`
- **THEN** the bottom bar under the wheel shows the three icons with `Reproductor` marked

### Requirement: Navigating with the bottom bar

Activating a bottom bar button SHALL show that section from any themed page, including the settings pages, which are left. Activating the button of the section already shown SHALL do nothing.

#### Scenario: Leaving settings

- **WHEN** the user is in `Personalización` and activates `Favoritos`
- **THEN** `Favoritos` is shown and its button is marked

#### Scenario: Current section

- **WHEN** the user is in `Reproductor` and activates `Reproductor`
- **THEN** nothing changes and playback continues

### Requirement: Back arrow on the settings pages

The back arrow of `Ajustes` SHALL return to the page `Ajustes` was opened from, such as `Inicio`, `Reproductor` or `Favoritos`, with its state kept. The back arrow of `Personalización` and `Preferencias` SHALL return to `Ajustes`, and that of a filter selection page to `Preferencias`. The Android back button SHALL do the same. The arrow SHALL have the accessible name `Atrás`.

#### Scenario: Ajustes opened from Reproductor

- **WHEN** the user opens `Ajustes` with the gear in `Reproductor` and activates the back arrow
- **THEN** `Reproductor` is shown again and playback is unchanged

#### Scenario: Back from Personalización

- **WHEN** the user is in `Personalización` and activates the back arrow
- **THEN** `Ajustes` is shown

#### Scenario: Back from a filter selection page

- **WHEN** the user is in the `Género` selection page and activates the back arrow
- **THEN** `Preferencias` is shown with its unsaved changes kept
