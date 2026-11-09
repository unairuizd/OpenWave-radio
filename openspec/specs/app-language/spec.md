# App Language Specification

## Purpose

Lets the user choose the language the app is shown in, with Spanish as the default and English as the first other language, and makes adding a new language a matter of translations and one registry entry.

## Requirements

### Requirement: Language choice

`Personalización` SHALL offer an `Idioma` choice listing every registered language by its own name, in the order of the registry. The first release SHALL register `Español` and `English`. `Español` SHALL be the default.

#### Scenario: Opening the language choice

- **WHEN** the user opens `Personalización` for the first time
- **THEN** the `Idioma` choice offers `Español` and `English` with `Español` selected

### Requirement: Language applied immediately

Choosing a language SHALL change at once, without restarting the app, every text the app writes: page titles, labels, buttons, messages, states, placeholders, accessible names and tooltips, on every page and in every theme. The current page, the selected station, playback, the session and the saved preferences SHALL be unchanged.

#### Scenario: Switching to English while playing

- **WHEN** a station is playing and the user selects `English` in `Personalización`
- **THEN** the page title reads `PERSONALIZATION` and the other pages show their English texts, such as `HOME`, `PLAYER` and `FAVOURITES`
- **AND** the station keeps playing and the user stays on the same page

#### Scenario: Back to Spanish

- **WHEN** the user selects `Español` again
- **THEN** every page shows its Spanish texts again

### Requirement: Spanish texts are the default texts

Where a requirement of any capability quotes a text the app shows or announces, that quoted text SHALL be the `Español` text. With another language selected, the same element SHALL show that language's equivalent text, with the same behaviour.

#### Scenario: Reading a requirement in English

- **WHEN** a requirement says that `Inicio` shows `No se encontraron emisoras.` and `English` is selected
- **THEN** `Inicio` shows the English equivalent in the same situation

### Requirement: Language remembered on the device

The chosen language SHALL be stored only on the device, SHALL survive closing and reopening the app, and SHALL NOT be linked to the user account or changed by logging in or out. A missing, unknown or corrupt stored value SHALL fall back to `Español`.

#### Scenario: Restarting in English

- **WHEN** the user selects `English` and restarts the app
- **THEN** the app opens in English

#### Scenario: Unknown stored language

- **WHEN** the stored language is not a registered language
- **THEN** the app opens in `Español` without crashing

### Requirement: Station data is not translated

Texts that come from the station catalogue SHALL be shown as the catalogue gives them, in every language: station names, countries, station languages and tags.

#### Scenario: Country of a station in English

- **WHEN** the app is in `English` and a station from Spain is listed
- **THEN** its country reads as the catalogue gives it (for example `Spain`), as in `Español`

### Requirement: Numbers in the app language

Numbers that the app formats itself, such as station counts, SHALL use the number format of the selected language.

#### Scenario: Count in both languages

- **WHEN** a country has 1204 stations
- **THEN** its count reads `1.204` in `Español` and `1,204` in `English`

### Requirement: Adding a language

Adding a language SHALL require only a translation file for the app with every text key, a translation file for the API with its keys, and one entry in the app's language registry with the language code and its own name. No page, view model, layout, control or service SHALL need to change. A text missing from a translation SHALL fall back to its `Español` text.

#### Scenario: Registering a third language

- **WHEN** a developer adds the app and API translation files for a new language and registers it
- **THEN** the language appears in the `Idioma` choice and every page shows its texts

#### Scenario: Missing translation

- **WHEN** a translation file lacks one text
- **THEN** that text is shown in Spanish and the app does not crash

### Requirement: Language sent to the API

Every request the app sends to the OpenWave API SHALL state the selected language, so the API answers its own texts in that language.

#### Scenario: Login error in English

- **WHEN** the app is in `English` and the user logs in with a wrong password
- **THEN** the login page shows the API's message in English
