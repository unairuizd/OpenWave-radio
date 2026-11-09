# Spec Delta

## MODIFIED Requirements

### Requirement: Casete portátil header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show the title in upper case centred and the gear `Ajustes` on the right; in `Reproductor` the header SHALL also show a house button `Inicio` on the left. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette, the current section marked; `Reproductor` SHALL NOT show it.

#### Scenario: Marked key

- **WHEN** the user opens `Favoritos`
- **THEN** the `Favoritos` icon is marked and screen readers announce it as selected

## REMOVED Requirements

### Requirement: Casete portátil Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Metal azul` palette; the silver keys and the brushed panel are no longer drawn.
