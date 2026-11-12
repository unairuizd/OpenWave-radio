# Tasks

## 1. Header option

- [x] 1.1 Add the `ShowGear` bindable property to `Views/SettingsHeader.cs` (default `true`; `false` hides the gear but keeps its column so the title stays centred); verify `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0` passes and the existing settings pages look unchanged

## 2. Account pages

- [x] 2.1 Rework `Views/LoginPage.xaml`: `Shell.NavBarIsVisible="False"`, page background from `PageBackgroundColor`, a `Grid` with the header (`ShowGear="False"`) in row 0 and the existing form in row 1, and remove the in-body title label; verify the Windows build passes and the page shows a single themed back arrow with `Iniciar sesión` centred
- [x] 2.2 The same for `Views/RegisterPage.xaml` with `Crear cuenta`; verify the Windows and Android builds pass
- [x] 2.3 Remove `ShellChrome.EnsureTitleView` and its call from `AppShell` (keep the radio colour code and `TitleBarText`); verify the three builds pass and no page shows a platform bar on Windows

## 3. Verification and documentation

- [x] 3.1 Verify on Windows: from `Favoritos` logged out, `Iniciar sesión` → arrow returns to `Favoritos`; from the `Inicio` strip → arrow returns to `Inicio`; `Crear cuenta` → arrow returns to `Iniciar sesión` with the typed email kept; after a successful login or registration the app still returns to the opener; in two themes (`Moderno` and one drawn theme) the header takes the theme's colours and font; at `Enorme` in the smallest window the header and form stay usable
- [x] 3.2 Verify on the Android emulator: the account pages show the themed header and no platform bar; the arrow and the system back button pop one page each (`Crear cuenta` → `Iniciar sesión` → opener)
- [x] 3.3 Update the README ("Navigation and layout": the account pages now show the shared header without the gear; check items 71 and 73 mention the themed arrow) and add a check item for the account pages' header and arrow in two themes and on Android; verify the steps match the app
- [x] 3.4 Run the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and report the real output
