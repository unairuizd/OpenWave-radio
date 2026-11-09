# Component Sizing Specification

## Purpose

Keeps every component of the app within a minimum and a maximum size, in every theme and at every window size, so that resizing the window never gives a component an odd shape, while the five text sizes keep working.

## Requirements

### Requirement: Size range of every component

Every component shown by the app, in every page and every theme, SHALL have a minimum and a maximum width and height. Between its minimum and its maximum a component SHALL follow the space the window gives it; it SHALL NOT be drawn smaller than its minimum or larger than its maximum.

#### Scenario: Narrowing the window

- **WHEN** the user narrows the Windows window step by step on any page of any theme
- **THEN** each component shrinks with the window until it reaches its minimum, and then stays at its minimum

#### Scenario: Widening the window

- **WHEN** the user widens the Windows window step by step
- **THEN** each component grows with the window until it reaches its maximum, and then stays at its maximum

### Requirement: Text components follow the text size

Components that contain text (rows, cards, panels, buttons, keys, chips, list lines, forms, the header and the bottom bar) SHALL have their minimum and maximum multiplied by the factor of the selected text size (`Pequeño` ×0.85, `Normal` ×1.0, `Grande` ×1.2, `Muy grande` ×1.4, `Enorme` ×1.6). Changing the text size SHALL apply the new limits at once.

#### Scenario: Choosing Enorme

- **WHEN** the user selects `Enorme`
- **THEN** the minimum and maximum of every text component become 1.6 times their `Normal` values, without restarting the app

### Requirement: Drawn pieces keep their shape

Drawn pieces (the turntable record and tonearm, record sleeves and crate, the cassette window, reels and J-card, the tuning dial, magic eye and knobs, the speakers and level meters, the jog wheel and pads, the LCD screens and the click wheel) SHALL keep the same minimum and maximum at every text size, and SHALL keep their aspect ratio as they scale between them.

#### Scenario: Cassette on a narrow window

- **WHEN** `Casete portátil` is selected and the user narrows the window
- **THEN** the cassette in `Reproductor` becomes smaller as a whole, keeping its shape, until it reaches its minimum

### Requirement: Text inside drawn pieces

Text shown inside a drawn piece (the cassette label, the tuning dial, the record crate, the LCD lines) SHALL wrap or be cut with `…` instead of making the piece larger, and SHALL stay available in full to screen readers and, for the station name, in the player panel.

#### Scenario: Long name on the cassette label at Enorme

- **WHEN** the text size is `Enorme` and the playing station has a long name
- **THEN** the cassette keeps its size range and shape, and the name on its label is cut with `…`
- **AND** a screen reader reads the full name, and the player panel shows it

### Requirement: Small window with a large text size

When the window is at or near its minimum size and the text size is large, components SHALL stay at their minimum (which, for text components, has grown with the text size) rather than overlap or be cut. When the content of a page no longer fits, the content area SHALL scroll vertically while the header and the bottom bar stay fixed. A page SHALL never scroll sideways. Names and other long texts SHALL wrap or be cut with `…`; no control SHALL be unreachable.

#### Scenario: Minimum window and Enorme

- **WHEN** the Windows window is at its minimum size, the text size is `Enorme` and the user opens `Reproductor` in any theme
- **THEN** every control can be reached by scrolling the content vertically
- **AND** no component overlaps another, and the header and bottom bar stay visible

#### Scenario: Phone and Enorme

- **WHEN** `Enorme` is selected on an Android phone in portrait
- **THEN** every page of every theme can be used by scrolling vertically, and nothing scrolls sideways

### Requirement: Large window

When the window is larger than the components' maximums, drawn pieces and player panels SHALL stay at their maximum and be centred. Station grids and lists SHALL use the extra width for more columns or rows; each card or row SHALL stay within its own limits.

#### Scenario: Maximised window in Casete portátil

- **WHEN** the Windows window is maximised on a large screen in `Casete portátil`
- **THEN** the cassette in `Reproductor` is at its maximum size and centred, with page background around it

#### Scenario: Maximised window in Moderno

- **WHEN** the Windows window is maximised in `Moderno`
- **THEN** `Inicio` shows more card columns, and each card stays within its width limits

### Requirement: Minimum window size on Windows

On Windows the app window SHALL NOT be resizable below about 360 units wide and 600 units high. Android SHALL keep the size of its screen; the limits SHALL be met on an Android phone in portrait.

#### Scenario: Shrinking the window

- **WHEN** the user drags the window's edge to make it as small as possible
- **THEN** the window stops at about 360 × 600

### Requirement: Same limits in every theme

A component that appears in several themes, such as the header, the bottom bar, the station rows and cards, the player panel, the settings rows and the chips, SHALL use the same limits in every theme. A theme SHALL NOT set its own size limits for those shared components.

#### Scenario: Player panel across themes

- **WHEN** the user switches theme with the window at the same size
- **THEN** the shared components keep the same size limits; only their look changes
