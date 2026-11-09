# Station Browsing Specification

## Purpose

Lets the user browse, search and select real stations in Inicio and understand what the app is doing while the list loads, when a search is empty or when the backend cannot be reached.

## Requirements

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

### Requirement: Error state with retry

When the station request fails, `Inicio` SHALL show an error message in Spanish and a `Reintentar` button, and the app SHALL remain usable without crashing. If the API cannot be reached, the message SHALL be `No hay conexión con el servidor.` If the API reports that the catalogue is unavailable, the message SHALL be `No se pudo obtener el catálogo de emisoras.` Activating `Reintentar` SHALL repeat the last request, including the current search term when one is active.

#### Scenario: API is stopped

- **WHEN** the API is stopped and the user opens `Inicio`
- **THEN** the page shows `No hay conexión con el servidor.`
- **AND** it shows a `Reintentar` button
- **AND** the app does not crash and the other destinations remain reachable

#### Scenario: Catalogue is unavailable

- **WHEN** the API returns HTTP 502 for the station request
- **THEN** `Inicio` shows `No se pudo obtener el catálogo de emisoras.`
- **AND** it shows a `Reintentar` button

#### Scenario: Retry succeeds

- **WHEN** the error state is shown and the user activates `Reintentar` while the API is running
- **THEN** the last request is sent again
- **AND** the error message disappears and the station list is shown

### Requirement: Selecting a station

Selecting a station in `Inicio` SHALL select it and start its stream, stopping the previous one. In `Moderno`, tapping a station card or its play button SHALL also open `Reproductor`. In `Tocadiscos`, pressing the focused sleeve SHALL play the station and stay in `Inicio`, and pressing it again while it is the current station SHALL toggle play and stop. In every other theme, the play action its theme specification names SHALL behave as the focused sleeve does in `Tocadiscos`.

#### Scenario: Selecting a station

- **WHEN** the user taps a station card or its play button in `Moderno`
- **THEN** `Reproductor` is shown
- **AND** it displays the name and country of that station
- **AND** playback of that station starts

#### Scenario: Selecting a station in Tocadiscos

- **WHEN** the user presses the focused sleeve of a station that is not playing in `Tocadiscos`
- **THEN** playback of that station starts
- **AND** `Inicio` stays on screen and its information panel shows `Cargando…` and then `Reproduciendo`

#### Scenario: Selecting a station in another theme

- **WHEN** the user activates the play action of a station that is not playing in `Radio de válvulas`, `Casete portátil`, `Boombox`, `Mesa de DJ`, `MP3` or `Clásico de rueda`
- **THEN** playback of that station starts and `Inicio` stays on screen
- **AND** activating the same action again while it plays stops it
### Requirement: Station search by name

`Inicio` SHALL provide a search input that takes the remaining width of its row and SHALL send the entered name to the OpenWave API, together with the saved preference filters while they are applied. The search SHALL run when the user submits the input from the keyboard or activates the search icon button `Buscar`. A clear icon button `Limpiar búsqueda` SHALL clear the input and restore the top stations, still filtered by the applied preferences. The app SHALL NOT call Radio Browser or any other catalogue host directly.

#### Scenario: Searching for a station

- **WHEN** the user enters `jazz` and submits the search
- **THEN** `Inicio` requests the station list from the OpenWave API with the `jazz` name filter
- **AND** the list shows the stations returned for that search

#### Scenario: Clearing the search

- **WHEN** the user activates the clear button
- **THEN** the search input is empty
- **AND** `Inicio` requests and shows the top stations again

#### Scenario: No search results

- **WHEN** no preference filter is applied and the search returns no stations
- **THEN** `Inicio` shows `No se encontraron emisoras.`
- **AND** the search input remains available

### Requirement: Selected station is identifiable

`Inicio` SHALL visibly mark the station currently selected in the app: in `Moderno` with the card highlight, in `Tocadiscos` with the peeking disc and outline on its sleeve, and in every other theme with the mark its theme specification describes. The mark SHALL move when another station is selected and SHALL be restored when `Inicio` is shown again. `Reproductor` SHALL identify the same station by showing its name and country.

#### Scenario: Highlighting the selected station

- **WHEN** the user selects a station in `Inicio`
- **THEN** that station's list entry is visually distinguished from the other entries
- **AND** `Reproductor` shows the same station's name and country

#### Scenario: Moving the highlight

- **WHEN** the user selects a different station
- **THEN** the highlight moves to the new station's entry
- **AND** the previous entry is no longer highlighted

#### Scenario: Returning to Inicio

- **WHEN** the user leaves `Inicio` and returns to it
- **THEN** the currently selected station is still highlighted if it is present in the list
### Requirement: Saved preferences as default filters

While at least one preference is saved and preferences are applied, every station request from `Inicio` SHALL include the saved country, language, genre and content type, each only when it is set, together with the typed name. Preferences SHALL be applied by default every time the app starts. When the saved preferences change, `Inicio` SHALL reload its list the next time it is shown. The preferences SHALL NOT affect `Favoritos` or `Reproductor`.

#### Scenario: Country saved

- **WHEN** the user saves the country `Spain` in `Preferencias` and then opens `Inicio`
- **THEN** `Inicio` reloads its list
- **AND** every listed station is from Spain

#### Scenario: Searching with preferences

- **WHEN** the genre `jazz` is saved and the user searches for `radio` in `Inicio`
- **THEN** the request includes the name `radio` and the genre `jazz`
- **AND** every listed station's name contains `radio` and it has the `jazz` tag

#### Scenario: Other pages are not filtered

- **WHEN** preferences are applied and the user opens `Favoritos` or plays a station in `Reproductor`
- **THEN** `Favoritos` lists every favourite and `Reproductor` plays the selected station, regardless of the preferences

### Requirement: Active filters bar

When at least one preference is saved and preferences are applied, `Inicio` SHALL show a bar above the station list that reads `Filtros activos`, followed by the labels of the saved values separated by ` · ` (for example `Spain · Spanish · Jazz · Música`), and offers `Desactivar`. When no preference is saved, no bar SHALL be shown.

#### Scenario: Filters active

- **WHEN** the country `Spain` and the content type `news` are saved and `Inicio` is shown
- **THEN** the bar shows `Filtros activos` with the summary `Spain · Noticias` and a `Desactivar` button

#### Scenario: No saved preferences

- **WHEN** every preference is `Todos`
- **THEN** `Inicio` shows no filter bar

### Requirement: Turning preference filters off and on

`Desactivar` SHALL stop applying the saved preferences in `Inicio` until the app is closed, SHALL reload the list without them, and SHALL NOT change the saved values. While preferences are not applied, the bar SHALL read `Preferencias desactivadas` and SHALL offer `Activar`, which applies them again and reloads the list.

#### Scenario: Turning the filters off

- **WHEN** the user activates `Desactivar`
- **THEN** `Inicio` reloads the list without the preference filters
- **AND** the bar shows `Preferencias desactivadas` and an `Activar` button
- **AND** `Preferencias` still shows the saved values

#### Scenario: Filters active again after a restart

- **WHEN** the user activated `Desactivar`, closes the app and opens it again
- **THEN** `Inicio` applies the saved preferences and the bar shows `Filtros activos`

#### Scenario: Turning the filters back on

- **WHEN** the bar shows `Preferencias desactivadas` and the user activates `Activar`
- **THEN** `Inicio` reloads the list with the saved preferences and the bar shows `Filtros activos`

### Requirement: No stations match the preferences

When preferences are applied and the request returns no stations, `Inicio` SHALL show `No se encontraron emisoras con tus preferencias.` with a `Desactivar` action that behaves like the bar's `Desactivar`.

#### Scenario: Preferences match nothing

- **WHEN** preferences are applied and the API returns an empty list
- **THEN** `Inicio` shows `No se encontraron emisoras con tus preferencias.` and `Desactivar`
- **WHEN** the user activates `Desactivar`
- **THEN** `Inicio` reloads the list without the preference filters

### Requirement: Station cards

In `Moderno`, each station card SHALL be taller than it is wide and show, from top to bottom: the station logo (or the fallback image when it is missing or fails), the name (bold, at most two lines, ending with `…`), the characteristics (country · language, then up to three tags; at most two lines, ending with `…`), and a row with a play button on the left and a heart on the right.

#### Scenario: Card content

- **WHEN** a station from Spain in Spanish with the tags `pop`, `rock`, `news` and `talk` is listed
- **THEN** its card shows the logo or fallback, the name, `Spain · Spanish` and the tags `pop`, `rock` and `news`, a play button and a heart

#### Scenario: Heart on a card

- **WHEN** a logged-in user activates the heart of a card
- **THEN** the favourite is added or removed as for any heart
- **AND** the station is not selected and playback does not change
