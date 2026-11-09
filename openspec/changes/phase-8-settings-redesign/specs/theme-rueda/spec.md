# Spec Delta

## MODIFIED Requirements

### Requirement: Clásico de rueda header and bottom bar

The screen's title bar of `Inicio`, `Reproductor` and `Favoritos` SHALL show the page title in upper case centred, a play-state glyph on the left and the gear `Ajustes` on the right. Every themed page, including `Reproductor`, SHALL show the shared icon bottom bar in the theme's palette, under the wheel where there is one, with the current section marked.

#### Scenario: Bar in Reproductor

- **WHEN** the user opens `Reproductor` in `Clásico de rueda`
- **THEN** the bottom bar is shown under the wheel with the `Reproductor` icon marked

## REMOVED Requirements

### Requirement: Clásico de rueda Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Blanco` or `Negro` palette; the screen menu and the screen-framed content are no longer drawn.
