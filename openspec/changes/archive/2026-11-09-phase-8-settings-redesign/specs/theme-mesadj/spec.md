# Spec Delta

## MODIFIED Requirements

### Requirement: Spanish booth labels

Every visible label of the theme SHALL be in the app language, including `CARGAR`, `ATRÁS`, `SIGUIENTE` and `EN DIRECTO` in `Español`. With `Español` selected, no English DJ term SHALL be shown.

#### Scenario: Reviewing the labels

- **WHEN** every page of `Mesa de DJ` is reviewed with `Español` selected
- **THEN** no visible label is in English

#### Scenario: Labels in English

- **WHEN** the app is in `English`
- **THEN** the booth labels are shown in English

### Requirement: Mesa de DJ header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show a thin title bar with the title in upper case centred, the gear `Ajustes` on the right and a tag `EN DIRECTO` lit while a station is buffering or playing; in `Reproductor` it SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette, the current section marked; `Reproductor` SHALL NOT show it. The shared settings pages SHALL NOT show the `EN DIRECTO` tag.

#### Scenario: Live tag

- **WHEN** a station starts playing
- **THEN** the `EN DIRECTO` tag lights up in `Inicio`, `Reproductor` and `Favoritos` without blinking

## REMOVED Requirements

### Requirement: Mesa de DJ Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Club` palette; the large pads and the screen panel are no longer drawn.
