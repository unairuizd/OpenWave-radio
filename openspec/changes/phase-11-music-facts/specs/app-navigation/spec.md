# Spec Delta

## MODIFIED Requirements

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
