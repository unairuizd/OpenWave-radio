# Proposal

## Why

Phases 1–4 delivered a working radio; from now on the goal is to make OpenWave attractive. Today a theme is only a colour palette (`Blanco`, `Negro`), so every theme looks like the same list of rows. To allow very different looks in later phases (cassette radio, tube radio, turntable…) without touching logic, a theme must also own the layout of the main pages. This phase builds that engine and delivers the first complete look, `Moderno`.

## What Changes

- A theme becomes **one or more palettes + one layout + one registry entry**. A palette holds the 14 colour keys; a layout holds a look for `Inicio`, `Reproductor` and `Favoritos` and the station card templates.
- **BREAKING (appearance only):** the themes `Blanco` and `Negro` disappear. They become the two variants `Claro` and `Oscuro` of the new theme `Moderno`, with exactly the same colours. The default is `Moderno` `Claro`. A device that stored `Blanco` starts in `Moderno Claro`; one that stored `Negro` starts in `Moderno Oscuro`.
- `Inicio`, `Reproductor` and `Favoritos` take their look from the active layout and change at once when the theme or variant changes. `Iniciar sesión`, `Crear cuenta`, `Ajustes`, `Preferencias` and `Personalización` stay shared by every theme and change only through palette, font and text size.
- `Moderno` layout: `Inicio` and `Favoritos` show stations as a grid of cards (logo, name, play and heart buttons) whose number of columns adapts to the width (at least 2); `Reproductor` shows a large artwork and today's controls in a streaming-app style. Every existing function and state is kept; no new functions are added.
- The audio player stays a single instance that does not belong to any layout, so changing the look never recreates it.
- `Personalización`: the theme selector lists the registered themes, and a `Claro` / `Oscuro` choice appears only when the selected theme has more than one variant. `Restablecer valores por defecto` returns to `Moderno Claro`.
- Accessibility rules for every theme: Spanish accessible labels on every control, 48-unit minimum touch targets, keyboard use on Windows, text contrast of at least 4.5:1, usable from `Pequeño` to `Enorme`.
- Themes never set font families or text sizes: the user's font and size always win.
- README: a short "How to add a theme" section.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `app-appearance`: themes become palettes plus a layout; `Blanco`/`Negro` are replaced by `Moderno` with `Claro`/`Oscuro`; the theme variant is selected in `Personalización` and remembered (with migration of old values); new requirements for themed page layouts, the `Moderno` card grid and player, a layout-independent audio player, unchanged functions in every theme, themes not setting fonts or sizes, and accessibility.

`station-browsing`, `favorites-browsing` and `audio-playback` keep their requirements: the new layouts show the same information and offer the same actions (the `Favoritos` remove action becomes the filled heart, still without confirmation).

## Impact

- App only (`src/OpenWave.App`): `Resources/Themes` (palettes renamed, new `ModernoLayout.xaml`, `ThemeCatalog`), `Resources/Styles/Styles.xaml` (touch-target and card tokens), `AppearanceService`, `PersonalizacionPage` and its view model, `InicioPage`, `ReproductorPage`, `FavoritosPage`, a small grid behaviour, `README.md`.
- No change to view model logic, services other than `AppearanceService`, the API, `OpenWave.Contracts`, the database or `database/schema.sql`. No new packages.
- Stored preference `appearance_theme` changes its values (`white`/`black` → `moderno`) and a new key `appearance_theme_variant` is added; old values are migrated on start.
