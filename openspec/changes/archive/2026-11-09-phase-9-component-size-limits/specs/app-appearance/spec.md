# Spec Delta

## MODIFIED Requirements

### Requirement: Usable at every text size

From `Pequeño` to `Enorme`, and at every window size down to the minimum window size, the themed pages and the settings pages SHALL show no clipped controls and no controls pushed off the screen. The text size SHALL also scale the size range of every component that contains text. The content area SHALL scroll vertically when needed while the header and bottom bar stay fixed.

#### Scenario: Enorme on a phone

- **WHEN** `Enorme` is selected on an Android phone and the user opens `Reproductor`
- **THEN** every control can be reached by scrolling the content
- **AND** the header and bottom bar stay visible

#### Scenario: Enorme in the smallest window

- **WHEN** `Enorme` is selected and the Windows window is at its minimum size
- **THEN** every control of every page can be reached by scrolling the content vertically
