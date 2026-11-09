# Design

## Context

See proposal.md (Why). Current state that shapes the approach:

- Sizes are set in three different ways today:
  - **Fixed numbers in the layouts**: for example the Casete Favoritos J-card is `HeightRequest="280"` inside a `MaximumWidthRequest="720"`, the Reproductor J-card is 170 high in a 560 maximum and the cassette window is 64 high, the Mesa de DJ jog wheel is 240×240, and the Boombox speakers are 150×150.
  - **Drawn controls that fill the space they get**: `VinylDisc`, `Tonearm`, `SleeveStack`, `JogWheel`, `SpeakerCone`, `MagicEye` and `TuningKnob` compute their parts from their own `Width`/`Height` and have no limit.
  - **One-off caps per theme**: Rueda's device is at most 520 wide, MP3's LCD at most 640, and Radio de válvulas' panel at most 560.
- `Styles.xaml` already has a few layout keys (`MinTouchTarget` 48, `CardMinWidth` 170, `CardSpacing`, `PlayerArtworkMaxSize` 280). `AppearanceService.ApplyScale` already recomputes `IconButtonSize` from the text size, and `AdaptiveSpanBehavior` turns the Moderno card grid's width into columns.
- Several themed templates put their drawn piece in a `*` row of a `Grid`, so the piece's height comes from the window's height. Inside a `ScrollView` a `*` row has no height, so these rows have to stop depending on the height.
- The decisions in this design were agreed with the user before planning: grow and scroll; a phone-sized window minimum; centred pieces with lists adding columns; drawn pieces keep their shape; both limits scale; only text components scale; shared keys; Android phone in portrait.

## Goals / Non-Goals

**Goals:**
- One table of named limits in `Styles.xaml`, used by every theme, with the text-component limits recomputed with the text size.
- Drawn pieces sized from the available **width**, clamped between their limits, with a fixed aspect ratio, so they no longer depend on the window's height.
- The content of every page in a vertical scroll area, so a small window or a large text size can always be scrolled; the header and bar stay outside it.
- A minimum window size on Windows.

**Non-Goals:**
- Changing what each theme shows, its colours or its animations.
- Landscape or tablet layouts on Android (not locked, not specifically checked).
- Automatic text shrinking: text is never made smaller than the chosen text size.

## Decisions

### Shared size keys

`Resources/Styles/Styles.xaml` gets one block of `x:Double` keys named `<Component>MinWidth`, `MaxWidth`, `MinHeight` and `MaxHeight` (and `<Piece>Ratio` for drawn pieces). Layouts and pages read them with `DynamicResource` on `MinimumWidthRequest`, `MaximumWidthRequest`, `MinimumHeightRequest` and `MaximumHeightRequest`. Literal sizes and theme-only caps are removed.
*Alternative:* each layout keeps its own numbers. Rejected by the user ("Shared size keys"), and it is how the inconsistencies arose.

### Text-component limits follow the text size

A static table `ComponentSizes` (in `Services/`) lists the `Normal` value of every text-component key. `AppearanceService.ApplyScale` writes `value × scale` for each one, exactly as it does for `IconButtonSize`. Drawn-piece keys are plain constants in `Styles.xaml` and are never rescaled.

### Aspect-ratio host for drawn pieces

A small layout `Views/Controls/AspectBox` (one class, no package):
- It takes `Ratio`, `MinWidth` and `MaxWidth`.
- When measured, it takes the available width, clamps it between `MinWidth` and `MaxWidth`, sets its height to `width / Ratio`, and centres itself horizontally.
- Each drawn piece (or the group that forms it, such as the turntable with its tonearm, or the cassette window with its reels and label) sits inside one `AspectBox`.

The controls that today read their own `Height` (`VinylDisc`, `Tonearm`, `SleeveStack`, `JogWheel`, `SpeakerCone`, `MagicEye`, `TuningKnob`, `CassetteReels`, `ClickWheel`) keep computing their parts from their size, which the box now gives them with the right shape. `ClickWheel` loses its fixed 220.

*Alternative:* set width and height on each piece in code from `SizeChanged`. That would repeat the same arithmetic in every control, and the result could still be squashed by a `*` row.

### Pages scroll vertically

Each themed template and each settings page has the same structure:
- header (`Auto`);
- `ScrollView` (`Orientation="Vertical"`), holding all the content;
- bottom bar (`Auto`).

Rows that were `*` inside the content become `Auto`. Lists that already virtualise (`CollectionView`, `CursorList`, `PagedSlots`) and scroll by themselves get a maximum height from their key, so they never sit inside an unbounded scroll. A page never scrolls horizontally: maximum widths keep everything within the window.

### Text components: SizeLimit and adaptive columns

Text components are hosted by `Views/Controls/SizeLimit` (the counterpart of `AspectBox` for content whose height depends on its text). It clamps its child's width between two keys and centres it. Its minimum never exceeds the space it actually gets, so the window always wins and nothing scrolls sideways. `FillHeight` passes the available height to a list that scrolls by itself. Grids of cards built with a wrapping `FlexLayout` use `Behaviors/AdaptiveColumnsBehavior`, which picks the number of equal columns from the width and `StationCardMinWidth` (one column in a small window at a large text size). Settings titles are one line, cut with `…`; bars of buttons stack their message above the buttons.

### Shared pieces in every theme

- Theme headers take `ChromeMinHeight`/`ChromeMaxHeight`; their titles are one line, cut with `…`. Reproductor headers that draw decorations above the title (the Boombox handle) take only the minimum.
- The filter bar of every theme is a wrapping row: the text and its button side by side when they fit, the button under the text when not.
- Messages inside drawn pieces (the crate, the preset slots) wrap inside the piece.
- Favourites pieces that used to sit in a fixed bottom row (Tocadiscos crate, Casete side A, Mesa de DJ pads, Boombox memories) are now the last item of the Reproductor scroll, so they never take the height the player needs in a small window.
- A row of a fixed number of keys (the five Radio de válvulas presets) is one component: the keys share its width and their height follows `KeyMinHeight`/`KeyMaxHeight`, so a key's own minimum width does not apply inside the row.
- Where a drawn piece and controls share a row (the Radio de válvulas dial with the magic eye and the knob), the row wraps so the piece keeps its minimum.

### Large windows

Drawn pieces and player panels have `HorizontalOptions="Center"` and stop at their maximum width. Grids keep using `AdaptiveSpanBehavior` (more columns). Rows and list lines stretch up to their own maximum width (`StationRowMaxWidth`) and are centred beyond it.

### Minimum window size on Windows

In `App.CreateWindow`, `window.MinimumWidth = 360` and `window.MinimumHeight = 600` (MAUI `Window` properties). This takes effect on Windows only; Android ignores them.

### Proposed limits (Normal text size, device-independent units)

These numbers come from the current sizes and are for review. The values of text components are multiplied by the text-size factor; drawn pieces are not.

| Key group | Kind | Min W | Max W | Height / ratio |
|---|---|---|---|---|
| Header, bottom bar | text | window | window | 56 – 80 high |
| Station card (Moderno grid) | text | 150 | 220 | content |
| Station row / list line / browser row | text | 280 | 720 | 48 – 96 high |
| Player information panel | text | 280 | 560 | content |
| Settings content column (rows, chips, filter list) | text | 300 | 720 | rows 48 – 120 high |
| Login and register forms | text | 280 | 480 | content |
| Keys, pads labels, rubber buttons, LCD lines | text | 48 | 160 | 48 – 72 high |
| Turntable (record + tonearm) | drawn | 200 | 420 | 1.15 : 1 |
| Big sleeve (Tocadiscos Inicio) | drawn | 160 | 300 | 1 : 1 (the stack with the two smaller sleeves below: 0.69 : 1) |
| Record crate | drawn | 300 | 640 | 2.4 : 1 |
| Cassette (Reproductor window, reels, label) | drawn | 280 | 560 | 1.6 : 1 |
| J-card (Casete, Reproductor and Favoritos) | drawn | 300 | 640 | 2 : 1 (the whole card: title, lines and flip row) |
| Tuning dial | drawn | 300 | 640 | 3 : 1 |
| Magic eye, knobs | drawn | 56 | 96 | 1 : 1 |
| Speaker (Boombox) | drawn | 110 | 220 | 1 : 1 |
| Level meter (Mesa de DJ) | drawn | 24 | 60 | 1 : 4 (the Boombox meter is a fixed 84 × 120 panel) |
| Jog wheel (Mesa de DJ) | drawn | 200 | 360 | 1 : 1 |
| Pads grid (4 × 2, Mesa de DJ) | drawn | 300 | 640 | 2 : 1 |
| Memories grid (3 × 2, Boombox) | drawn | 300 | 640 | 3 : 1 |
| LCD screen (MP3) | drawn | 300 | 640 | 4 : 3 |
| Device screen (Clásico de rueda) | drawn | 300 | 520 | 4 : 3 |
| Click wheel | drawn | 180 | 260 | 1 : 1 |

Small parts inside a component (logo thumbnails, icons, the player artwork, the Boombox handle, front speakers and meter, the power knob, the jog play button) keep one fixed size (minimum = maximum) written in the layout; only component sizes are keys.

Two keys complete the table: `ListMaxHeight` (560, text) bounds the height of every list that scrolls by itself inside the page scroll, and `PagePadding` (32) is the width the page padding takes, used to cap minimum widths at the window.

Where a theme puts a drawn piece beside a panel (Tocadiscos sleeves and panel, Boombox speakers and display, Mesa de DJ jog wheel and information), the pair is placed so that it stacks vertically when the two minimums do not fit side by side; otherwise the page would scroll sideways.

The Casete J-cards change from fixed-height strips (280 high up to 720 wide, 170 high up to 560 wide) to a 2 : 1 card, and the cassette keeps the shape of a real cassette (1.6 : 1) with reels drawn from its size. This fixes the stretched look the user reported.

## Risks / Trade-offs

- [Moving `*` rows into a scroll area can make a page taller than before on a big window] → Drawn pieces have a maximum, so a maximised window shows the same page with space around it, not a taller one; checked theme by theme.
- [Lists inside a scroll area] → Every virtualised list gets a maximum height key, so it scrolls itself and the page scrolls around it, never two unbounded scrolls.
- [At `Enorme` on the minimum window, text components can be wider than the window] → Their maximum is applied after the window limit: a text component's minimum width is capped at the window width less the page padding, and its text wraps. This is the one place where the window wins over a minimum, so nothing ever scrolls sideways.
- [Proposed numbers may look wrong in a theme] → They are in one file; each group of tasks ends with a visual check at the minimum window, a maximised window and a phone, at `Normal` and `Enorme`.

## Migration Plan

No data migration. Stored appearance values are unchanged. Rollback is reverting the change.

## Assumptions

None beyond the decisions agreed with the user. The proposed numbers in the table above are for the user to review before or during implementation.
