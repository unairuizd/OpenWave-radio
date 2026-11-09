# Spec Delta

## MODIFIED Requirements

### Requirement: Personalización page

The app SHALL provide a `Personalización` page that looks the same in every theme, changing only its colours with the theme's palette. From top to bottom it SHALL show a preview, the theme choice as a grid of cards, the variant choice as a segmented control shown only when the selected theme has more than one palette, the font choice as chips each written in its own font, the text size choice as a five-step segmented control with the selected size's name under it, the `Idioma` choice, and a `Restablecer valores por defecto` button. There SHALL be no save button: every change SHALL be applied and stored as soon as it is selected.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Personalización` with `Moderno` selected
- **THEN** the theme cards are, in this order, `Moderno`, `Tocadiscos`, `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` and `Clásico de rueda`, with `Moderno` marked
- **AND** the variant control offers `Claro` and `Oscuro`
- **AND** the font chips offer `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each displayed in that font
- **AND** the text size control offers `Pequeño`, `Normal`, `Grande`, `Muy grande` and `Enorme`, from smallest to largest
- **AND** every control shows the current theme, variant, font, text size and language

#### Scenario: Theme with a single palette

- **WHEN** the selected theme has only one palette, such as `Tocadiscos`, `Casete portátil`, `Boombox`, `Mesa de DJ` or `MP3`
- **THEN** no variant control is shown
- **AND** the page and the preview use that theme's palette

#### Scenario: Theme with two palettes

- **WHEN** the selected theme is `Radio de válvulas` or `Clásico de rueda`
- **THEN** the variant control offers `Baquelita` and `Nogal`, or `Blanco` and `Negro`
- **AND** the page and the preview use the selected palette

#### Scenario: Changing theme on this page

- **WHEN** the user selects another theme card
- **THEN** the page keeps its layout and its scroll position, and only its colours change

#### Scenario: Restoring the defaults

- **WHEN** the user activates `Restablecer valores por defecto`, in any theme
- **THEN** the theme becomes `Moderno` `Claro`, the font `Ubuntu`, the text size `Normal` and the language `Español` on every page
- **AND** the controls and the preview show those values

### Requirement: Themed pages

`Inicio`, `Reproductor` and `Favoritos` SHALL take their layout and header from the active theme. `Ajustes`, `Personalización`, `Preferencias` and the filter selection pages SHALL keep one shared layout in every theme and change only through palette, font, size and language, as SHALL `Iniciar sesión` and `Crear cuenta`. The bottom bar SHALL be the same in every theme.

#### Scenario: Changing the variant with a session and a station

- **WHEN** a logged-in user with a selected station and saved preferences changes the theme or variant
- **THEN** the themed pages change their look immediately
- **AND** the selected station, the favourites, the session and the saved preferences are unchanged

#### Scenario: Settings pages in two themes

- **WHEN** the user opens `Preferencias` in `Moderno` and then in `Mesa de DJ`
- **THEN** the page has the same layout, texts and controls in both, in the colours of each palette

### Requirement: Adding a theme

A theme SHALL consist of one or more palette files, one layout file and one entry in the theme registry. Adding a theme SHALL require only those three things: no page, view model, service, style or other code SHALL need to change, and the new theme SHALL then appear in the theme choice. The layout SHALL define the header, the station item and the `Inicio`, `Reproductor` and `Favoritos` templates; it SHALL NOT define the settings pages or the bottom bar.

#### Scenario: Registering a third theme

- **WHEN** a developer adds a palette file defining every colour name, a layout file defining the header, the station item and the `Inicio`, `Reproductor` and `Favoritos` templates, and registers them with an id and a display name
- **THEN** the theme appears in `Personalización` as a card and can be selected
- **AND** every function of the app works in it without other code changes, with the shared settings pages and bottom bar in its colours

## ADDED Requirements

### Requirement: Fixed preview in Personalización

The preview SHALL stay at the top of `Personalización`, visible while the options scroll under it. It SHALL show a sample station card with a name, characteristics, a `Reproducir` button and a heart, drawn with the current palette, font, text size and language. The sample SHALL be fixed data: its buttons SHALL NOT play anything or change the favourites.

#### Scenario: Scrolling the options

- **WHEN** the user scrolls down to the text size control
- **THEN** the preview is still visible at the top

#### Scenario: Changing the font

- **WHEN** the user selects the `Caladea` chip
- **THEN** the preview's texts are drawn in Caladea at once

### Requirement: Theme cards

Each theme card SHALL show a small sample of the theme's colours and the theme's display name, and the selected theme's card SHALL be marked. Each card SHALL be a single control whose accessible name is the theme's name, announced as selected when it is.

#### Scenario: Selecting a card

- **WHEN** the user activates the `Boombox` card
- **THEN** `Boombox` is applied at once and its card is marked

### Requirement: Font chips with more

The font choice SHALL show the first three fonts as chips and a `+N` chip, where N is the number of remaining fonts, that shows the rest. When the selected font is not among the first three, its chip SHALL be shown as well.

#### Scenario: Showing every font

- **WHEN** the user activates `+3`
- **THEN** the chips `Cormorant Garamond`, `Recursive` and `Pixelify Sans` are shown too

#### Scenario: Selected font hidden behind more

- **WHEN** the selected font is `Recursive` and the user opens `Personalización`
- **THEN** the `Recursive` chip is shown and marked without activating `+N`
