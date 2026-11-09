# Theme brief: <Spanish name>

## 1. Concept

<One paragraph: which real object the app becomes, its era, and the one idea each page is built around.>

## 2. Identity

| Field | Value |
|---|---|
| Theme id | `<id>` (stored in preferences, never shown) |
| `DisplayName` | `<Spanish name>` |
| Palettes | `<palette id>` "<Spanish name>" (`Light`/`Dark`), … |
| Layout class | `Resources/Themes/<Name>Layout.xaml` (+ `.cs`) |
| Palette classes | `Resources/Themes/<Name><Palette>Palette.xaml` (+ `.cs`) |

## 3. Palette

`SelectedItemBackgroundColor` role: **background** / **outline**.

| Key | Value | Role in this theme |
|---|---|---|
| `PageBackgroundColor` | | |
| `SurfaceColor` | | |
| `InputBackgroundColor` | | |
| `DividerColor` | | |
| `SelectedItemBackgroundColor` | | |
| `PrimaryTextColor` | | |
| `SecondaryTextColor` | | |
| `ErrorTextColor` | | |
| `AccentColor` | | |
| `AccentTextColor` | | |
| `FavoriteIconColor` | | |
| `TabBarBackgroundColor` | | |
| `TabBarSelectedColor` | | |
| `TabBarUnselectedColor` | | |

Measured contrast: <Primary/Page, Primary/Surface, Secondary/Page, Secondary/Surface, Error/Page, Error/Surface,
AccentText/Accent, Favorite/Page, TabSelected/TabBar, TabUnselected/TabBar, and Selected as above>.

## 4. Pages

### Header (`HeaderTemplate`) and where the shared bottom bar is shown
### Station item (`StationCardTemplate`)
### Inicio (`InicioTemplate`)
### Favoritos (`FavoritosTemplate`)
### Reproductor (`ReproductorTemplate`)

## 5. Function map

| Function | Where it is in this theme | Bound to |
|---|---|---|
| Search / clear / filters | | |
| Choose and play a station | | |
| Play / stop | | |
| Status, error, `Reintentar` | | |
| Heart | | |
| Favourites from Reproductor | | |
| Account row, log in / out | | |
| Gear to Ajustes | | |

## 6. Custom controls

| Control | New / reused | Bindable properties | Notes |
|---|---|---|---|

## 7. Motion

<What moves, which state it observes, durations, what reduced motion does.>

## 8. Input and accessibility

<Touch, mouse wheel, keyboard on Windows, focus order, Spanish descriptions, minimum touch targets.>

## 9. View-model needs

<Additive members only, or "none".>

## 10. Open questions and risks

## 11. Manual checks

- [ ] …
