# Proposal

## Why

Phases 1–4 delivered a working radio. The goal from now on is to make OpenWave attractive. Today a theme is only a colour palette (`Blanco`, `Negro`), and every page uses the native Shell header and tab bar. Later phases will add very different looks without touching logic, so a theme must also own the layout of the pages and of the app's chrome (header and bottom bar).

This phase builds that engine and delivers the first complete look, `Moderno`, which also redesigns the main pages.

## What Changes

**Theme engine**
- A theme becomes one or more palettes, one layout and one registry entry.
  - A palette holds the colour keys.
  - A layout holds the page templates, the shared header, the shared bottom bar and the station card.
- **BREAKING (appearance only):** the themes `Blanco` and `Negro` disappear and become the `Claro` and `Oscuro` variants of the new theme `Moderno`, with exactly the same colours.
  - The default is `Moderno Claro`.
  - Stored `Blanco` is migrated to `Moderno Claro`, and stored `Negro` to `Moderno Oscuro`.

**Templated pages**
- `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias` take their look from the active layout. They change at once when the theme or variant changes.
- `Iniciar sesión` and `Crear cuenta` keep their current layout and the native header. They change only through palette, font and size.

**Common header and bottom bar**
- Every templated page shows a header: the page title in upper case, centred on the full width, and an `Ajustes` gear on the right.
- Every templated page shows a fixed bottom bar with `Inicio`, `Reproductor` and `Favoritos`. The current section is marked; on the settings pages none is marked.

**Navigation (BREAKING for the navigation spec)**
- `Ajustes` is no longer a fourth tab. It opens from the gear.
- The native tab bar and header are hidden on templated pages.
- `Ajustes` shows two cards, `PERSONALIZACIÓN` and `PREFERENCIAS`.

**Moderno look**
- `Inicio`:
  - a search row with magnifier and clear icon buttons;
  - the existing filter bar;
  - a grid of portrait station cards (logo, name, characteristics, play and heart), with columns that adapt to the width (at least 2).
- `Favoritos`:
  - the same cards; the filled heart removes the station;
  - a compact email and `Cerrar sesión` row.
- `Reproductor`:
  - a large logo;
  - name and characteristics with a heart;
  - `Favorita anterior`, play/stop and `Favorita siguiente` buttons;
  - a compact status line, with the error and `Reintentar` when needed.

**New function: previous and next favourite**
- Moves through the user's favourites in the `Favoritos` order, wrapping around at both ends, and plays the chosen station.
- When the user is logged out, it opens `Iniciar sesión`.
- With no favourites, it shows `Aún no tienes emisoras favoritas.`

**Audio player**
- There is still exactly one audio player, and it does not belong to any layout.
- Playback still stops when leaving `Reproductor`, exactly as today.

**Personalización**
- A generic theme list.
- A `Claro`/`Oscuro` choice that appears only when the selected theme has more than one palette.
- `Restablecer valores por defecto` returns to `Moderno Claro`.

**Icons**
- Original, simple SVG icons, tinted from the palette: gear, search, clear, home, player, favourites, hearts, play, stop, previous and next.
- Their licence is recorded next to them.

**Accessibility (applies to every theme)**
- Spanish accessible labels.
- Touch targets of at least 48 units.
- Keyboard use on Windows.
- Text contrast of at least 4.5:1.
- Layouts usable from `Pequeño` to `Enorme`.

**Fonts and sizes**
- Themes never set font families or text sizes.

**Docs**
- README: "How to add a theme".

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `app-appearance`:
  - themes become palettes plus a layout;
  - `Moderno` with `Claro`/`Oscuro` replaces `Blanco`/`Negro`;
  - the variant is selected in `Personalización` and remembered, with migration;
  - templated pages and the chrome follow the layout;
  - the audio player is independent of the layout;
  - themes do not set fonts or sizes;
  - accessibility rules;
  - icons scale with the text.
- `app-navigation`:
  - three sections in a bottom bar drawn by the layout;
  - a common header with the `Ajustes` gear;
  - `Ajustes` is no longer a tab and shows two cards.
- `station-browsing`:
  - `Inicio` shows stations as a grid of portrait cards with play and heart;
  - the search and clear buttons are icon buttons.
- `favorites-browsing`:
  - hearts are filled and outline icons;
  - `Favoritos` uses the same cards;
  - the filled heart removes a station.
- `audio-playback`:
  - the `Reproductor` layout (large logo, characteristics, play/stop icon, always visible controls);
  - new previous and next favourite actions.

`user-session` is unchanged: the email and `Cerrar sesión` stay in `Favoritos`.

## Impact

**App only (`src/OpenWave.App`)**
- `Resources/Themes`:
  - the palettes are renamed;
  - new `ModernoLayout.xaml`;
  - `ThemeCatalog` is extended.
- `Resources/Images`: new SVG icons and their licence. The `.csproj` image item now includes only image files, and the licence text ships as an asset like the font licences.
- `Resources/Styles/Styles.xaml`: tokens for touch targets, icons and cards.
- `AppearanceService`.
- `AppShell`: `Ajustes` is a route, and the native tab bar is hidden.
- `ShellChrome`: the tab-font code is removed, because no native tab bar is shown any more.
- A new navigation helper for the header and bar.
- A new grid behaviour.
- All six templated pages:
  - `InicioViewModel`, `FavoritosViewModel` and `ReproductorViewModel` (the card commands, `IsFavorite`/`IsActive`, and previous/next);
  - `StationListItem` (characteristics text, commands);
  - `PersonalizacionViewModel`.
- `README.md`.

**Not changed**
- The API, `OpenWave.Contracts`, the database and `database/schema.sql`.
- No new packages: `IconTintColorBehavior` is already in CommunityToolkit.Maui.

**Stored preferences**
- `appearance_theme` changes its values (`white`/`black` → `moderno`).
- New key `appearance_theme_variant`.
- Old values are migrated on start.
