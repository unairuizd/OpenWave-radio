# App Appearance Specification

## Purpose

Lets the user choose the app's theme, font family and text size, applied immediately to every screen and remembered on the device, and keeps every colour, font and size in one replaceable place so new themes can be added easily.

## Requirements

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

### Requirement: Themes

The app SHALL offer eight themes. `Moderno` SHALL have two variants: `Claro`, with a white page background, near-black text and the colours of the former `Blanco`, and `Oscuro`, with a pure black page background (`#000000`), near-white text and the colours of the former `Negro`. `Tocadiscos` SHALL have one palette, a record player look with walnut, cream and brass. `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` and `Clásico de rueda` SHALL each be defined by their own theme specification. Every palette SHALL give every visible element readable contrast.

#### Scenario: Readable Negro theme

- **WHEN** `Moderno` `Oscuro` (the former `Negro`) is selected
- **THEN** page backgrounds are `#000000`, body text is near-white, and the hearts, icons, selection highlight and error text remain clearly visible

#### Scenario: Tocadiscos available

- **WHEN** the user opens the theme selector
- **THEN** `Tocadiscos` can be selected next to `Moderno`

#### Scenario: Every theme available

- **WHEN** the user selects each of the eight themes in turn
- **THEN** every themed page shows that theme's look, and the app keeps working in each one
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

#### Scenario: Cycling through every theme

- **WHEN** the user has opened every page and selects each of the eight themes in turn, visiting `Inicio`, `Reproductor` and `Favoritos` after each
- **THEN** every page shows the selected theme each time without restarting the app
### Requirement: Font family

The app SHALL bundle the fonts `Ubuntu` (the default), `Dosis`, `Caladea`, `Cormorant Garamond`, `Recursive` and `Pixelify Sans`, each with its licence text, and SHALL offer exactly these six. Selecting a font SHALL apply it at once to all text in the app, on every page.

#### Scenario: Choosing Pixelify Sans

- **WHEN** the user selects `Pixelify Sans`
- **THEN** the preview and every page's labels, buttons and inputs use Pixelify Sans without restarting the app

### Requirement: Text size

The app SHALL scale its four text sizes (caption 12, body 16, subtitle 18, title 24) by the selected preset: `Pequeño` ×0.85, `Normal` ×1.0, `Grande` ×1.2, `Muy grande` ×1.4 and `Enorme` ×1.6. Selecting a preset SHALL resize the text on every page at once. Icons, such as the hearts and the header and bottom bar icons, SHALL scale with it.

#### Scenario: Choosing Enorme

- **WHEN** the user selects `Enorme`
- **THEN** body text on every page becomes about 26 units and titles about 38 units, without restarting the app
- **AND** the icons become larger too

### Requirement: Appearance remembered on the device

The selected theme, variant, font and text size SHALL be stored on the device and applied when the app starts, before the first page is displayed, so the wrong look is never shown first. Every palette SHALL be either the light or the dark palette of its theme. Selecting a theme SHALL keep the user's light or dark choice when that theme has a palette of the same kind; selecting a theme with a single palette SHALL keep the stored variant. A missing, unknown or corrupt stored value SHALL fall back to its default (`Moderno` `Claro`, `Ubuntu`, `Normal`) without crashing. The appearance SHALL NOT be stored in the database, SHALL work without logging in, and SHALL NOT change when the user logs in or out.

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

#### Scenario: Light or dark kept across themes

- **WHEN** the user selects `Moderno` `Oscuro` and then `Radio de válvulas`
- **THEN** `Radio de válvulas` opens with `Nogal`
- **AND** selecting `Clásico de rueda` then opens it with `Negro`, and selecting `Moderno` again restores `Oscuro`

#### Scenario: Light choice in a two-palette theme

- **WHEN** the user selects `Clásico de rueda` `Blanco`, then `Boombox`, then `Moderno`
- **THEN** `Moderno` opens with `Claro`

#### Scenario: Unknown stored variant

- **WHEN** the stored variant is unknown for the stored theme
- **THEN** the theme opens with its first palette and the app does not crash
### Requirement: No hard-coded appearance values

Pages, layouts, templates and controls SHALL NOT contain literal colours, font families or font sizes. Every colour SHALL come from the palette files, and every font family and font size from the style resources, so that a change of theme, variant, font or size updates every element that uses them.

#### Scenario: Reviewing the pages

- **WHEN** the app's pages, layouts and styles are reviewed
- **THEN** no page, layout or template sets a colour, font family or font size directly
- **AND** colours are defined only in the palette files, and the font family and sizes only in the style resources

### Requirement: Adding a theme

A theme SHALL consist of one or more palette files, one layout file and one entry in the theme registry. Adding a theme SHALL require only those three things: no page, view model, service, style or other code SHALL need to change, and the new theme SHALL then appear in the theme choice. The layout SHALL define the header, the station item and the `Inicio`, `Reproductor` and `Favoritos` templates; it SHALL NOT define the settings pages or the bottom bar.

#### Scenario: Registering a third theme

- **WHEN** a developer adds a palette file defining every colour name, a layout file defining the header, the station item and the `Inicio`, `Reproductor` and `Favoritos` templates, and registers them with an id and a display name
- **THEN** the theme appears in `Personalización` as a card and can be selected
- **AND** every function of the app works in it without other code changes, with the shared settings pages and bottom bar in its colours

### Requirement: Themed pages

`Inicio`, `Reproductor` and `Favoritos` SHALL take their layout and header from the active theme. `Ajustes`, `Personalización`, `Preferencias` and the filter selection pages SHALL keep one shared layout in every theme and change only through palette, font, size and language, as SHALL `Iniciar sesión` and `Crear cuenta`. The bottom bar SHALL be the same in every theme.

#### Scenario: Changing the variant with a session and a station

- **WHEN** a logged-in user with a selected station and saved preferences changes the theme or variant
- **THEN** the themed pages change their look immediately
- **AND** the selected station, the favourites, the session and the saved preferences are unchanged

#### Scenario: Settings pages in two themes

- **WHEN** the user opens `Preferencias` in `Moderno` and then in `Mesa de DJ`
- **THEN** the page has the same layout, texts and controls in both, in the colours of each palette

### Requirement: Same functions in every theme

A theme SHALL change only the appearance and where its controls are placed. Every function SHALL be available and behave the same in every theme: search, the filter bar, selecting and playing a station, play/stop, retry, the current-station marking, hearts, the `Favoritos` actions, `Cerrar sesión`, `Personalización`, `Preferencias`, and the loading, error and empty states. Changing favourite from `Reproductor` SHALL be possible in every theme: with previous and next buttons in `Moderno`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` and `Clásico de rueda`, with the record crate in `Tocadiscos`, and with the preset keys in `Radio de válvulas`.

#### Scenario: Functions after a theme change

- **WHEN** the user changes the variant and then searches, plays a station, toggles a heart and removes a favourite
- **THEN** each action behaves exactly as before the change

#### Scenario: Functions in Tocadiscos

- **WHEN** the user selects `Tocadiscos` and then searches, applies and turns off the preference filters, plays and stops a station, retries a failed station, toggles a heart, removes a favourite and logs out
- **THEN** each action has the same result as in `Moderno`

#### Scenario: Functions in every new theme

- **WHEN** the user selects any of `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` or `Clásico de rueda` and then searches, applies and turns off the preference filters, plays and stops a station, retries a failed station, toggles a heart, changes favourite from `Reproductor`, removes a favourite and logs out
- **THEN** each action has the same result as in `Moderno`
### Requirement: Audio player independent of the theme

The app SHALL use a single audio player that does not belong to any theme layout or page. Changing the theme or variant SHALL never create a second player, crash, stop, restart or otherwise change the playback.

#### Scenario: Playing after changing the variant

- **WHEN** the user changes the variant and then plays a station in `Reproductor`
- **THEN** exactly one stream plays and the status reaches `Reproduciendo`

#### Scenario: Changing theme while a station plays

- **WHEN** a station is playing and the user switches from `Moderno` to `Tocadiscos` and back
- **THEN** the same station keeps playing without interruption and only one stream is heard

#### Scenario: Cycling every theme while a station plays

- **WHEN** a station is playing and the user selects each of the eight themes in turn
- **THEN** the same station keeps playing without interruption and only one stream is heard
### Requirement: Themes never set fonts or text sizes

Palettes and layouts SHALL NOT choose font families or text sizes. All text SHALL use the font and size chosen in `Personalización`.

#### Scenario: Font and size in a themed page

- **WHEN** the user selects `Pixelify Sans` and `Enorme`
- **THEN** the header, bottom bar labels, cards and player texts use Pixelify Sans at the enlarged sizes

### Requirement: Accessible controls

Every interactive control on the themed pages SHALL have a Spanish accessible name (for example `Ajustes`, `Buscar`, `Limpiar búsqueda`, `Reproducir` followed by the station name, `Añadir a favoritas`, `Quitar de favoritas`, `Emisora favorita anterior`, `Emisora favorita siguiente`, `Detener`). Each SHALL offer a touch target of at least 48 units on Android and 32 pixels on Windows.

#### Scenario: Screen reader on a station card

- **WHEN** a screen reader focuses the buttons of a station card that is not a favourite
- **THEN** it announces `Reproducir` followed by the station name, and `Añadir a favoritas`

### Requirement: Keyboard use on Windows

On Windows, the header, the bottom bar, the station cards' buttons and every other button SHALL be reachable with the keyboard and activated with Enter or Space, and the focused control SHALL be visibly indicated.

#### Scenario: Playing with the keyboard

- **WHEN** the user moves with the keyboard to a card's play button and presses Enter
- **THEN** `Reproductor` opens and the station plays

### Requirement: Text contrast

In every variant, every text colour SHALL have a contrast ratio of at least 4.5:1 with the background it is drawn on.

#### Scenario: Checking Moderno

- **WHEN** the text and background colours of `Moderno` `Claro` and `Oscuro` are measured
- **THEN** every pair is at least 4.5:1

### Requirement: Usable at every text size

From `Pequeño` to `Enorme`, and at every window size down to the minimum window size, the themed pages and the settings pages SHALL show no clipped controls and no controls pushed off the screen. The text size SHALL also scale the size range of every component that contains text. The content area SHALL scroll vertically when needed while the header and bottom bar stay fixed.

#### Scenario: Enorme on a phone

- **WHEN** `Enorme` is selected on an Android phone and the user opens `Reproductor`
- **THEN** every control can be reached by scrolling the content
- **AND** the header and bottom bar stay visible

#### Scenario: Enorme in the smallest window

- **WHEN** `Enorme` is selected and the Windows window is at its minimum size
- **THEN** every control of every page can be reached by scrolling the content vertically

### Requirement: Theme animations follow the playback state

Animations of any theme (spinning records and reels, pulsing speakers, level meters, glowing lamps, scrolling text) SHALL only show the current playback state and SHALL never start, stop or change playback. They SHALL run only while their page is visible and SHALL stop when it is hidden. A page shown while a station plays SHALL show the playing state at once.

#### Scenario: Leaving an animated page

- **WHEN** a station plays in `Reproductor` with an animation running and the user opens `Inicio`
- **THEN** the animation in `Reproductor` stops and the audio keeps playing

#### Scenario: Opening a page while playing

- **WHEN** the user opens `Reproductor` while a station plays
- **THEN** its animated parts already show the playing state, without a start-up transition

### Requirement: Reduced motion in every theme

When the operating system asks for reduced motion (Windows animations turned off, or Android's animator duration scale at 0), every theme SHALL replace its movements with instant changes and SHALL keep showing the playback state by other means, such as colour, position or text.

#### Scenario: Animations turned off

- **WHEN** Windows animations are turned off and a station plays in `Boombox`
- **THEN** the speakers do not pulse and the level meter does not move
- **AND** the page still shows that the station is playing

### Requirement: Decorative theme parts

Parts of a theme that only decorate (speaker grilles, antennas, scales, meters, lamps, reels, the battery icon) SHALL NOT be focusable, SHALL be hidden from screen readers, and SHALL NOT look like a control that does nothing. Every part that looks pressable SHALL work.

#### Scenario: Screen reader in Boombox

- **WHEN** a screen reader moves through `Reproductor` in `Boombox`
- **THEN** it reaches the transport keys, the heart and the memories, and skips the speakers and the level meter

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
