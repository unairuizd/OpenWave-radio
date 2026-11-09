# Station Preferences Specification

## Purpose

Lets the user save default station filters (country, language, genre and content type) on the device, so `Inicio` shows matching stations without searching each time. The filters work without an account and survive app restarts.

## Requirements

### Requirement: Preferencias page

The app SHALL provide a `Preferencias` page that looks the same in every theme, changing only its colours with the theme's palette. From top to bottom it SHALL show the summary of the filters, a group `Dónde y en qué idioma` with the rows `País` and `Idioma`, a group `Qué quieres escuchar` with the row `Género` and the `Tipo de contenido` chips, and `Restablecer`. Each row SHALL show its icon, its name, the selected value (`Todos` when there is no filter) and `›`, and SHALL open its filter selection page. The options SHALL come from the OpenWave API's country, language, genre and content type lists.

#### Scenario: Opening Preferencias

- **WHEN** the user opens `Preferencias` and the option lists load
- **THEN** the rows `País`, `Idioma` and `Género` show the saved values, or `Todos` when nothing is saved
- **AND** the `Tipo de contenido` chips show the saved type marked, or `Todos`

#### Scenario: Labels of the options

- **WHEN** the user looks at the `Tipo de contenido` chips
- **THEN** they are `Todos`, `Música`, `Noticias`, `Charlas` and `Deportes`

### Requirement: Saving and resetting preferences

Choosing a value in a row or a chip SHALL change only the page's pending choice. While the pending choices differ from the saved ones, the page SHALL show a bar with `Cambios sin guardar`, `Descartar` and `Guardar`. `Guardar` SHALL store the four pending values on the device, storing nothing for `Todos`, hide the bar and show `Preferencias guardadas.` `Descartar` SHALL bring back the saved values and hide the bar. `Restablecer` SHALL set all four to `Todos` and store that immediately. Saved preferences SHALL survive closing and reopening the app.

#### Scenario: Saving preferences

- **WHEN** the user chooses `Spain` in the `País` page, keeps `Todos` for the others, and activates `Guardar`
- **THEN** the page shows `Preferencias guardadas.` and the bar disappears
- **AND** after closing and reopening the app, `Preferencias` shows `Spain` as `País` and `Todos` for the others

#### Scenario: Discarding changes

- **WHEN** the user chooses `Noticias` as `Tipo de contenido` and activates `Descartar`
- **THEN** the chips show the saved type again, the bar disappears and nothing is stored

#### Scenario: Resetting preferences

- **WHEN** the user activates `Restablecer`
- **THEN** the four filters show `Todos` and no bar is shown
- **AND** no preference is stored, so `Inicio` shows no filter bar

#### Scenario: Leaving with unsaved changes

- **WHEN** the user chooses a genre and leaves `Preferencias` without saving
- **THEN** the saved preferences are unchanged and `Inicio` keeps applying them

### Requirement: Saved value missing from the list

If a saved value is not among the options returned by the API, its row SHALL still show that value, its filter selection page SHALL list it as an option and mark it, and saving SHALL keep it unless the user chooses another option.

#### Scenario: Genre no longer in the top list

- **WHEN** the saved genre is `bossanova` and the genre list from the API does not contain it
- **THEN** the `Género` row shows `Bossanova` and its page marks `Bossanova`
- **AND** activating `Guardar` after changing another filter keeps `bossanova` saved

### Requirement: Option lists unavailable

While the option lists load, `Preferencias` SHALL show a loading indicator. If they cannot be loaded, the page SHALL show `No hay conexión con el servidor.` when the API cannot be reached, or `No se pudo obtener el catálogo de emisoras.` when the API reports that the catalogue is unavailable, with a `Reintentar` button that loads them again. A failed load SHALL NOT change or erase the saved preferences, and the app SHALL keep working.

#### Scenario: API stopped

- **WHEN** the API is not running and the user opens `Preferencias`
- **THEN** the page shows `No hay conexión con el servidor.` and `Reintentar`
- **AND** the saved preferences are unchanged and `Inicio` keeps applying them

#### Scenario: Retry succeeds

- **WHEN** the error is shown, the API becomes available, and the user activates `Reintentar`
- **THEN** the selectors load and show the saved values

### Requirement: Preferences stored only on the device

Preferences SHALL be stored only on the device. They SHALL NOT be sent to the database or linked to a user account, SHALL work without logging in, and SHALL NOT be cleared when the user logs in or out.

#### Scenario: Using preferences without an account

- **WHEN** no user is logged in and the user saves preferences
- **THEN** the preferences are saved and applied in `Inicio`

#### Scenario: Logging out

- **WHEN** preferences are saved and the user logs out
- **THEN** `Preferencias` still shows the saved values and `Inicio` still applies them

### Requirement: Filter summary

The top of `Preferencias` SHALL show `Inicio mostrará` followed by the pending filters in one line of words, such as `Emisoras de Spain en Spanish · Jazz`, or `Todas las emisoras` when every filter is `Todos`. The summary SHALL follow the pending choices at once. It SHALL NOT show a number of stations.

#### Scenario: Summary of three filters

- **WHEN** the pending choices are `Spain`, `Spanish`, `Jazz` and `Todos`
- **THEN** the summary reads `Inicio mostrará` and `Emisoras de Spain en Spanish · Jazz`

#### Scenario: No filters

- **WHEN** every filter is `Todos`
- **THEN** the summary reads `Inicio mostrará` and `Todas las emisoras`

### Requirement: Filter selection page

Activating the `País`, `Idioma` or `Género` row SHALL open a page titled with the filter's name. It SHALL show a search box, then `Todos` with `sin filtro`, then `Recientes` when there are recent values, then the full list with each option's label and number of stations. The pending value SHALL be marked with a tick. Choosing an option SHALL make it the pending value and return to `Preferencias`.

#### Scenario: Layout of the País page

- **WHEN** the user opens `País`
- **THEN** the page shows `Buscar país`, `Todos` with `sin filtro`, `Recientes` and the full list under `Todos los países`
- **AND** the `Idioma` and `Género` pages use `Buscar idioma` / `Todos los idiomas` and `Buscar género` / `Todos los géneros`

#### Scenario: Choosing a country

- **WHEN** the user opens `País` and chooses `Austria`
- **THEN** `Preferencias` is shown again with `Austria` in the `País` row and the unsaved changes bar

#### Scenario: Choosing no filter

- **WHEN** the user opens `Género` and chooses `Todos`
- **THEN** the `Género` row shows `Todos`

#### Scenario: Station counts

- **WHEN** the user opens `País`
- **THEN** each country of the list shows how many stations it has, such as `1.204`

### Requirement: Searching a filter list

Typing in the search box SHALL show only the options whose label contains the typed text, ignoring letter case and accents, and SHALL hide `Recientes` while the box is not empty. When no option matches, the page SHALL show `Sin resultados`.

#### Scenario: Searching a country

- **WHEN** the user types `aus` in the `País` page
- **THEN** the list shows `Australia` and `Austria` and no other country

#### Scenario: No match

- **WHEN** the user types a text no option contains
- **THEN** the page shows `Sin resultados`

### Requirement: Recent filter values

Each filter SHALL remember on the device the last values the user saved, newest first, up to five, without `Todos` and without repeats. `Recientes` SHALL list them with the pending one marked. Recent values SHALL survive closing and reopening the app and SHALL NOT be linked to the user account.

#### Scenario: A saved country becomes recent

- **WHEN** the user saves `Spain` as `País` and later opens the `País` page
- **THEN** `Recientes` lists `Spain`

#### Scenario: At most five

- **WHEN** the user has saved six different countries over time
- **THEN** `Recientes` lists the last five, newest first
