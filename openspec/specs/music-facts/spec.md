# Music Facts Specification

## Purpose

Gives the user a short music fun fact every day, keeps the facts of the last two weeks at hand, and adds what happened on today's date in music history, served by the API in the app language.

## Requirements

### Requirement: One fact per calendar day

The API SHALL hold a curated list of music facts, each with the same identifier and text in `Español` and `English`, and SHALL assign exactly one fact to each calendar day (UTC) with a fixed rule: the fact at position `(days since 2026-01-01) mod (number of facts)`. The same day SHALL always give the same fact, for every user and on every request.

#### Scenario: Same fact all day

- **WHEN** the daily facts are requested twice on the same UTC day
- **THEN** both responses give the same fact for today

#### Scenario: Next day

- **WHEN** the daily facts are requested on the following UTC day
- **THEN** today's fact is the next one in the list, and yesterday's fact is the one served the day before

#### Scenario: Wrap-around

- **WHEN** more days than facts have passed since 2026-01-01
- **THEN** the list starts again from its first fact without repeating a fact twice in a row

### Requirement: Daily facts endpoint

The API SHALL expose `GET /api/facts/daily` with an optional `days` query parameter (default 15, allowed 1 to 31). It SHALL return, in the language of the request, the fact of today and of the `days − 1` previous days, newest first, each with its date, plus the `Tal día como hoy` items for today. A `days` value outside the range SHALL be answered with 400 and a `ProblemDetails` whose `detail` is in the request language.

#### Scenario: Fifteen days in Spanish

- **WHEN** `GET /api/facts/daily?days=15` is sent without `Accept-Language`
- **THEN** it answers 200 with 15 facts in Spanish, the first dated today and the last dated 14 days ago

#### Scenario: English

- **WHEN** `GET /api/facts/daily` is sent with `Accept-Language: en`
- **THEN** every fact text is the English text of the same fact identifiers

#### Scenario: Days out of range

- **WHEN** `GET /api/facts/daily?days=0` is sent
- **THEN** it answers 400 with a `ProblemDetails` whose `detail` is a Spanish message

### Requirement: Tal día como hoy from Wikipedia

For today's date, the API SHALL query the public Wikimedia "On this day" feed for the request language (`events` and `births`), identifying itself with a User-Agent, SHALL keep only the items whose text contains a music keyword of that language, matched as whole words, and SHALL return at most three of them, each with its year, its text and the link to the Wikipedia article.

#### Scenario: Music items today

- **WHEN** the Wikipedia feed for today has music-related events or births in the request language
- **THEN** the response lists up to three of them with year, text and article link

#### Scenario: Keyword inside another word

- **WHEN** an event text contains `Pope` or `disband` but no music keyword as a whole word
- **THEN** it is not returned as a music item

### Requirement: Wikipedia never blocks the facts

The API SHALL cache the `Tal día como hoy` result per date and language until the end of the UTC day, and SHALL never let a Wikipedia failure fail the endpoint: when Wikipedia cannot be reached within the API's timeout, answers an error, or has no music item, the list SHALL be empty and the facts SHALL still be returned with status 200.

#### Scenario: Wikipedia unreachable

- **WHEN** the Wikimedia feed does not answer within the API's timeout
- **THEN** the endpoint still answers 200 with the facts and an empty `Tal día como hoy` list

#### Scenario: Cached for the day

- **WHEN** the endpoint is called again on the same day and language
- **THEN** Wikipedia is not queried again and the same items are returned

### Requirement: Daily fact only for logged-in users

The daily fact, its history and the `Tal día como hoy` items SHALL be shown only while a user is logged in. The app SHALL request them from the API only while logged in, SHALL load them right after a login, and SHALL forget them on logout. While logged out, the strip and the `Curiosidades` page SHALL show `Inicia sesión para ver el dato del día.` and a way to log in, instead of redirecting automatically.

#### Scenario: Logging in loads the fact

- **WHEN** a logged-out user logs in and returns to `Inicio`
- **THEN** the strip shows today's fact without restarting the app

#### Scenario: Logging out forgets the facts

- **WHEN** a logged-in user activates `Cerrar sesión` in `Favoritos` and opens `Inicio` or `Curiosidades`
- **THEN** the strip and the page show `Inicia sesión para ver el dato del día.` and no fact

### Requirement: Dato del día strip in Inicio

`Inicio` SHALL show, in every theme, under the search and filter area and above the station list, a `Dato del día` strip: a music icon, the caption `DATO DEL DÍA`, the first line of today's fact cut with `…`, and `›`. It SHALL follow the theme's colours, font and text size, keep the station row size limits, and be one control, with the accessible name `Dato del día` plus the fact, that opens `Curiosidades` (the login page while logged out, showing the log-in hint).

#### Scenario: Strip in every theme

- **WHEN** a logged-in user opens `Inicio` in any of the eight themes with the API running
- **THEN** the strip shows today's fact between the filter area and the station list, in that theme's look

#### Scenario: Opening Curiosidades from the strip

- **WHEN** a logged-in user activates the strip
- **THEN** the `Curiosidades` page opens, and going back returns to `Inicio`

#### Scenario: Strip while logged out

- **WHEN** no user is logged in and the user opens `Inicio`
- **THEN** the strip reads `Inicia sesión para ver el dato del día.`
- **WHEN** the user activates it
- **THEN** the login page opens, and after a successful login `Inicio` shows the strip with today's fact

#### Scenario: Strip while loading or without connection

- **WHEN** the facts are still loading
- **THEN** the strip shows `Cargando…` in place of the fact
- **WHEN** the API cannot be reached
- **THEN** the strip shows `Sin conexión` and still opens `Curiosidades`

### Requirement: Curiosidades page

The app SHALL provide a `Curiosidades` page, the same in every theme (only the palette changes), with a header, the settings column width limits and the bottom bar. While logged in, from top to bottom it SHALL show: today's fact in full with its date; the `Tal día como hoy en la música` block; and the previous 14 facts, newest first, each with its date. Dates SHALL be written in the app language. While logged out it SHALL show only the log-in hint and an `Iniciar sesión` button.

#### Scenario: Curiosidades while logged out

- **WHEN** no user is logged in and the user opens `Curiosidades` from `Ajustes`
- **THEN** the page shows `Inicia sesión para ver el dato del día.` and `Iniciar sesión`, and no request is sent to the API
- **WHEN** the user logs in from that button
- **THEN** the app returns to `Curiosidades`, which loads and shows the facts

#### Scenario: Today and the history

- **WHEN** a logged-in user opens `Curiosidades` with the API running
- **THEN** the page shows today's fact first, then the `Tal día como hoy` block, then 14 earlier facts with their dates, newest first

#### Scenario: Dates in English

- **WHEN** the app is in `English`
- **THEN** every date on the page is written in English

#### Scenario: Enorme in the smallest window

- **WHEN** `Enorme` is selected and the Windows window is at its minimum size
- **THEN** every item of the page can be reached by scrolling and nothing scrolls sideways

### Requirement: Tal día como hoy block in the page

The block SHALL list the items the API returns, each with its year, its text and a `Ver en Wikipedia` link that opens the article in the device browser, followed by the attribution `Fuente: Wikipedia (CC BY-SA)`. When the API returns no item, the block SHALL show `Hoy no tenemos ninguna efeméride musical.` instead of being hidden.

#### Scenario: Items with links

- **WHEN** the API returns two items for today
- **THEN** the block shows both, with year and text, and each `Ver en Wikipedia` opens its article outside the app

#### Scenario: No items today

- **WHEN** the API returns no item
- **THEN** the block shows `Hoy no tenemos ninguna efeméride musical.` and the facts below are unaffected

### Requirement: Facts load and error states

The app SHALL request the daily facts from the API once per login and reuse the result for the strip and the page; `Curiosidades` SHALL offer `Reintentar` after a failure and SHALL refresh when the day has changed since the last load. While loading, the page SHALL show a loading indicator. When the API cannot be reached, the page SHALL show `No hay conexión con el servidor.` and `Reintentar`, and the app SHALL remain usable.

#### Scenario: API stopped

- **WHEN** the API is not running and a logged-in user opens `Curiosidades`
- **THEN** the page shows `No hay conexión con el servidor.` and `Reintentar`
- **WHEN** the API is started and the user activates `Reintentar`
- **THEN** the facts load without restarting the app

#### Scenario: Day changes while the app is open

- **WHEN** the app stays open past midnight (UTC) and the user opens `Curiosidades`
- **THEN** the page requests the facts again and shows the new day's fact first

### Requirement: Facts texts in the app language

Every text the app writes on the strip and the page SHALL come from the resource files, in Spanish by default and in English, and the facts and the `Tal día como hoy` items SHALL be requested again in the new language when the user changes it.

#### Scenario: Switching to English

- **WHEN** the user selects `English` in `Personalización` and returns to `Inicio`
- **THEN** the strip reads `FACT OF THE DAY` with the English text of the same fact, and `Curiosidades` shows English facts, English dates and English Wikipedia items
