# Proposal

## Why

The settings pages are drawn differently by each of the eight themes (cards, sleeves, keys, pads, LCD menus), list their entries in different orders, and use plain radio lists and pickers that are hard to scan. Several themes replace the Inicio/Reproductor/Favoritos icons with text keys, so the main navigation looks different everywhere. There is no way back from `Ajustes` to the page it was opened from, the entries have no description, and the whole app is Spanish only. This change gives the settings one clear, shared design (the one in the reference images), one icon bottom bar, and an app language choice that starts with English and makes new languages easy to add.

## What Changes

- **Shared settings design.** `Ajustes`, `Personalización`, `Preferencias` and the new filter selection pages have one layout in every theme; changing theme only recolours them with the theme's palette. **BREAKING** for theme authors: layouts no longer define `AjustesTemplate`, `SettingsPageTemplate` or `SettingsPanelStyle`, and each theme's own settings look (sleeves, ivory keys, silver keys, rubber buttons, pads, LCD menu, screen menu) is removed.
- **`Ajustes`** lists `Personalización` first and `Preferencias` second in every theme, each as a row with an icon, its title, a description and `›`:
  - `Personalización`: «Cambia el estilo de la app, el idioma, el tipo de letra y su tamaño.»
  - `Preferencias`: «Establece filtros de búsqueda predeterminados: país, idioma de las emisoras, género y tipo.»
- **Back arrow.** The header of `Ajustes`, `Personalización`, `Preferencias` and the filter pages shows a back arrow on the left. On `Ajustes` it returns to the page `Ajustes` was opened from (Inicio, Reproductor or Favoritos); on the other pages it returns to the page before.
- **`Personalización` redesign:** a fixed preview at the top (a sample station card with `Reproducir` and the heart) that stays visible while the options scroll; the themes as a grid of cards; the variant as a segmented control under them; the fonts as chips, each written in its font, with `+N` to show the rest; the text size as a five-step segmented control with the size's name under it; the new `Idioma` choice; and `Restablecer valores por defecto`.
- **`Preferencias` redesign:** a summary at the top (`Inicio mostrará` followed by the filters in words, without a station count); the rows `País`, `Idioma` and `Género`, each showing its value and opening a selection page; `Tipo de contenido` as chips; and a bar with `Cambios sin guardar`, `Descartar` and `Guardar` while there are unsaved changes. `Restablecer` stays.
- **Filter selection pages** (`País`, `Idioma`, `Género`): a search box, `Todos` (`sin filtro`), the recently chosen values, and the full list with each option's station count.
- **One bottom bar.** Every theme shows the same bar of three icons (house, radio, star) for `Inicio`, `Reproductor` and `Favoritos`, coloured by the palette, with the current one marked. **BREAKING** for theme authors: layouts no longer define `BottomBarTemplate`. Where each theme shows the bar (for example not in Reproductor in six themes) does not change.
- **App language.** `Personalización` offers `Idioma`: `Español` (default) and `English`. The choice is applied at once to every text, title and accessible name of the app, is remembered on the device, and is restored to `Español` by `Restablecer valores por defecto`. Adding a language means adding one translation file per project (app and API) and one registry entry, with no other code change.
- **API language.** The app sends its language with every request; the API returns its error `detail` messages and the content type labels in that language, and in Spanish when the language is missing or not supported. Station data from Radio Browser (names, countries, languages, tags) is shown as the catalogue gives it.

## Capabilities

### New Capabilities
- `app-language`: the app's display language: the `Idioma` choice, applying it everywhere at once, remembering it, Spanish as default, and adding languages with one translation file and one registry entry.
- `api-language`: the language of the API's own texts (error `detail` messages and content type labels) chosen from the request, with Spanish as default and fallback.

### Modified Capabilities
- `app-navigation`: `Ajustes` page (shared design, order, descriptions), the header's back arrow on the settings pages, the gear, and the bottom bar (one icon bar for every theme).
- `app-appearance`: the `Personalización` page (new design, language choice, reset), themed pages (settings pages shared), and adding a theme (fewer layout keys).
- `station-preferences`: the `Preferencias` page (summary, rows, chips, unsaved-changes bar), the new filter selection pages with search, recent values and counts, and saving and discarding.
- `user-session`: session errors are shown in the app language instead of always in Spanish.
- `theme-tocadiscos`: its bottom bar becomes the shared icon bar; its own settings pages are removed.
- `theme-valvulas`: header and bottom bar (shared icon bar); its own settings pages are removed.
- `theme-casete`: header and bottom bar (shared icon bar); its own settings pages are removed.
- `theme-boombox`: header and bottom bar (shared icon bar); its own settings pages are removed.
- `theme-mesadj`: header and bottom bar (shared icon bar, `EN DIRECTO` only on its own pages); booth labels in the app language; its own settings pages are removed.
- `theme-mp3`: header and bottom bar (shared icon bar); its own settings pages are removed.
- `theme-rueda`: header and bottom bar (shared icon bar under the wheel); its own settings pages are removed.

## Impact

- **App:** `AjustesPage`, `PersonalizacionPage`, `PreferenciasPage` and their view models are rewritten; new filter selection page and view model; `BottomBar` draws its own icons; the eight layouts lose `AjustesTemplate`, `SettingsPageTemplate`, `SettingsPanelStyle` and `BottomBarTemplate`; `AppNavigation` gets the back action; new localization service, translation resources (`es`, `en`) and language registry; every user-visible string in pages, layouts, controls and view models moves to the resources; `AppearanceService` stores the language; HTTP clients send `Accept-Language`; recent filter values are stored with MAUI `Preferences`.
- **API:** request localization middleware, translation resources (`es`, `en`) for ProblemDetails `detail` messages and content type labels.
- **Project conventions:** the rule "every string shown to the user in Spanish" becomes "every string in the app language, Spanish by default"; `openspec/config.yaml` and the README are updated accordingly.
- **No database change.** Preferences, recent values and the language stay on the device.
