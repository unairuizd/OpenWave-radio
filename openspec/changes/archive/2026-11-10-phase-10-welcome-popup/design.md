# Design

## Context

See proposal.md for the motivation. What the pop-up builds on today:

- `AppearanceService` already owns the app language: `SetLanguage(code)` applies it at once through `Localizer` and stores it under `appearance_language`; `LanguageCatalog.All` lists `Español` and `English`. `PersonalizacionViewModel` shows that list as `ChoiceItem` chips.
- The session lives in `SessionService` and is never stored; `LoginPage` (`login` route) returns with `..` on success, and `RegisterPage` (`register` route, reached from the login page) returns with `../..`.
- The app opens on `//inicio`; `InicioPage` is a singleton whose `OnAppearing` runs on every return to it.
- `CommunityToolkit.Maui` 15.0.1 is already referenced and initialised (`UseMauiCommunityToolkit`). It brings a `Popup` host (a view shown over the current page with a scrim, closable by the back button and by tapping outside).
- Every colour, font and size comes from the semantic keys of phase 7, and phase 9 added `Form*` width limits and `SizeLimit`.

## Goals / Non-Goals

**Goals:**

- One small, self-contained view that reuses the language chips and the login pages instead of duplicating them.
- A "seen" flag that is independent of appearance, preferences and session, so no existing reset or logout can bring the pop-up back.
- The pop-up looks like the rest of the app in every theme and text size without new per-theme files.

**Non-Goals:**

- Onboarding beyond these two choices (no theme picker, no tour, no "remind me later").
- Showing the pop-up again from a settings switch.
- Any API or database change.

## Decisions

### 1. Host: the toolkit `Popup`, not a hand-drawn overlay

The pop-up is a `CommunityToolkit.Maui.Views.Popup` (`Views/WelcomePopup.xaml`) shown with the toolkit's `ShowPopupAsync` from `InicioPage`. The toolkit gives the scrim, the centring, the Android back button and "tap outside" dismissal for free, and the pop-up stays out of the eight theme layouts.

*Alternative:* an overlay `Grid` inside `InicioPage`. Rejected: every theme template defines its own `Inicio` page content, so the overlay would have to be added to eight templates (or wrap them), and the back button would need manual handling.

### 2. The "seen" flag lives in its own tiny service

`Services/WelcomeService.cs` (singleton): `bool IsSeen` and `MarkSeen()`, stored in `Preferences` under `welcome_seen`. It is deliberately **not** part of `AppearanceService`, so `ResetToDefaults()` (appearance and language) leaves it alone, and not part of `PreferencesService` (station filters) either.

The flag is written when the pop-up **closes**, whatever closed it. It is not written when the pop-up opens, so an app killed with the pop-up still open shows it again (spec: "Killed while open").

### 3. Trigger: `InicioPage`, once, after the page is on screen

`InicioPage.OnAppearing` already runs `LoadIfNeeded()`. It also calls `_welcome.ShowIfNeededAsync(this)` (a method on the `WelcomeViewModel`/page pairing) which does nothing when `IsSeen` is true, and otherwise shows the pop-up after the page is loaded (`Dispatcher.Dispatch`, so the first layout has happened and the station list keeps loading behind it). Because `OnAppearing` runs again when the user comes back from the login page, the `IsSeen` guard is what prevents a second showing; the flag was set when the pop-up closed to open that page.

### 4. Account buttons close the pop-up, then navigate

The pop-up returns a result (`Login`, `Register`, `Continue` or `null` when dismissed). `InicioPage` marks it seen and then:

- `Login` → `Shell.Current.GoToAsync("login")`. The existing `..` on success returns to `Inicio`.
- `Register` → `Shell.Current.GoToAsync("login/register")`, pushing both pages, so the back stack is the same one the register page already expects: its `../..` on success returns to `Inicio`, and the back arrow goes to `Iniciar sesión` first, as it does today. No change to `RegisterViewModel`.
- `Continue` or `null` → nothing more.

Navigating after closing (and not from inside the pop-up) avoids navigating while a modal is being torn down, which misbehaves on Android.

### 5. Language chips reuse `ChoiceItem` and the `Chip` style

`WelcomeViewModel` exposes `Languages` built from `LanguageCatalog.All` exactly as `PersonalizacionViewModel` does, with `SelectLanguage(item)` calling `AppearanceService.SetLanguage`, and refreshes `IsSelected` on `Localizer.LanguageChanged`. The chips use the existing `Chip` style and its selected/unselected triggers. Every other text uses `{loc:Translate}`, so the pop-up re-labels itself the moment the language changes; language names are their own names and never translate.

### 6. Layout and size limits

Content: a `Border` (`SurfaceColor`, the card corner radius) holding a `controls:SizeLimit` with the `FormMinWidth`/`FormMaxWidth` keys, then a `ScrollView` with a `VerticalStackLayout`: title (`PageTitleLabel`), a one-line intro (`SecondaryLabel`), the `Idioma` group label, the chips in a wrapping `FlexLayout`, then `Iniciar sesión` (`AccentButton`), `Crear cuenta` and `Continuar sin cuenta` (`OutlineButton`). Buttons are full width so their labels can wrap at `Enorme`.

The toolkit sizes a pop-up to its content, so a `ScrollView` inside it needs a height cap: the pop-up's code-behind sets `MaximumHeightRequest` to the window height minus a margin on `Loaded` and on the window's `SizeChanged`. Width is handled by `SizeLimit` plus a horizontal margin, so the pop-up never exceeds the window on the smallest window either.

The scrim colour is a new semantic key `ScrimColor` with a default in `Styles.xaml` (a translucent black); palettes may override it but none needs to. No colour, font or size is hard-coded in the pop-up.

### 7. Strings

New keys in `AppResources.resx` / `.en.resx`: `Welcome_Title` (`Bienvenido a OpenWave`), `Welcome_Intro` (one sentence saying the language can be changed later in `Personalización` and that an account is only needed for favourites), `Welcome_Continue` (`Continuar sin cuenta`). `Idioma`, `Iniciar sesión` and `Crear cuenta` reuse `Personalizacion_Language`, `Page_Login` and `Page_Register`.

## Risks / Trade-offs

- [The toolkit `Popup` API in 15.0.1 differs from older samples (v2 API: `Popup`, `PopupOptions`, `ShowPopupAsync`)] → Check the actual signatures in the referenced package when implementing; if a needed piece (back-button dismissal, result, scrim colour) is missing in that version, stop and ask rather than switching package versions.
- [`DynamicResource` inside a pop-up hosted in a native dialog] → The pop-up is part of the application's visual tree, so application-level resources resolve; verify in two themes on both platforms during the manual run and, if a theme switch while the pop-up is open is not reflected, accept it (the pop-up appears before any theme change can happen in practice).
- [A second `OnAppearing` before the flag is written] → The flag is written synchronously as soon as `ShowPopupAsync` returns, before any navigation starts, so returning from the login page cannot show it again.
- [First start also hits the API] → Independent: the list (or its error message) loads behind the scrim and is visible when the pop-up closes.
- [Testing "first start" repeatedly] → Documented: on Windows delete the app's `preferences.dat`, on Android clear the app's data or reinstall; both reset every other preference too.
