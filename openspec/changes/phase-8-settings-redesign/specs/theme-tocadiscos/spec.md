# Spec Delta

## MODIFIED Requirements

### Requirement: Tocadiscos bottom bar

`Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` in `Tocadiscos` SHALL show the shared icon bottom bar in the Tocadiscos palette, marking the current section as in every theme. `Reproductor` SHALL NOT show the bottom bar; the record crate takes its place.

#### Scenario: Bar in Favoritos

- **WHEN** the user opens `Favoritos` in `Tocadiscos`
- **THEN** the bottom bar shows the icons for `Inicio`, `Reproductor` and `Favoritos` with `Favoritos` marked

#### Scenario: No bar in Reproductor

- **WHEN** the user opens `Reproductor` in `Tocadiscos`
- **THEN** no bottom bar is shown and the record crate is at the bottom of the page

## REMOVED Requirements

### Requirement: Tocadiscos Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the Tocadiscos palette; the record-sleeve cards and the wood panel are no longer drawn.
