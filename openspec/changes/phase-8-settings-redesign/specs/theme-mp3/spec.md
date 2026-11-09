# Spec Delta

## MODIFIED Requirements

### Requirement: MP3 header and bottom bar

`Inicio`, `Reproductor` and `Favoritos` SHALL show the title in upper case centred on the LCD status line and the gear `Ajustes` on the right; in `Reproductor` it SHALL also show a house button `Inicio` on the left. The status line SHALL also show a decorative battery icon. `Inicio`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` SHALL show the shared icon bottom bar in the theme's palette, the current section marked; `Reproductor` SHALL NOT show it.

#### Scenario: Header in Inicio

- **WHEN** the user opens `Inicio` in `MP3`
- **THEN** the LCD status line shows `INICIO` centred and the gear on the right
- **AND** the bottom bar shows the three icons with `Inicio` marked

## REMOVED Requirements

### Requirement: MP3 Ajustes and settings pages

**Reason**: `Ajustes`, `Personalización` and `Preferencias` now have one shared layout in every theme that changes only its colours, with `Personalización` listed first.
**Migration**: The settings pages follow the `app-navigation`, `app-appearance` and `station-preferences` requirements in the `Azul` palette; the LCD menu and the LCD-framed panel are no longer drawn.
