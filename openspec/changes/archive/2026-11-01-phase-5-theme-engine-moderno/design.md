# Design

## Context

See proposal.md for the motivation. The current state that shapes the approach:

**Appearance today**
- `AppearanceService` merges one theme dictionary (`WhiteTheme` or `BlackTheme`, 14 colour keys) into `Application.Current.Resources`.
- It sets `UserAppTheme` and writes the font and size resources.
- `Styles.xaml` reads everything with `DynamicResource`.
- `ThemeCatalog` is a list of `AppThemeDefinition(Id, DisplayName, ResourceDictionaryType, BaseTheme)`.

**Navigation today**
- `AppShell` is a `TabBar` with four `ShellContent`s (`inicio`, `reproductor`, `favoritos`, `ajustes`).
- `login`, `register`, `personalizacion` and `preferencias` are pushed routes.
- The native header gets a styled `TitleView` from `ShellChrome.EnsureTitleView`.
- The native tab labels get the font from `ShellChrome.ApplyTabFonts`, through platform code.

**Audio today**
- One singleton `MediaElementAudioPlayer` drives a single `MediaElement` that is the content of `ReproductorPage`.
- The page attaches it in `OnAppearing` and detaches it in `OnDisappearing`, and the detach stops playback. Leaving `Reproductor` therefore stops audio, as the `audio-playback` spec requires.

**Station lists today**
- `Inicio` lists `StationListItem` (with `IsSelected` and `IsFavorite`).
- `Favoritos` lists `StationDto`.
- Hearts are text glyphs (`FavoritesService.FilledHeart` and `EmptyHeart`).

**Things the brief names differently from the code**
- The brief's "select-and-play action" is `PlaybackService.SelectStationAsync`, called by `InicioViewModel.SelectStationCommand` and `FavoritosViewModel.PlayCommand`.
- The brief says logged-out users of `Favoritos` "are sent to Iniciar sesión". In the code (and the `favorites-browsing` spec), `Favoritos` shows `Inicia sesión para ver tus favoritas.` with an `Iniciar sesión` button. That behaviour is kept unchanged.

## Goals / Non-Goals

**Goals:**
- A theme is fully described by palette dictionaries, one layout dictionary and one `ThemeCatalog` entry.
- No view model or service knows which theme is active.
- The header and bottom bar exist once per layout and are reused by every page template.
- Page behaviour is preserved. The only new logic is previous/next favourite and the small view-model properties the new layout binds to.

**Non-Goals:**
- No change to when audio stops (leaving `Reproductor` or the app going to the background still stops it).
- No mini-player.
- No animations.
- No new packages.
- No changes to `Iniciar sesión`/`Crear cuenta` beyond what the palette, font and size already do.

## Decisions

### 1. Theme = palette dictionaries + layout dictionary + registry entry

**Registry**
```csharp
public sealed record ThemePalette(string Id, string DisplayName, Type ResourceDictionaryType, AppTheme BaseTheme);
public sealed record AppThemeDefinition(string Id, string DisplayName, Type LayoutDictionaryType, IReadOnlyList<ThemePalette> Palettes);
```
- `ThemeCatalog.All` registers only `moderno`, with the palettes `light` ("Claro", `ModernoLightPalette`, Light) and `dark` ("Oscuro", `ModernoDarkPalette`, Dark).
- `DefaultId = "moderno"` and `DefaultPaletteId = "light"`.

**Palettes**
- `ModernoLightPalette.xaml` and `ModernoDarkPalette.xaml` are the old `WhiteTheme` and `BlackTheme`: same 14 keys and the same values, so the contrast measured in phase 4 still holds.
- The old files are deleted.

**Applying a theme**
- `AppearanceService` keeps two references, `_paletteDictionary` and `_layoutDictionary`.
- `ApplyTheme(theme, palette)` replaces the palette dictionary. It replaces the layout dictionary only when its type changes, so switching Claro ↔ Oscuro does not rebuild templates.
- It then sets `UserAppTheme` from `palette.BaseTheme` and raises `ThemeId` and `PaletteId`.
- `SetTheme(id)` keeps the current palette id if the new theme has it, otherwise it uses that theme's first palette.
- `SetPalette(id)` is new.
- `ResetToDefaults` also resets the palette.

**Persistence**
- Keys: `appearance_theme` (default `moderno`) and the new `appearance_theme_variant` (default `light`).
- In `Initialize`, a stored `white` or `black` is mapped to `moderno` with `light` or `dark`, and the result is written back.
- An unknown theme resets both values to the defaults. An unknown palette falls back to the theme's first palette.
- `Initialize` still runs in the `App` constructor, before `CreateWindow`, so the first page never shows the wrong look.

**Alternative rejected:** one dictionary per theme-and-variant combination. It duplicates the layout in every variant and does not scale to themes with several palettes.

### 2. Templated pages with `ControlTemplate="{DynamicResource …}"`

The layout dictionary defines:
- page templates: `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`, `AjustesTemplate` and `SettingsPageTemplate`;
- the station card `StationCardTemplate`;
- the shared chrome: `HeaderTemplate` and `BottomBarTemplate`.

Each of the six pages sets `ControlTemplate="{DynamicResource XTemplate}"` and `Shell.NavBarIsVisible="False"`.

**Binding context**
- Templated content does not inherit the page's view model.
- The root of every page template therefore sets `BindingContext="{Binding BindingContext, Source={RelativeSource TemplatedParent}, x:DataType=ContentPage}"` together with `x:DataType="vm:XViewModel"`. Bindings inside are compiled and target the same view models and commands as today.
- Phase 4 code-behind only sets `BindingContext` and forwards `OnAppearing`. No code-behind refers to named elements except `ReproductorPage.Player` (see decision 6), so nothing else has to move.

**Where page content lives**
- `Inicio`, `Favoritos`, `Reproductor` and `Ajustes` move all their visible content into their template. The page itself keeps only its view model and, for `Reproductor`, the player.
- `Personalización` and `Preferencias` keep their content in the page, unchanged except that the in-page title label is removed (the header now shows it). `SettingsPageTemplate` hosts that content with a `ContentPresenter` between the header and the bottom bar.

**Page skeleton**
- Every page template is a `Grid` with rows `Auto, *, Auto`: header, content, bottom bar.
- Only the middle row scrolls (a `ScrollView` or the `CollectionView`), so the header and the bar stay fixed.

**Live refresh**
- Changing a merged dictionary re-evaluates `DynamicResource` on existing pages, including `ControlTemplate`.
- Task group 2 verifies this on a page that already exists. If a page does not refresh, `AppearanceService` re-applies the template on the existing themed pages (by reading the resource again), but only in that case.

**Alternative rejected:** a separate page class per theme. That duplicates code-behind and navigation per theme.

### 3. Shared header and bottom bar

`HeaderTemplate` and `BottomBarTemplate` are `ControlTemplate`s defined once in `ModernoLayout.xaml`. Each page template places them with `<ContentView ControlTemplate="{StaticResource HeaderTemplate}" />` and the same for the bar.

**Header**
- A one-cell `Grid`.
- The title `Label` spans the full width with `HorizontalTextAlignment="Center"`, `TextTransform="Uppercase"` and the `PageTitleLabel` style (`AppFontFamilyBold`, `FontSizeTitle`). Its left and right margins are equal to the touch-target width, so it stays centred on the page and never runs under the gear.
- The gear is an `ImageButton` aligned to the end.
- The title text is `{Binding Title, Source={RelativeSource AncestorType={x:Type ContentPage}}}`. The pages' `Title` becomes `Inicio`, `Reproductor`, `Favoritos`, `Ajustes`, `Personalización` and `Preferencias`, as today.

**Bottom bar**
- Three columns. Each column is an `ImageButton` (icon) above a `Label` (short Spanish label).
- The `ImageButton` is the focusable, accessible control. The label also has a tap gesture with the same command and is hidden from the accessibility tree, so screen readers announce each section once.

**Current section**
- Each page declares it with a new attached property `AppNavigation.Section` (`Inicio`, `Reproductor`, `Favoritos`, or `None` by default).
- The bar reads it from the ancestor page.
- `DataTrigger`s set the icon tint and label colour to `TabBarSelectedColor` for the marked button and `TabBarUnselectedColor` for the others.
- They also set the marked button's accessible description to, for example, `Inicio, seleccionado`.

**Commands**
- The commands live in a small static class `Services/AppNavigation` (UI navigation only, no business logic), bound with `{x:Static services:AppNavigation.OpenAjustesCommand}` and `GoToSectionCommand`.
- **Alternative rejected:** adding chrome commands to the six view models. That duplicates code and would make view models know about the chrome.

### 4. Navigation structure

**Shell**
- `AppShell` keeps a `TabBar` with only the three sections, because it gives the `//inicio`, `//reproductor` and `//favoritos` absolute routes.
- It sets `Shell.TabBarIsVisible="False"` at Shell level, so the native tab bar is never shown.
- `ajustes` becomes a registered route (pushed), like `personalizacion` and `preferencias`.

**`AppNavigation.OpenAjustes`**
- If the current page is `AjustesPage`, it does nothing.
- If `AjustesPage` is in the current navigation stack (that is, a settings sub-page is open), it pops back to it with `GoToAsync("..")`.
- Otherwise it pushes `ajustes`.

**`AppNavigation.GoToSection(route)`**
- If the current location is that section's root page, it does nothing.
- Otherwise it calls `GoToAsync("//" + route)`. This also resets any pushed settings pages.

**Back and playback**
- The Android back button pops `Personalización` or `Preferencias` back to `Ajustes`, because they are pushed.
- `Ajustes` is pushed onto the current section, so opening it from `Reproductor` makes `Reproductor` disappear and stop audio. Switching to the `Ajustes` tab did exactly the same before.

**`Iniciar sesión` and `Crear cuenta`**
- They stay pushed routes with the native header (`Shell.NavBarIsVisible` default `True`), so `ShellChrome.EnsureTitleView` keeps styling their title.
- With the native tab bar always hidden, `ShellChrome.ApplyTabFonts` and its Android and Windows parts would do nothing, so they are removed. `ApplyRadioColors` stays.
- Hiding the tab bar on these two pages as well is a deliberate choice: the brief only said to hide it on templated pages. Showing a native tab bar only on the login flow would look out of place, and the login flow returns with its back action.

**Alternative rejected:** dropping Shell sections and navigating only with pushed pages. That loses `//route` resets and changes the back behaviour everywhere.

### 5. One station card for Inicio and Favoritos

`StationCardTemplate` is the single `DataTemplate` for `StationListItem`. `Favoritos` therefore switches from `StationDto` to `StationListItem` as well.

**What the card binds to (all on the item)**
- `Station`, `IsSelected` and `IsFavorite`.
- A new `Details` text: the country and language, then up to three tags, all joined with ` · ` (`Desconocido` for a missing country or language, as today). This keeps the formatting out of XAML.
- Two new command properties, `PlayCommand` and `HeartCommand`, set by the owning view model when it creates the items:
  - `Inicio`: `SelectStationCommand` and `ToggleFavoriteCommand`.
  - `Favoritos`: `PlayCommand` and `RemoveCommand`.
  - Each takes the item as its parameter.
- **Alternative rejected:** binding to an ancestor view model of a fixed type, which cannot serve two view model types from one template.

**Highlight**
- `FavoritosViewModel` now also sets `IsSelected` from `PlaybackService.CurrentStation` (the same handler as in `Inicio`), so the shared card highlights the current station in both lists.

**Card layout (portrait)**
- A `Border` (`StationCard` style; selected trigger: `SelectedItemBackgroundColor` with an `AccentColor` stroke) with a tap gesture → `PlayCommand`.
- A square logo area (`HeightRequest` bound to its own `Width`), `AspectFit`, with the fallback image underneath.
- The name (`StationNameLabel`, `MaxLines=2`, tail truncation).
- The details (`SecondaryLabel`, `MaxLines=2`).
- A row with the play `ImageButton` (accent background, `AccentTextColor` tint, description `Reproducir {name}`) and the heart `ImageButton` (`FavoriteIconColor` tint; filled or outline through a `DataTrigger` on `IsFavorite`, which also switches the description).
- The square logo plus at least three text lines and a 48-unit button row make every card taller than it is wide.

**Equal card heights**
- The Windows grid sizes every item like the first one it measures.
- So the name area and the details area each reserve two lines: an invisible two-line placeholder label (`Opacity=0`, hidden from the accessibility tree) sits under each real label.
- This lesson comes from the previous attempt at this phase.

**Grid**
- A `CollectionView` with a vertical `GridItemsLayout` (span 2) and a new `AdaptiveSpanBehavior` (in `Behaviors/`).
- On `SizeChanged` the behaviour sets `Span = max(2, (int)(Width / CardMinWidth))`, with `CardMinWidth` = 170 as a layout token.
- It changes `Span` on the existing layout, because assigning a new `GridItemsLayout` after the first render is ignored on Windows.

### 6. Single audio player

**Where it lives**
- The `MediaElement` stays the page content of `ReproductorPage` (`x:Name="Player"`, 1×1, as today).
- `ReproductorTemplate` shows it through a `ContentPresenter` placed outside the artwork, so it never draws over the logo.
- The code-behind keeps `Attach(Player)` in `OnAppearing` and `Detach(Player)` in `OnDisappearing`.
- **Result:** there is exactly one player object, it is not part of any layout, and today's rule (leaving `Reproductor` stops audio) is unchanged.

**Theme changes and audio**
- A theme change can only be made in `Personalización`, where `Reproductor` is not shown. Audio is therefore already stopped when the template is swapped.
- When `Reproductor` appears again, the same `MediaElement` is re-attached. No theme change can restart or duplicate audio.

**Alternatives rejected:**
- Hosting the `MediaElement` in the Shell or in a window overlay. That would keep audio alive across pages, which changes today's behaviour and the `audio-playback` spec.
- Defining it inside the template. It would be recreated whenever the layout changes, and the code-behind could not reach it.

### 7. Reproductor view model additions

**New properties**
- `IsActive` (from `PlaybackService.IsActive`) selects the play or stop icon and its description.
- `IsFavorite` replaces `FavoriteGlyph`. `FilledHeart` and `EmptyHeart` are removed once no binding uses them.
- `Details`: `Country · Language · tag1 · tag2 · tag3`, with missing values skipped.
- `HasStation` enables the play/stop button.
- `Notice` is a short message for the status line (used for "no favourites").
- `ShowPlayerControls` is removed. The centre button is always visible and enabled only with a station. In the error state it shows the play icon and starts playback again, and `Reintentar` stays in the status line.

**`PreviousFavoriteCommand` and `NextFavoriteCommand`**
- They share one private `MoveToFavoriteAsync(int step)`.
- **Logged out:** set `FavoritesService.PendingStation = null` (nothing to add) and call `GoToAsync("login")`.
- **The list:** the favourites with `IsFavorite(id)` from `FavoritesService.Favorites` (newest first). This is the same filter and order as the `Favoritos` page.
- **Empty list:** if the list is empty, reload it once with `FavoritesService.LoadAsync()` (it can be empty because loading failed at login).
  - If it is still empty, set `Notice = "Aún no tienes emisoras favoritas."` and return.
  - If the reload throws `ApiRequestException`, set `Notice` to its Spanish message and return.
- **The target:** the index of the current station in the list.
  - If it is not found, the target is the first favourite for next and the last for previous.
  - Otherwise the target is `(index + step + count) % count`.
- **Same station:** if the target id equals the current id, do nothing.
- **Otherwise:** call `PlaybackService.SelectStationAsync(target)`. That stops the previous stream and starts the new one, and `Inicio` and `Favoritos` move their highlight through the existing `CurrentStation` change handlers.
- `Notice` is cleared whenever the current station or the playback state changes.

### 8. Icons

**Drawing and tinting**
- Original, single-colour 24×24 SVGs drawn for this project in `Resources/Images/`: `ic_settings`, `ic_search`, `ic_clear`, `ic_home`, `ic_player`, `ic_favorites`, `ic_heart`, `ic_heart_outline`, `ic_play`, `ic_stop`, `ic_previous`, `ic_next`.
- They are tinted at runtime with CommunityToolkit.Maui's `IconTintColorBehavior`, whose `TintColor` comes from a palette key through `DynamicResource`. They follow the variant and contain no colour of their own that is ever shown.

**Size**
- Icon size is the `FontSizeTitle` resource (24 at `Normal`, about 38 at `Enorme`). Icons therefore scale with the text, as the `Text size` requirement says.
- The buttons keep a minimum of `MinTouchTarget` (48) in both directions.

**Licence and project file**
- `Resources/Images/Icons-LICENSE.txt` states that the icons are original work of this project.
- The `.csproj` image item becomes `Resources\Images\*.svg`, so the `.txt` is not treated as an image. The `.txt` ships as `MauiAsset` under `Licences\`, like the font licences.

**Alternative considered:** Material Symbols (Apache-2.0). Drawing twelve simple icons avoids downloading third-party files and keeps the licence trivial.

### 9. Ajustes cards and keyboard access

**Ajustes cards**
- They are `Button`s with a new `SettingsCardButton` style:
  - `SurfaceColor` background, `DividerColor` border and `PrimaryTextColor` text;
  - `AppFontFamilyBold`, `FontSizeSubtitle`;
  - full width, the text in upper case.
- They are not tappable `Border`s, because a `Border` cannot take keyboard focus on Windows.

**Keyboard and accessibility**
- Station cards stay `Border`s with a tap gesture. Their play button gives keyboard and screen-reader access to the same command.
- All interactive elements are `Button`, `ImageButton`, `Entry` or `RadioButton`, which WinUI makes tab stops that activate with Enter or Space and show the system focus rectangle.
- Every `ImageButton` sets `SemanticProperties.Description`.

### 10. Styles and tokens (`Styles.xaml`)

- New tokens:
  - `MinTouchTarget` 48 (replaces the 44 literals);
  - `CardMinWidth` 170;
  - `CardSpacing` 12;
  - `PlayerArtworkMaxSize`.
- New styles: `IconButton` (transparent background, `MinTouchTarget` minimums, Padding 12), `AccentIconButton` (accent background, round) and `SettingsCardButton`. Tint colours stay in the templates through `DynamicResource`.
- Any colour the layout needs for the header and the bar comes from existing keys:
  - `PageBackgroundColor` for the header;
  - `TabBarBackgroundColor`, `TabBarSelectedColor` and `TabBarUnselectedColor` for the bar;
  - `DividerColor` for separators.
- No new palette keys, so the 14-key contract holds.

### 11. Safe areas

- On Android, .NET 10 pages lay out inside the system bars by default.
- The bar is the last row of the page grid, so it sits above the gesture or navigation bar.
- Task group 2 checks this on the emulator. If the bar overlaps, the page templates set `SafeAreaEdges` to keep the bottom edge clear.

### 12. Contrast

The palette values do not change. The new text and background pairs are:

| Pair | Claro | Oscuro |
|---|---|---|
| `TabBarSelectedColor` on `TabBarBackgroundColor` | about 5.4:1 | about 7.8:1 |
| `TabBarUnselectedColor` on `TabBarBackgroundColor` | about 6.4:1 | about 7.0:1 |

The lowest existing pair is `AccentColor` on `SelectedItemBackgroundColor`, 4.72:1 in Claro. Task group 8 measures every pair again and records the values in the README.

## Risks / Trade-offs

- **`Shell.TabBarIsVisible="False"` may leave an empty navigation pane on Windows.** → Verify in task group 2. If it does, hide the WinUI pane in `ShellChrome.Windows.cs` (platform code already exists there).
- **`DynamicResource` on `ControlTemplate` might not refresh an existing page on one platform.** → Verify in task group 2, with a fallback re-apply in `AppearanceService` (decision 2).
- **`IconTintColorBehavior` on `ImageButton` might not update when the tint resource changes.** → Verify the Claro ↔ Oscuro switch in task group 7. If needed, bind the tint through a style setter.
- **Large text (`Enorme`) makes the header title collide with the gear on narrow phones.** → The title has equal side margins, `MaxLines=1` and tail truncation.
- **On Windows the tint draws an icon at the size it had when first drawn.** After a live text size change, icons on pages that already exist keep their old size until the page is created again (for example after a restart); Android resizes them at once. Redrawing the tint, reloading the image and rebuilding the templates were tried and did not work reliably, so this is accepted and documented in the README as a known limitation.
- **The card grid on Windows measures items from the first card.** → The two-line placeholders (decision 5).
- **Prev/next uses the in-memory favourites, which may be older than the server.** → It is the same list the hearts and `Favoritos` use, and it is reloaded when empty and each time `Favoritos` opens. This is acceptable for the course project.

## Migration Plan

- On the first start of the new version, the stored `appearance_theme` value is migrated:
  - `white` becomes `moderno` with `light`;
  - `black` becomes `moderno` with `dark`;
  - anything else becomes the defaults.
- The new values are written back.
- No database change.
- Rollback (installing the previous build) reads `moderno` as unknown and falls back to `Blanco` without crashing.
