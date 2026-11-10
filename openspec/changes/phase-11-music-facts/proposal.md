# Proposal

## Why

OpenWave shows stations and nothing else: there is no reason to open it on a day you don't want to listen. A short music fun fact that changes every day, with the last two weeks kept at hand, gives the app a small daily reason to come back, and a live "what happened today in music" block connects it to the outside world without touching the stations flow.

## What Changes

- **A daily music fun fact, for logged-in users.** The daily fact, its history and the Wikipedia block are an account feature: a user who is not logged in sees an invitation to log in instead (the same pattern as `Favoritos`), in the strip and in the page. The API owns a curated list of short facts in Spanish and English (the same facts, translated) and picks one per calendar day with a fixed formula, so everyone sees the same fact on the same day and any past day's fact can be recomputed. No database table.
- **`Dato del día` strip in `Inicio`**, in every theme: a thin tappable band under the filter bar with a music icon, the caption `DATO DEL DÍA`, the first line of today's fact and a chevron. It is one shared control (like the bottom bar), drawn with the semantic keys, so each theme adds it with one line and gets its own colours, font and text size.
- **`Curiosidades` page**, reached from the strip and from a new third entry in `Ajustes`. It shows today's fact in full, then a block `Tal día como hoy en la música` (up to three music events or births for today's date from Wikipedia, each with its year, text and a link to the article, with the attribution Wikipedia requires), then the facts of the 14 previous days, newest first (15 facts in total, each with its date).
- **One new API endpoint**, `GET /api/facts/daily?days=15`, that returns the facts and the Wikipedia items in the request language (`Accept-Language`, Spanish by default). The API calls the Wikimedia Feed API ("On this day") for the current date, filters it with music keywords, caches the result for the day, and leaves the block empty when Wikipedia has nothing or cannot be reached: the daily fact never depends on Wikipedia.
- The gate is in the app, where the session lives (the API has no authorised endpoints in this project, see the known limitation in the project context): the facts are requested only while a user is logged in and forgotten on logout.
- The app keeps its rule: it talks only to our API. Loading, no-connection (`No hay conexión con el servidor.` + `Reintentar`) and empty states follow the existing patterns.
- All new texts come from the resource files, in both languages.

## Capabilities

### New Capabilities

- `music-facts`: the daily music fact and its history, available only to logged-in users: how the fact of a day is chosen, in which language it is served, the `Dato del día` strip in `Inicio`, the `Curiosidades` page with the 15-day history, the `Tal día como hoy en la música` block from Wikipedia and its failure modes, and the API endpoint behaviour.

### Modified Capabilities

- `app-navigation`: the "Ajustes page" requirement lists two entries; it gains a third, `Curiosidades`, with its description, in third position.

## Impact

- **Contracts:** new `DailyFactsDto`, `FactDto` and `MusicEventDto`.
- **API (`src/OpenWave.Api`):** `Resources/Facts.es.json` and `Facts.en.json` (about 60 facts, same ids in both files); `Options/WikimediaOptions.cs`; `Services/IWikimediaClient` + `WikimediaClient` (typed `HttpClient`, User-Agent, 5-second timeout); `Services/IFactsService` + `FactsService` (daily pick, history, keyword filter, per-day cache); `Controllers/FactsController.cs`; registration in `Program.cs`; new keys in `Messages*.resx` only if an API-side text is needed (none expected: facts are content, not messages).
- **App (`src/OpenWave.App`):** `Services/FactsApiClient.cs`; `ViewModels/CuriosidadesViewModel.cs`; `Views/CuriosidadesPage.xaml`; `Views/FunFactStrip.cs` (shared control) inserted into the eight `InicioTemplate`s; a third `SettingsRow` in `AjustesPage` and its command in `AjustesViewModel`; Shell route `curiosidades`; registrations in `MauiProgram.cs`; strings in `AppResources*.resx`; one icon for the Ajustes row.
- **README:** "Daily fact and Curiosidades" section (where it lives, how the day's fact is chosen, the Wikipedia source and its limits) and manual check items.
- **External dependency (API only):** the public Wikimedia Feed API, no key, content CC BY-SA (attribution shown in the page). Radio Browser, MySQL and the database schema are unchanged.
