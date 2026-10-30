# App Appearance Specification

## Purpose

Lets the user choose the app's theme, font family and text size, applied immediately to every screen and remembered on the device, and keeps every colour, font and size in one replaceable place so new themes can be added easily.

## Requirements

### Requirement: Personalización page

The app SHALL provide a `Personalización` page with a theme selector, a font selector that writes each font name in its own font, a text size selector, a preview card with a sample title, body text, caption and button that reflects the current choices, and a `Restablecer valores por defecto` button. There SHALL be no save button: every change SHALL be applied and stored as soon as it is selected.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Personalización`
- **THEN** the theme selector offers `Blanco` and `Negro`
- **AND** the font selector offers `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each displayed in that font
- **AND** the text size selector offers `Pequeño`, `Normal`, `Grande`, `Muy grande` and `Enorme`
- **AND** the three selectors show the current theme, font and text size

#### Scenario: Restoring the defaults

- **WHEN** the user activates `Restablecer valores por defecto`
- **THEN** the theme becomes `Blanco`, the font `Ubuntu` and the text size `Normal` on every page
- **AND** the selectors and the preview show those values

### Requirement: Themes

The app SHALL offer exactly two themes. `Blanco` SHALL use a white page background with near-black text. `Negro` SHALL use a pure black page background (`#000000`) with near-white text. Each theme SHALL give every visible element readable contrast: pages, the tab bar, page headers, station cards, the heart icon, the selection highlight, secondary and error texts, buttons and input controls.

#### Scenario: Readable Negro theme

- **WHEN** `Negro` is selected
- **THEN** page backgrounds are `#000000`, body text is near-white, and the heart, selection highlight and error text remain clearly visible

### Requirement: Theme applied immediately

Selecting a theme SHALL apply it at once to every page, the tab bar and the platform's native controls (text inputs, selectors and dialogs), without restarting the app. The app SHALL NOT follow the operating system's light or dark setting; the default theme SHALL be `Blanco`.

#### Scenario: Switching to Negro

- **WHEN** the user selects `Negro`
- **THEN** `Personalización` immediately shows a black background with light text
- **AND** `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Iniciar sesión`, `Crear cuenta` and the tab bar use the black theme when shown, without restarting the app
- **AND** text inputs and selectors use dark native styling

#### Scenario: Operating system in dark mode

- **WHEN** the operating system uses a dark theme and the user has never chosen a theme
- **THEN** the app uses `Blanco`

### Requirement: Font family

The app SHALL bundle the fonts `Ubuntu` (the default), `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each with its licence text, and SHALL offer exactly these six. Selecting a font SHALL apply it at once to all text in the app, on every page.

#### Scenario: Choosing Pixelify Sans

- **WHEN** the user selects `Pixelify Sans`
- **THEN** the preview and every page's labels, buttons and inputs use Pixelify Sans without restarting the app

### Requirement: Text size

The app SHALL scale its four text sizes (caption 12, body 16, subtitle 18, title 24) by the selected preset: `Pequeño` ×0.85, `Normal` ×1.0, `Grande` ×1.2, `Muy grande` ×1.4 and `Enorme` ×1.6. Selecting a preset SHALL resize the text on every page at once. Icons drawn as text, such as the heart, SHALL scale with it.

#### Scenario: Choosing Enorme

- **WHEN** the user selects `Enorme`
- **THEN** body text on every page becomes about 26 units and titles about 38 units, without restarting the app

### Requirement: Appearance remembered on the device

The selected theme, font and text size SHALL be stored on the device and applied when the app starts, before the first page is displayed, so the wrong theme is never shown first. A missing, unknown or corrupt stored value SHALL fall back to its default (`Blanco`, `Ubuntu`, `Normal`) without crashing. The appearance SHALL NOT be stored in the database, SHALL work without logging in, and SHALL NOT change when the user logs in or out.

#### Scenario: Restarting the app

- **WHEN** the user selects `Negro`, `Caladea` and `Grande`, closes the app and opens it again
- **THEN** the first page shown already uses the black theme, Caladea and the large text size

#### Scenario: Corrupt stored value

- **WHEN** the stored theme is an unknown value
- **THEN** the app starts with `Blanco` and does not crash

### Requirement: No hard-coded appearance values

Pages, item templates and controls SHALL NOT contain literal colours, font families or font sizes. Every colour, font family and font size SHALL come from named resources defined only in the theme and style resource files, so that a change of theme, font or size updates every element that uses them.

#### Scenario: Reviewing the pages

- **WHEN** the app's pages and styles are reviewed
- **THEN** no page or template sets a colour, font family or font size directly
- **AND** colours are defined only in the theme files, and the font family and sizes only in the style resources

### Requirement: Adding a theme

Adding a new theme SHALL require only a new theme resource file that defines the same colour names and one new entry in the theme registry. No page, style or other code SHALL need to change, and the new theme SHALL then appear in the theme selector.

#### Scenario: Registering a third theme

- **WHEN** a developer adds a theme file defining every colour name and registers it with an id and a display name
- **THEN** the theme appears in `Personalización` and can be selected like `Blanco` and `Negro`
