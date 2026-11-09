# Proposal

## Why

When the Windows window is resized, some components take odd shapes: the Casete portátil J-card in Favoritos has a fixed height of 280 but a width capped at 720 (and the Reproductor cassette has a fixed window height), so it is squashed on a narrow window and stretched on a wide one, and drawn pieces such as the turntable record, the sleeves, the jog wheel and the speakers simply fill whatever space they get. Other pieces are capped with a different number in each theme (520, 560, 640, 720). The app needs one rule for how far every component may grow or shrink, which still works with the five text sizes.

## What Changes

- **Size limits for every component.** Every component of every page and theme gets a minimum and a maximum size. Between them it stays responsive and follows the window; it never goes below its minimum or above its maximum.
- **Two kinds of component:**
  - **Text components** (rows, cards, panels, buttons, keys, chips, list lines, forms): their minimum and maximum are multiplied by the text-size factor (×0.85 `Pequeño` to ×1.6 `Enorme`), so the text inside always has room.
  - **Drawn pieces** (turntable, record crate, sleeves, cassette window and reels, J-card, tuning dial, magic eye, knob, speakers, level meters, jog wheel, pads, LCD screens, click wheel): their size range is the same at every text size and they keep their shape (a fixed aspect ratio) as they scale. Text inside a drawn piece (the cassette label, the dial, the crate, the LCD lines) wraps or is cut with `…`; the full text stays available to screen readers and in the player panel.
- **Small window with a large text size:** components shrink to their minimum (which, for text components, has grown with the text size). When the page no longer fits, its content scrolls vertically, never sideways, while the header and the bottom bar stay fixed. Long names wrap or are cut with `…`.
- **Large window:** components stop at their maximum. Drawn pieces and player panels are centred; grids and lists use the extra width for more columns or rows (as the Moderno card grid already does).
- **Minimum window size on Windows:** the window cannot be made smaller than a small phone (about 360 × 600). Android keeps its fixed screen; the rules are defined and checked for a phone in portrait.
- **One place for the numbers:** the limits become named size keys shared by all themes, recomputed with the text size like the icon size is today. Themes stop using their own literal maximum widths.

## Capabilities

### New Capabilities
- `component-sizing`: the size limits of the app's components: minimum and maximum per component, which ones follow the text size, keeping the shape of drawn pieces, the behaviour on small and large windows, and the minimum window size on Windows.

### Modified Capabilities
- `app-appearance`: "Usable at every text size" now holds at every window size down to the minimum window, and states that the text size also scales the size range of text components.
- `station-browsing`: the Moderno card grid shows as many columns as fit at the card minimum width, so it can show one column in a small window at a large text size (it was "never fewer than two"). Agreed with the user during implementation.

## Impact

- **App:** `Resources/Styles/Styles.xaml` gets the shared size keys; `AppearanceService` recomputes the text-component keys with the text size; the eight layouts, the shared settings pages and the drawn controls in `Views/Controls/` (`VinylDisc`, `Tonearm`, `SleeveStack`, `RecordCrate`, `CassetteReels`, `TuningDial`, `MagicEye`, `SpeakerCone`, `LevelMeter`, `JogWheel`, `PagedSlots`, `ClickWheel`, `MarqueeLabel`) take their limits from those keys and keep their aspect ratio; the Windows platform code sets the minimum window size.
- **No API, contract or database change.** No new package.
- **Docs:** README (sizes and the manual check list).
