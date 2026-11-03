# Proposal

## Why

Phase 5 built a theme engine (palette dictionaries + one layout dictionary + one `ThemeCatalog` entry) but ships a single theme, `Moderno`. A second, very different theme proves the engine really separates look from function, and gives the course project its signature look: `Tocadiscos`, a 1960s–70s hi-fi record player where stations are record sleeves, the player is a spinning vinyl with a tonearm, and favourites live in a wooden record crate.

## What Changes

- **New theme `Tocadiscos`** (`tocadiscos`), registered next to `Moderno`, with one palette (`standard`, dark: walnut wood, cream sleeves, brass accent, warm light text) and its own layout dictionary that defines every key `ModernoLayout` defines, without depending on it.
- **Inicio and Favoritos in Tocadiscos:** a vertical stack of square record sleeves (the focused one big, the next two smaller and dimmed) on the left and an information panel on the right (name, characteristics, heart, and the play state when the focused station is the current one). Pressing the big sleeve plays the station **without leaving the page**; pressing a smaller one only brings it into focus. Favoritos lists only the favourites and keeps the compact email + `Cerrar sesión` row.
- **Reproductor in Tocadiscos:** a header with a house button (to Inicio), the title and the gear; a large vinyl record whose label is the station logo, with a tonearm that plays or stops the station and visibly lands on or lifts off the record; the information panel with the status line and `Reintentar`; and a wooden record crate holding the user's favourites, 6 per page (3 per compartment), with arrows and swipe when there are more. No bottom bar on this page, and no previous/next favourite buttons (the crate replaces them).
- **Ajustes in Tocadiscos:** the same structure as Moderno (two full-width cards and the bottom bar), drawn as cream record sleeves on walnut; Personalización and Preferencias keep their content, on a rounded wood panel.
- **UI-only custom controls** in `Views/Controls/` (vinyl disc, tonearm, sleeve stack, record crate), drawn with MAUI shapes and palette colours, with animations that only observe the playback state, stop when the page is hidden and respect the system's reduced-motion setting.
- **BREAKING (behaviour, both themes): audio keeps playing across pages.** The single audio player moves out of `ReproductorPage` to a theme-independent, app-level host. Leaving `Reproductor` no longer stops the audio; going to the background still does. This is what lets Tocadiscos play from Inicio and Favoritos, and lets a theme change never interrupt a station. Moderno's look and its navigation (selecting a station still opens Reproductor) are unchanged.
- **Small additive view-model members** (no service, API or database change): a play-in-place command in Inicio and Favoritos, the playback state for their information panel, and the favourites list, login state, select-favourite and log-in commands for the crate in Reproductor. Existing members, including previous/next favourite, stay.
- **Personalización:** `Tema` lists `Moderno` and `Tocadiscos`; the variant choice appears only for Moderno. Choosing Tocadiscos keeps the stored Moderno variant, so returning to Moderno restores it.
- README: the theme list, the Tocadiscos controls and their licence note, and that animations only observe the playback state.

## Capabilities

### New Capabilities

- `theme-tocadiscos`: the Tocadiscos look and its UI-only behaviour: the sleeve stack and information panel in Inicio and Favoritos, the turntable (vinyl, tonearm, animations) and record crate in Reproductor, the Tocadiscos header and bottom bar, the restyled Ajustes, reduced motion, and the theme's accessibility and keyboard rules.

### Modified Capabilities

- `app-appearance`: two themes instead of one; the theme selector and variant memory; the audio is not interrupted by a theme change; "same functions in every theme" names previous/next favourite as a Moderno control.
- `app-navigation`: a theme may give `Reproductor` a house button and no bottom bar (Tocadiscos); the top-level destinations and header requirements stop assuming one layout.
- `audio-playback`: playback continues across pages (replaces "Playback is scoped to the player page"); the player description, the play/stop control and previous/next favourite are stated per theme.
- `station-browsing`: the station list, selection and cards are stated per theme (Moderno grid that opens Reproductor; Tocadiscos sleeve stack that plays in place).
- `favorites-browsing`: the heart, the Favoritos list, playing and removing a favourite are stated per theme.

## Impact

- **App code:** new `Resources/Themes/TocadiscosPalette.xaml`, `TocadiscosLayout.xaml` (+ `.cs`), `Views/Controls/` (vinyl disc, tonearm, sleeve stack, record crate, reduced-motion helper with small Windows/Android parts), `Resources/Images/Tocadiscos/LICENSE.txt`; one `ThemeCatalog` entry; the audio host moves from `ReproductorPage` to an app-level host (`App`/`AppShell` and `ReproductorPage` change; `MediaElementAudioPlayer` and `PlaybackService` keep their contract); additive members in `InicioViewModel`, `FavoritosViewModel` and `ReproductorViewModel`; `.csproj` asset item for the new licence file; README.
- **Unchanged:** `ModernoLayout.xaml` and the Moderno palettes, `PlaybackService`, `FavoritesService`, `SessionService`, `PreferencesService`, the API, the database and `database/schema.sql`. No new packages.
- **Manual verification only** on Windows and the Android emulator, in both themes.
