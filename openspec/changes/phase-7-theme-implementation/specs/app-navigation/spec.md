# Spec Delta

## MODIFIED Requirements

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
