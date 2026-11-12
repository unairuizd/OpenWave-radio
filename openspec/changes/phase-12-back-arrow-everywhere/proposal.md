# Proposal

## Why

`Iniciar sesión` and `Crear cuenta` are the only pages that keep the platform's own navigation bar: a small system back button drawn by Windows or Android, not by the theme, next to a title the app styles by hand. Every other page already has the app's own way out: the bottom bar (`Inicio`, `Reproductor`, `Favoritos`) or the shared settings header with its back arrow (`Ajustes`, `Personalización`, `Preferencias`, the filter pages, `Curiosidades`). The account pages should look and behave like the rest.

## What Changes

- **`Iniciar sesión` and `Crear cuenta` get the app's own header**: the shared settings header with the back arrow on the left and the title centred, **without the gear** and without a bottom bar. The platform navigation bar is hidden on both pages, so there is one arrow, drawn with the theme's colours, font and size.
- **The arrow goes back one page**, exactly as on the settings pages: from `Iniciar sesión` to the page that opened it (`Favoritos`, `Inicio`, `Curiosidades`, the welcome pop-up's page); from `Crear cuenta` to `Iniciar sesión`. The Android back button and the Windows window arrow keep doing the same.
- The duplicated in-body title (`Iniciar sesión` / `Crear cuenta` as the first label of the form) is removed, since the header now shows it.
- No page is left without an exit: after this change every page shows either the bottom bar or a header with the back arrow.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `app-navigation`: "Common header" (the account pages show the shared header without the gear instead of the platform header) and "Back arrow on the settings pages" (the arrow of `Iniciar sesión` and `Crear cuenta`, and the guarantee that every page has a bottom bar or a back arrow).

## Impact

- **App only (`src/OpenWave.App`):** `Views/SettingsHeader.cs` gains a `ShowGear` option; `Views/LoginPage.xaml` and `Views/RegisterPage.xaml` hide the platform bar, add the header and lose the in-body title; `Services/ShellChrome.cs` and `AppShell.xaml.cs` drop the title-view code that only served the platform header.
- **README:** the "Navigation and layout" paragraph about the login pages and the manual check items.
- **API, Contracts, database, strings:** unchanged (`Nav_Back`, `Page_Login` and `Page_Register` already exist).
