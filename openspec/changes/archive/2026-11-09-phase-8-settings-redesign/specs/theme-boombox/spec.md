# Spec Delta

## MODIFIED Requirements

### Requirement: Boombox header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show the title in upper case centred under a chrome trim line and the gear `Ajustes` on the right; in `Reproductor` the header SHALL also show a house button `Inicio` on the left and a decorative carrying handle above it. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette, the current section marked; `Reproductor` SHALL NOT show it.

#### Scenario: Marked button

- **WHEN** the user opens `Inicio`
- **THEN** the `Inicio` icon is marked and the other two are not

## REMOVED Requirements

### Requirement: Boombox Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Cromado` palette; the rubber buttons and the chrome-bordered panel are no longer drawn.
