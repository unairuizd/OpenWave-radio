# Station Preferences Specification

## Purpose

Lets the user save default station filters (country, language, genre and content type) on the device, so `Inicio` shows matching stations without searching each time. The filters work without an account and survive app restarts.

## Requirements

### Requirement: Preferencias page

The app SHALL provide a `Preferencias` page with four single-choice selectors labelled `País`, `Idioma`, `Género` and `Tipo de contenido`. Each selector's first option SHALL be `Todos`, meaning no filter. The other options SHALL come from the OpenWave API's country, language, genre and content type lists, showing each option's label. The page SHALL offer `Guardar` and `Restablecer`.

#### Scenario: Opening Preferencias

- **WHEN** the user opens `Preferencias` and the option lists load
- **THEN** the page shows the four selectors with `Todos` as their first option
- **AND** each selector shows the value saved for it, or `Todos` when nothing is saved

#### Scenario: Labels of the options

- **WHEN** the user opens the `Tipo de contenido` selector
- **THEN** it offers `Todos`, `Música`, `Noticias`, `Charlas` and `Deportes`

### Requirement: Saving and resetting preferences

`Guardar` SHALL store the four selected values on the device, storing nothing for `Todos`, and SHALL show `Preferencias guardadas.` `Restablecer` SHALL set all four selectors to `Todos` and store that immediately. Saved preferences SHALL survive closing and reopening the app.

#### Scenario: Saving preferences

- **WHEN** the user selects `Spain` as `País`, keeps `Todos` for the others, and activates `Guardar`
- **THEN** the page shows `Preferencias guardadas.`
- **AND** after closing and reopening the app, `Preferencias` shows `Spain` as `País` and `Todos` for the others

#### Scenario: Resetting preferences

- **WHEN** the user activates `Restablecer`
- **THEN** the four selectors show `Todos`
- **AND** no preference is stored, so `Inicio` shows no filter bar

### Requirement: Saved value missing from the list

If a saved value is not among the options returned by the API, the matching selector SHALL still show that value as selected, and saving SHALL keep it unless the user chooses another option.

#### Scenario: Genre no longer in the top list

- **WHEN** the saved genre is `bossanova` and the genre list from the API does not contain it
- **THEN** the `Género` selector shows `Bossanova` as selected
- **AND** activating `Guardar` without changing it keeps `bossanova` saved

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
