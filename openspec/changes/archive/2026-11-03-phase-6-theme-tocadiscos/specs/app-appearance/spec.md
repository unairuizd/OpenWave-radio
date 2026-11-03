# Spec Delta

## MODIFIED Requirements

### Requirement: Personalización page

The app SHALL provide a `Personalización` page with a theme selector listing every registered theme, a variant selector shown only when the selected theme has more than one palette, a font selector that writes each font name in its own font, a text size selector, a preview card with a sample title, body text, caption and button that reflects the current choices, and a `Restablecer valores por defecto` button. There SHALL be no save button: every change SHALL be applied and stored as soon as it is selected.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Personalización` with `Moderno` selected
- **THEN** the theme selector offers `Moderno` and `Tocadiscos`
- **AND** the variant selector offers `Claro` and `Oscuro`
- **AND** the font selector offers `Ubuntu`, `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each displayed in that font
- **AND** the text size selector offers `Pequeño`, `Normal`, `Grande`, `Muy grande` and `Enorme`
- **AND** the selectors show the current theme, variant, font and text size

#### Scenario: Theme with a single palette

- **WHEN** the selected theme has only one palette, such as `Tocadiscos`
- **THEN** no variant selector is shown
- **AND** the preview uses that theme's palette

#### Scenario: Restoring the defaults

- **WHEN** the user activates `Restablecer valores por defecto`, in any theme
- **THEN** the theme becomes `Moderno` `Claro`, the font `Ubuntu` and the text size `Normal` on every page
- **AND** the selectors and the preview show those values

### Requirement: Themes

The app SHALL offer two themes. `Moderno` SHALL have two variants: `Claro`, with a white page background, near-black text and the colours of the former `Blanco`, and `Oscuro`, with a pure black page background (`#000000`), near-white text and the colours of the former `Negro`. `Tocadiscos` SHALL have one palette, a record player look with walnut, cream and brass. Every palette SHALL give every visible element readable contrast.

#### Scenario: Readable Negro theme

- **WHEN** `Moderno` `Oscuro` (the former `Negro`) is selected
- **THEN** page backgrounds are `#000000`, body text is near-white, and the hearts, icons, selection highlight and error text remain clearly visible

#### Scenario: Tocadiscos available

- **WHEN** the user opens the theme selector
- **THEN** `Tocadiscos` can be selected next to `Moderno`

### Requirement: Theme applied immediately

Selecting a theme or a variant SHALL apply it at once to every page, the header, the bottom bar and the platform's native controls (text inputs, selectors and dialogs), without restarting the app and in both directions between themes, including on pages that were already open. The app SHALL NOT follow the operating system's light or dark setting; the default SHALL be `Moderno` `Claro`.

#### Scenario: Switching to Negro

- **WHEN** the user selects the variant `Oscuro` (the former `Negro`)
- **THEN** `Personalización` immediately shows a black background with light text, including its header and bottom bar
- **AND** `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Preferencias`, `Iniciar sesión` and `Crear cuenta` use the dark variant when shown, without restarting the app
- **AND** text inputs and selectors use dark native styling

#### Scenario: Operating system in dark mode

- **WHEN** the operating system uses a dark theme and the user has never chosen a theme
- **THEN** the app uses `Moderno` `Claro`

#### Scenario: Switching between Moderno and Tocadiscos

- **WHEN** the user has opened every page, selects `Tocadiscos`, visits every page, then selects `Moderno` again
- **THEN** every page shows the selected theme each time, including `Reproductor`, without restarting the app

### Requirement: Appearance remembered on the device

The selected theme, variant, font and text size SHALL be stored on the device and applied when the app starts, before the first page is displayed, so the wrong look is never shown first. Selecting a theme with a single palette SHALL keep the stored variant of the other theme. A missing, unknown or corrupt stored value SHALL fall back to its default (`Moderno` `Claro`, `Ubuntu`, `Normal`) without crashing. The appearance SHALL NOT be stored in the database, SHALL work without logging in, and SHALL NOT change when the user logs in or out.

#### Scenario: Restarting the app

- **WHEN** the user selects `Oscuro`, `Caladea` and `Grande`, closes the app and opens it again
- **THEN** the first page shown already uses `Moderno` `Oscuro`, Caladea and the large text size

#### Scenario: Corrupt stored value

- **WHEN** the stored theme is an unknown value
- **THEN** the app starts with `Moderno` `Claro` and does not crash

#### Scenario: Value stored by the previous version

- **WHEN** the device stored the former theme `Negro`
- **THEN** the app starts in `Moderno` `Oscuro`
- **AND** a device that stored `Blanco` starts in `Moderno` `Claro`

#### Scenario: Returning to Moderno

- **WHEN** the user selects `Moderno` `Oscuro`, then `Tocadiscos`, restarts the app and selects `Moderno` again
- **THEN** the app starts in `Tocadiscos` after the restart
- **AND** selecting `Moderno` restores `Oscuro`

### Requirement: Same functions in every theme

A theme SHALL change only the appearance and where its controls are placed. Every function SHALL be available and behave the same in every theme: search, the filter bar, selecting and playing a station, play/stop, retry, the current-station marking, hearts, the `Favoritos` actions, `Cerrar sesión`, `Personalización`, `Preferencias`, and the loading, error and empty states. Changing favourite from `Reproductor` SHALL be possible in every theme: with previous and next buttons in `Moderno` and with the record crate in `Tocadiscos`.

#### Scenario: Functions after a theme change

- **WHEN** the user changes the variant and then searches, plays a station, toggles a heart and removes a favourite
- **THEN** each action behaves exactly as before the change

#### Scenario: Functions in Tocadiscos

- **WHEN** the user selects `Tocadiscos` and then searches, applies and turns off the preference filters, plays and stops a station, retries a failed station, toggles a heart, removes a favourite and logs out
- **THEN** each action has the same result as in `Moderno`

### Requirement: Audio player independent of the theme

The app SHALL use a single audio player that does not belong to any theme layout or page. Changing the theme or variant SHALL never create a second player, crash, stop, restart or otherwise change the playback.

#### Scenario: Playing after changing the variant

- **WHEN** the user changes the variant and then plays a station in `Reproductor`
- **THEN** exactly one stream plays and the status reaches `Reproduciendo`

#### Scenario: Changing theme while a station plays

- **WHEN** a station is playing and the user switches from `Moderno` to `Tocadiscos` and back
- **THEN** the same station keeps playing without interruption and only one stream is heard
