## MODIFIED Requirements

### Requirement: Personalización page

The app SHALL provide a `Personalización` page with a theme selector, a variant selector shown only when the selected theme has more than one variant, a font selector that writes each font name in its own font, a text size selector, a preview card with a sample title, body text, caption and button that reflects the current choices, and a `Restablecer valores por defecto` button. There SHALL be no save button: every change SHALL be applied and stored as soon as it is selected.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Personalización`
- **THEN** the theme selector offers `Moderno`
- **AND** because `Moderno` has two variants, a variant selector offers `Claro` and `Oscuro`
- **AND** the font selector offers `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each displayed in that font
- **AND** the text size selector offers `Pequeño`, `Normal`, `Grande`, `Muy grande` and `Enorme`
- **AND** the selectors show the current theme, variant, font and text size

#### Scenario: Theme with a single variant

- **WHEN** the selected theme has only one variant
- **THEN** no variant selector is shown

#### Scenario: Restoring the defaults

- **WHEN** the user activates `Restablecer valores por defecto`
- **THEN** the theme becomes `Moderno` with the variant `Claro`, the font `Ubuntu` and the text size `Normal` on every page
- **AND** the selectors and the preview show those values

### Requirement: Themes

A theme SHALL consist of one or more colour variants (palettes) and one layout for the main pages. The app SHALL offer exactly one theme, `Moderno`, with the variants `Claro` and `Oscuro`. `Claro` SHALL use a white page background with near-black text. `Oscuro` SHALL use a pure black page background (`#000000`) with near-white text. Each variant SHALL give every visible element readable contrast: pages, the tab bar, page headers, station cards, the heart icon, the selection highlight, secondary and error texts, buttons and input controls.

#### Scenario: Readable Negro theme

- **WHEN** `Moderno` `Oscuro` (the colours of the former `Negro` theme) is selected
- **THEN** page backgrounds are `#000000`, body text is near-white, and the heart, selection highlight and error text remain clearly visible

#### Scenario: Former themes are gone

- **WHEN** the user opens the theme selector
- **THEN** `Blanco` and `Negro` are not offered

### Requirement: Theme applied immediately

Selecting a theme or a variant SHALL apply it at once to every page, including the layout of `Inicio`, `Reproductor` and `Favoritos`, the tab bar and the platform's native controls (text inputs, selectors and dialogs), without restarting the app. The app SHALL NOT follow the operating system's light or dark setting; the default SHALL be `Moderno` `Claro`.

#### Scenario: Switching to Negro

- **WHEN** the user selects the variant `Oscuro`, which replaces the former `Negro` theme
- **THEN** `Personalización` immediately shows a black background with light text
- **AND** `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Iniciar sesión`, `Crear cuenta` and the tab bar use the dark colours when shown, without restarting the app
- **AND** text inputs and selectors use dark native styling

#### Scenario: Switching back to Claro

- **WHEN** the user selects `Oscuro` and then `Claro`
- **THEN** every page and the tab bar return to the light colours without restarting the app

#### Scenario: Operating system in dark mode

- **WHEN** the operating system uses a dark theme and the user has never chosen a theme
- **THEN** the app uses `Moderno` `Claro`

### Requirement: Appearance remembered on the device

The selected theme, variant, font and text size SHALL be stored on the device and applied when the app starts, before the first page is displayed, so the wrong theme is never shown first. A missing, unknown or corrupt stored value SHALL fall back to its default (`Moderno` `Claro`, `Ubuntu`, `Normal`) without crashing. A theme stored by an earlier version SHALL be migrated: `Blanco` SHALL become `Moderno` `Claro` and `Negro` SHALL become `Moderno` `Oscuro`. The appearance SHALL NOT be stored in the database, SHALL work without logging in, and SHALL NOT change when the user logs in or out.

#### Scenario: Restarting the app

- **WHEN** the user selects `Moderno` `Oscuro`, `Caladea` and `Grande`, closes the app and opens it again
- **THEN** the first page shown already uses the dark variant, the `Moderno` layout, Caladea and the large text size

#### Scenario: Migrating Negro

- **WHEN** the device stored the theme `Negro` from an earlier version
- **THEN** the app starts in `Moderno` `Oscuro`
- **AND** `Personalización` shows `Moderno` and `Oscuro` selected

#### Scenario: Migrating Blanco

- **WHEN** the device stored the theme `Blanco` from an earlier version
- **THEN** the app starts in `Moderno` `Claro`

#### Scenario: Corrupt stored value

- **WHEN** the stored theme or variant is an unknown value
- **THEN** the app starts with `Moderno` `Claro` and does not crash

### Requirement: No hard-coded appearance values

Pages, layouts, item templates and controls SHALL NOT contain literal colours, font families or font sizes. Every colour SHALL come from the active palette, and every font family and font size from the style resources, so that a change of theme, variant, font or size updates every element that uses them.

#### Scenario: Reviewing the pages

- **WHEN** the app's pages, layouts and styles are reviewed
- **THEN** no page, layout or template sets a colour, font family or font size directly
- **AND** colours are defined only in the palette files, and the font family and sizes only in the style resources

### Requirement: Adding a theme

Adding a new theme SHALL require only its palette files (one per variant) defining the same colour names, one layout file defining the look of `Inicio`, `Reproductor` and `Favoritos` and their station cards, and one new entry in the theme registry. No page, view model, style or other code SHALL need to change, and the new theme SHALL then appear in the theme selector.

#### Scenario: Registering a third theme

- **WHEN** a developer adds a theme alongside `Moderno` (which has two variants): they add a palette file defining every colour name, a layout file and a registry entry with an id, a display name and that palette
- **THEN** the theme appears in `Personalización` and can be selected like `Moderno`
- **AND** because it has one variant, no variant selector is shown for it

## ADDED Requirements

### Requirement: Themed page layouts

`Inicio`, `Reproductor` and `Favoritos` SHALL take their look from the active theme's layout. `Iniciar sesión`, `Crear cuenta`, `Ajustes`, `Preferencias` and `Personalización` SHALL be shared by every theme and change only through the palette, font and text size.

#### Scenario: Pages after a theme change

- **WHEN** the user changes the theme or variant and then opens `Inicio`, `Reproductor` and `Favoritos`
- **THEN** each page shows the active theme's layout and colours
- **AND** the logged-in session, the favourites, the saved preferences and the selected station are unchanged

### Requirement: Functions unchanged in every theme

A theme SHALL change only appearance, never what the user can do. In every layout the user SHALL be able to search, clear the search, use the `Filtros activos` bar with `Desactivar` and `Activar`, select a station, see the selected station highlighted in `Inicio`, add or remove a favourite with the heart, play and stop, retry after an error, log in and out from `Favoritos`, and play and remove favourites. Loading, error and empty states SHALL be shown as today.

#### Scenario: Using Inicio in Moderno

- **WHEN** the user searches for a station, applies and deactivates the saved preferences and taps a station
- **THEN** the results, the filter bar and the selection behave exactly as in the previous version
- **AND** `Reproductor` opens with that station

#### Scenario: Heart while logged out

- **WHEN** a logged-out user activates a heart in `Inicio` or `Reproductor`
- **THEN** `Iniciar sesión` opens, as in the previous version

#### Scenario: States in Moderno

- **WHEN** the station list is loading, fails or is empty
- **THEN** the loading indicator, the error message with `Reintentar`, or the empty message is shown, as in the previous version

### Requirement: Moderno station card grid

In `Moderno`, `Inicio` and `Favoritos` SHALL show stations as a grid of cards. Each card SHALL show the station logo (or the placeholder image), the station name on at most two lines ending with an ellipsis, its secondary details, a play button and a heart button. A card SHALL be at least as tall as it is wide and SHALL grow with the text size.

#### Scenario: Card actions in Inicio

- **WHEN** the user taps a card or its play button in `Inicio`
- **THEN** the station is selected and `Reproductor` opens, as when tapping a station before
- **AND** the heart on the card adds or removes the station from the favourites without selecting it

#### Scenario: Card actions in Favoritos

- **WHEN** the user taps a card or its play button in `Favoritos`
- **THEN** that favourite starts in `Reproductor`
- **AND** the filled heart on the card removes the station from the favourites immediately, without confirmation

### Requirement: Adaptive number of columns

The station grid SHALL use at least two columns and SHALL add a column for about every 180 units of available width.

#### Scenario: Phone width

- **WHEN** the grid is shown on a phone in portrait
- **THEN** it shows two columns

#### Scenario: Wide window

- **WHEN** the Windows window is widened to about 1000 units
- **THEN** the grid shows more columns than on a phone, and the cards keep their actions visible

### Requirement: Moderno player layout

In `Moderno`, `Reproductor` SHALL show a large station artwork with the placeholder fallback, the station name, the country, the state text, the play/stop button, `Reintentar` on error and the heart, in a streaming-app style. It SHALL NOT offer any control that the app did not offer before.

#### Scenario: Playing a station

- **WHEN** the user selects a station in `Inicio`
- **THEN** `Reproductor` shows its artwork, name, country and state, the `Detener` button while playing and the heart
- **AND** no volume, previous/next, slot or equalizer control is shown

### Requirement: Single audio player independent of the layout

The app SHALL use a single audio player that does not belong to any theme layout. Changing the theme or variant SHALL NOT create a second player, SHALL NOT crash and SHALL NOT start or restart audio.

#### Scenario: Changing theme after listening

- **WHEN** the user plays a station, goes to `Personalización`, changes the variant and returns to `Reproductor`
- **THEN** the app does not crash, no audio starts by itself, and play/stop works as before

### Requirement: Themes do not set fonts or text sizes

A theme's palettes and layout SHALL NOT set font families or font sizes. The font and text size chosen by the user SHALL apply in every theme.

#### Scenario: Font kept across themes

- **WHEN** the user selects `Pixelify Sans` and `Grande` and then changes the variant
- **THEN** every page of the new variant still uses Pixelify Sans at the large size

### Requirement: Accessible labels

Every interactive control in the themed pages SHALL have an accessible description in Spanish that names its action, for example `Reproducir` followed by the station name, `Añadir a favoritas`, `Quitar de favoritas` and `Detener`.

#### Scenario: Screen reader on a card

- **WHEN** a screen reader focuses the play button and the heart of a card in `Inicio` for a station that is not a favourite
- **THEN** it reads `Reproducir` followed by the station name, and `Añadir a favoritas`

### Requirement: Minimum touch targets

Every button in the themed pages SHALL have a touch target of at least 48 units on Android and at least 32 units on Windows, even when its drawing is smaller.

#### Scenario: Heart on a card

- **WHEN** the heart of a card is measured on the Android emulator
- **THEN** its tappable area is at least 48 by 48 units

### Requirement: Keyboard use on Windows

On Windows, every button in the themed pages, including each card's play and heart buttons, SHALL be reachable with Tab, SHALL be activated with Enter or Space, and SHALL show a visible focus indication.

#### Scenario: Playing a card with the keyboard

- **WHEN** the user presses Tab until a card's play button has the focus and presses Enter
- **THEN** the focus is visible on that button before Enter is pressed
- **AND** the station is selected and `Reproductor` opens

### Requirement: Text contrast

In every variant, text SHALL have a contrast ratio of at least 4.5:1 against the background it is drawn on, including text on cards, on the selection highlight, on buttons and on the tab bar.

#### Scenario: Secondary text on the selection highlight

- **WHEN** the contrast of secondary text on the selected card is measured in `Moderno` `Claro` and in `Moderno` `Oscuro`
- **THEN** both ratios are at least 4.5:1

### Requirement: Usable at every text size

The themed pages SHALL stay usable at every text size from `Pequeño` to `Enorme`: no text SHALL be clipped vertically, long station names SHALL end with an ellipsis, and every control SHALL stay reachable.

#### Scenario: Enorme in Inicio

- **WHEN** the text size is `Enorme` and `Inicio` shows the grid
- **THEN** every card shows its name, details, play button and heart without text cut in half
