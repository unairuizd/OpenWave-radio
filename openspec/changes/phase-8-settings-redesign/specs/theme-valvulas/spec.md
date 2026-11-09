# Spec Delta

## MODIFIED Requirements

### Requirement: Radio de válvulas header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show a wooden header strip with the page title in upper case centred and the gear `Ajustes` on the right; in `Reproductor` it SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette; `Reproductor` SHALL NOT show it.

#### Scenario: Header in Reproductor

- **WHEN** the user opens `Reproductor` in `Radio de válvulas`
- **THEN** the header shows the house button, `REPRODUCTOR` centred and the gear
- **AND** no bottom bar is shown

#### Scenario: Bar in Inicio

- **WHEN** the user opens `Inicio` in `Radio de válvulas`
- **THEN** the bottom bar shows the three icons with `Inicio` marked, in the palette's colours

## REMOVED Requirements

### Requirement: Radio de válvulas Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Baquelita` or `Nogal` palette; the ivory keys and the cloth panel are no longer drawn.
