# Tasks

## 1. Flag and strings

- [x] 1.1 Add `Services/WelcomeService.cs` (singleton, `IsSeen` / `MarkSeen()` over the `welcome_seen` preference) and register it in `MauiProgram.cs`; verify `ResetToDefaults()` in `AppearanceService` does not touch it and `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0` passes
- [x] 1.2 Add `Welcome_Title`, `Welcome_Intro` and `Welcome_Continue` to `AppResources.resx` (Spanish) and `AppResources.en.resx` (English); verify both files parse and the Windows build passes
- [x] 1.3 Add the `ScrimColor` default to `Resources/Styles/Styles.xaml`; verify the Windows build passes and no page references a literal scrim colour

## 2. Pop-up view and view model

- [x] 2.1 Add `ViewModels/WelcomeViewModel.cs`: `Languages` (`ChoiceItem` list from `LanguageCatalog.All`), `SelectLanguage` calling `AppearanceService.SetLanguage`, selection refreshed on `Localizer.LanguageChanged`, and `Login` / `Register` / `Continue` commands that close the pop-up with a result; verify the Windows build passes
- [x] 2.2 Add `Views/WelcomePopup.xaml(.cs)` as a toolkit `Popup`: `Border` with `SurfaceColor`, `SizeLimit` with the `Form*` keys, `ScrollView`, title, intro, `Idioma` chips with the `Chip` style, and the three full-width buttons, every text through `{loc:Translate}`, no literal colour, font or size; verify the Windows and Android builds pass
- [x] 2.3 Cap the pop-up height to the window height minus a margin on `Loaded` and window `SizeChanged`, and enable dismissal by the back button and by tapping outside; verify on Windows at the smallest window and `Enorme` that the content scrolls inside the pop-up and nothing is clipped
- [x] 2.4 Register `WelcomeViewModel` and `WelcomePopup` in `MauiProgram.cs`; verify the Windows build passes

## 3. Showing it once from Inicio

- [x] 3.1 In `InicioPage`, show the pop-up from `OnAppearing` when `WelcomeService.IsSeen` is false, after the page is loaded, and mark it seen as soon as it closes, whatever closed it; verify on Windows that a first start (after deleting `preferences.dat`) shows the pop-up over the loading list, that closing it and restarting shows no pop-up, and that killing the app with the pop-up open shows it again
- [x] 3.2 Route the result: `Login` → `login`, `Register` → `login/register`, `Continue` / dismissed → nothing; verify that `Iniciar sesión` from the pop-up opens the login page and returns to `Inicio` on success or on back, that `Crear cuenta` opens the register page, returns to `Inicio` after a successful registration and goes back to `Iniciar sesión` with the back arrow, and that none of these shows the pop-up again
- [x] 3.3 Verify the language part: tapping `English` re-labels the pop-up and the pages behind it at once, the choice survives a restart and shows as selected in `Personalización`, and `Restablecer valores por defecto` returns to Spanish without bringing the pop-up back
- [x] 3.4 Verify the pop-up on the Android emulator after clearing the app's data: it appears over `Inicio`, taps behind it do nothing, the system back button closes it and marks it seen, and at `Enorme` every control is reachable by scrolling inside it

## 4. Documentation and integration check

- [x] 4.1 Add a "Welcome pop-up" section to the README (what it offers, that it is shown once, how to see it again for testing on Windows and Android) and new manual check items for the first start, the account buttons, the language choice and the back button; verify the steps match the app
- [x] 4.2 Manual run in two themes (`Moderno` and one drawn theme) on Windows (smallest window and maximised) and on the Android phone: the pop-up uses the theme's colours and font, meets the width limits, and every text is Spanish by default and English after choosing `English`
- [x] 4.3 Run the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and report the real output
