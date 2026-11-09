# Spec Delta

## MODIFIED Requirements

### Requirement: Station list in Inicio

When the station list loads successfully, `Inicio` SHALL show every station returned by the API, one entry per station: in `Moderno` as a grid of station cards with as many columns as fit at the card minimum width (two or more on a phone at the `Normal` text size, one when two cards would be narrower than their minimum) and more columns on wider windows, in `Tocadiscos` as the stack of record sleeves, and in every other theme as its theme specification describes. Without an active search, `Inicio` SHALL request and show the top stations from the catalogue.

#### Scenario: API is running

- **WHEN** the API is running and the user opens `Inicio`
- **THEN** the page lists the stations returned by the API as cards in `Moderno`, as sleeves in `Tocadiscos`, or as the entries of the selected theme
- **AND** each visible card, the focused sleeve's information panel, or each visible entry of the other themes shows the station name

#### Scenario: Loading the list

- **WHEN** the station request is in progress
- **THEN** `Inicio` shows a loading indicator
- **AND** no error message is shown

#### Scenario: Empty catalogue

- **WHEN** the API returns an empty station list
- **THEN** `Inicio` shows `No se encontraron emisoras.`

#### Scenario: Wide window

- **WHEN** the Windows window is widened in `Moderno`
- **THEN** the grid shows more columns, one for every card minimum width (150 units at `Normal`), and every card stays within its width limits

#### Scenario: Up to a hundred stations

- **WHEN** the API returns 100 stations and any theme other than `Moderno` is selected
- **THEN** the user can reach every station and the page stays responsive

#### Scenario: Small window at a large text size

- **WHEN** the Windows window is at its minimum size, `Moderno` is selected and the text size is `Grande` or larger
- **THEN** the grid shows one column of cards, each at least its minimum width, and nothing scrolls sideways
