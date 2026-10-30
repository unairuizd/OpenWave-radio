# Design

## Context

See proposal.md for motivation. Current state (phase 4):

- `Resources/Themes/WhiteTheme.xaml` and `BlackTheme.xaml` are `x:Class` resource dictionaries with the 14 colour keys. `ThemeCatalog` holds `AppThemeDefinition(Id, DisplayName, ResourceDictionaryType, BaseTheme)` entries `white`/`black`.
- `AppearanceService` (singleton `ObservableObject`) swaps the palette dictionary in `Application.Current.Resources.MergedDictionaries`, sets `UserAppTheme`, sets `AppFontFamily`/`AppFontFamilyBold`/`FontSize*`, and stores `appearance_theme`, `appearance_font`, `appearance_font_scale` in `Preferences`. `Initialize()` runs in the `App` constructor, before `CreateWindow`.
- `Styles.xaml` holds the implicit and named styles (`StationCard`, `FavoriteToggleButton`, `PlayerArtwork`…). It reads colours with `DynamicResource`. Buttons use a minimum height of 44.
- `AppShell.RefreshChrome` re-applies the header (`ShellChrome.EnsureTitleView`), tab fonts and Android radio colours on navigation and on every `AppearanceService` property change.
- `InicioPage` and `FavoritosPage` contain the whole UI in XAML and have no named elements used from code-behind. `ReproductorPage` code-behind calls `_audioPlayer.Attach(Player)` / `Detach(Player)` on the named `MediaElement` in `OnAppearing`/`OnDisappearing`.
- Members that differ from the request's wording (the code is followed):
  - The `Inicio` card tap runs `InicioViewModel.SelectStationCommand`, and its heart runs `InicioViewModel.ToggleFavoriteCommand`, which calls `FavoritesService.ToggleFromHeartAsync`.
  - `Favoritos` items are `StationDto` with `FavoritosViewModel.PlayCommand` and `RemoveCommand`; `Inicio` items are `StationListItem` (`Station`, `IsSelected`, `IsFavorite`, `FavoriteGlyph`).
  - The selection highlight exists only in `Inicio` (`IsSelected`); `Favoritos` has none today and gets none.

## Goals / Non-Goals

**Goals:**
- One mechanism (palette + layout dictionaries) that later themes reuse with no code change outside `Resources/Themes` and one `ThemeCatalog` entry.
- Zero change to view models' logic and to every service except `AppearanceService`.

**Non-Goals:**
- Per-theme versions of the shared pages (`Ajustes`, `Preferencias`, `Personalización`, `Iniciar sesión`, `Crear cuenta`).
- Changing the 14 colour values: they already pass 4.5:1 (lowest pair: primary accent `#3A5BD9` on the `Claro` selection highlight `#E4E9FB`, 4.72:1; secondary text there 5.62:1).
- A preview of the layout inside `Personalización` (the preview keeps showing palette, font and size).

## Decisions

### 1. Theme registry shape

```csharp
public sealed record ThemePalette(string Id, string DisplayName, Type ResourceDictionaryType, AppTheme BaseTheme);
public sealed record AppThemeDefinition(string Id, string DisplayName, Type LayoutDictionaryType, IReadOnlyList<ThemePalette> Palettes);
```

`ThemeCatalog.DefaultId = "moderno"` and `DefaultPaletteId = "light"`. Only `Moderno` is registered: palettes `light` "Claro" (`ModernoLightPalette`, Light) and `dark` "Oscuro" (`ModernoDarkPalette`, Dark), layout `ModernoLayout`. `Find(id)` stays. A palette is found within its theme, so two themes can both have a `light` palette.

*Alternative:* one flat list of "theme+variant" entries. It was rejected because Personalización needs to show the variants of the chosen theme only, and a future theme with one palette must show no variant selector.

### 2. Dictionaries in `MergedDictionaries`

`AppearanceService` keeps two references: `_paletteDictionary` and `_layoutDictionary`. `ApplyAppearance(theme, palette)` removes both old ones and adds the palette first, then the layout. It sets `UserAppTheme = palette.BaseTheme` and updates the `ThemeId` and `PaletteId` observable properties. The layout dictionary is replaced only when the theme changes, not on a variant change, so a `Claro`↔`Oscuro` switch rebuilds no template.

The palette and layout files are `x:Class` dictionaries (like today), created with `Activator.CreateInstance`. Inside the layout, colours and styles are read with `DynamicResource`, so a palette swap recolours already built templates and the layout never depends on load order.

### 3. Pages use `ControlTemplate="{DynamicResource …}"`

`InicioPage`, `FavoritosPage` and `ReproductorPage` set `ControlTemplate="{DynamicResource InicioTemplate|FavoritosTemplate|ReproductorTemplate}"`. Each template's root declares `x:DataType` of the page's view model. The templated content inherits the page's `BindingContext`, so the existing bindings and commands move into the template unchanged. `Inicio` and `Favoritos` page bodies become empty. Their code-behind (`LoadIfNeeded`, `OnAppearingAsync`) does not use named elements and stays.

The required keys each layout must define are `InicioTemplate`, `ReproductorTemplate`, `FavoritosTemplate`, `InicioStationTemplate` and `FavoritoStationTemplate`. Two station `DataTemplate`s are needed because the items have different types and commands (see Context). They share the card styles.

*Alternative:* swapping whole `ContentView`s per theme in code. It was rejected: it needs code per theme, while `ControlTemplate` + `DynamicResource` needs none.

### 4. The single audio player stays in the page, outside the template

The 1×1 `MediaElement` `x:Name="Player"` stays as `ReproductorPage`'s own `Content`. `ReproductorTemplate` places a `ContentPresenter` where the host goes (at the end, 1×1, never collapsed), so the code-behind's `Attach(Player)` / `Detach(Player)` keep working with no change. A template swap re-hosts the same `MediaElement` object; it is never created again and its `Source` is not touched. Themes change in `Ajustes`, so `Reproductor` has already run `OnDisappearing` → `Detach` (audio stopped). When the user comes back, `OnAppearing` → `Attach` runs as today.

*Alternatives:*
- A `MediaElement` inside each template. Rejected: it would be recreated per theme and is unreachable from code-behind.
- Moving it to `AppShell` or a window overlay. Rejected: it changes today's "playback belongs to `Reproductor`" behaviour and adds lifecycle code.

### 5. Re-applying the template on a theme change

`ControlTemplate` set through `DynamicResource` is re-evaluated when the key's value changes, and MAUI rebuilds the templated content when `ControlTemplate` changes. If a page that already exists does not refresh (to verify in group 2 on both platforms), `AppearanceService` raises its property change. A small `ThemedPage` helper then re-assigns the dynamic resource on the three pages, by having them listen to `AppearanceService.PropertyChanged` in code-behind and calling `SetDynamicResource(ControlTemplateProperty, key)`. That helper is added only if verification shows it is needed.

### 6. Adaptive grid: `AdaptiveSpanBehavior`

A `Behavior<CollectionView>` in `App/Behaviors/`, with a bindable `MinColumnWidth` (180 from the layout dictionary). On `SizeChanged`, if the computed `span = max(2, (int)(Width / MinColumnWidth))` differs from the current span, it sets a new `GridItemsLayout(span, ItemsLayoutOrientation.Vertical)` with the spacing token. It assigns a new instance rather than changing `Span`, because a live `Span` change is unreliable on Windows. It is attached inside the layout templates, so a future theme may use it or not.

### 7. Moderno card

The card is a `Border` (`StationCard` style; selection trigger in `Inicio` as today) with a `TapGestureRecognizer`. Inside it is a vertical grid:

- **Artwork:** the placeholder image with the logo `Image` on top, and `HeightRequest` bound to its own `Width`, so it is square at any column width.
- **Name:** `StationNameLabel`, `MaxLines=2`, `LineBreakMode=TailTruncation`.
- **Secondary label:** one line, tail-truncated. `Inicio` shows country · language as today; `Favoritos` shows the country, or `Desconocido`.
- **Button row:** a play button and the heart.

There are no fixed heights, so the card grows with the text size and is never shorter than it is wide. The play glyph is a text glyph (`▶` + U+FE0E, like the hearts) in a new `CardIconButton` style. In `Favoritos` the heart shows the filled glyph `FavoritesService.FilledHeart` and runs `RemoveCommand`.

### 8. Moderno player

A vertically centred column with the following parts, in a `ScrollView` so `Enorme` never clips:

- the artwork (`PlayerArtwork`, size token raised for this layout to 240);
- the name (`PageTitleLabel`, centred) with the heart beside it;
- the country and the state text;
- a wide primary play/stop `Button` (`ToggleButtonText`);
- `Reintentar` on error.

There is no `Reproductor` title label beyond the Shell header, which already shows it. The bindings are exactly today's `ReproductorViewModel` members.

### 9. Accessibility

- **Touch targets:** a new `MinTouchTarget` resource (48) in `Styles.xaml` is used as `MinimumHeightRequest`/`MinimumWidthRequest` by the implicit `Button` style, `FavoriteToggleButton` and `CardIconButton`. 48 units meets both 48 dp on Android and 32 px on Windows.
- **Labels:** `SemanticProperties.Description` is set on every button and on the card:
  - the card and its play button: `Reproducir {0}` via `StringFormat` on the station name;
  - hearts: `Añadir a favoritas` / `Quitar de favoritas` chosen by a `DataTrigger` on `IsFavorite` in `Inicio`, and on `FavoriteGlyph` = `{x:Static FavoritesService.FilledHeart}` in `Reproductor`; `Quitar de favoritas` on the `Favoritos` heart;
  - the toggle reads its own text (`Reproducir`/`Detener`).

  No view model change is needed.
- **Keyboard:** WinUI buttons are tab stops, activate on Enter/Space and draw the system focus rectangle. The card `Border` is not a tab stop, and its play button gives keyboard access to the same command. No custom focus code is needed.
- **Contrast:** already verified (see Non-Goals), and re-checked if a layout introduces a new text-on-colour pair. The play button uses `AccentColor`/`AccentTextColor` (5.71:1 `Claro`, 9.07:1 `Oscuro`).

### 10. Persistence and migration

The keys are `appearance_theme` (theme id) and the new `appearance_theme_variant` (palette id). `Initialize()`:

1. Read the theme. `white` maps to (`moderno`, `light`) and `black` maps to (`moderno`, `dark`), and both are written back at once.
2. Otherwise look the theme up in `ThemeCatalog`; an unknown theme falls back to `moderno`.
3. Read the variant and look it up within the theme. An unknown variant uses the theme's first palette, and for `moderno` that is `light`, so an unknown value always lands on Moderno Claro.

All reads keep today's try/catch. Public API: `SetTheme(themeId)` keeps the current palette id when the new theme has it (otherwise its first palette). `SetPalette(paletteId)` changes only the variant. `ResetToDefaults()` sets `moderno` + `light`.

### 11. Personalización

`PersonalizacionViewModel` gains:

- `SelectedPaletteId` (object, like the other radio values);
- `Palettes` (the selected theme's palettes);
- `ShowPaletteChoice` (`Palettes.Count > 1`), all updated from `AppearanceService.PropertyChanged`.

The page adds a "Variante" radio group with `BindableLayout` over `Palettes`, visible on `ShowPaletteChoice`. The `Tema` group is unchanged except for the data.

## Risks / Trade-offs

- [The `DynamicResource` `ControlTemplate` may not rebuild on a live swap on one platform] → verify in group 2; fall back to the re-assignment helper (Decision 5).
- [Re-hosting the `MediaElement` through `ContentPresenter` may disconnect its handler] → it is detached and stopped while `Ajustes` is shown; verify play/stop after a theme change on both platforms.
- [`GridItemsLayout` re-layout glitches on Windows when the span changes] → assign a new layout instance; only on real span changes.
- [Self-bound `HeightRequest` (square artwork) can cause a layout pass loop on resize] → if seen, compute the artwork height in `AdaptiveSpanBehavior` from the column width and expose it as a bindable value instead.
- [Two station `DataTemplate`s per layout is more work for future themes] → accepted; it avoids changing view models or item types.
- [Users lose the names `Blanco`/`Negro`] → same colours under `Moderno Claro`/`Oscuro`, migrated automatically.

## Migration Plan

Old stored values are migrated on first start (Decision 10). There is no database or API change. Rolling back means restoring the previous app build; a migrated value `moderno` is unknown there and falls back to `Blanco` without crashing (the existing fallback).
