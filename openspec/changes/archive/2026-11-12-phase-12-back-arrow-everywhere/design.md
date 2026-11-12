# Design

## Context

See proposal.md for the motivation. Today:

- `Views/SettingsHeader.cs` is a shared `ContentView`: back arrow (`AppNavigation.GoBackCommand`, which does `GoToAsync("..")` when the stack has more than one page), the page title bound to `Page.Title`, the gear (`AppNavigation.OpenAjustesCommand`), a divider; colours, font and height come from the semantic keys (`ChromeMin/MaxHeight`, `PrimaryTextColor`, `DividerColor`). Every settings-style page uses it inside a `Grid` with `RowDefinitions="Auto,*,Auto"` and `Shell.NavBarIsVisible="False"`.
- `LoginPage` and `RegisterPage` are the only pages that leave `Shell.NavBarIsVisible` at its default: the platform bar draws the system back button, and `ShellChrome.EnsureTitleView` (called from `AppShell.RefreshChrome` on every navigation) gives that bar a `TitleView` label styled like the app. Both pages also repeat their title as the first `Label` of the form.
- Navigation into the account pages: `login` is pushed from `Favoritos` (heart or button), the `Inicio` strip, `Curiosidades` and the welcome pop-up; `register` from the login page or as `login/register` from the pop-up. `LoginViewModel` returns with `..` and `RegisterViewModel` with `../..` on success.

## Goals / Non-Goals

**Goals:**

- One header component for every pushed page; the account pages differ only in hiding the gear.
- No change to how the login flow is reached or how it returns on success.

**Non-Goals:**

- A bottom bar on the account pages (they are a flow, not a destination).
- Touching the themed pages' headers or the Windows title-bar arrow.

## Decisions

### 1. `SettingsHeader.ShowGear`, default `true`

A bindable `ShowGear` property; `false` hides the gear (`IsVisible = false`) and keeps its column so the title stays centred on the page, as it is on the other pages. The account pages declare `<views:SettingsHeader ShowGear="False" />`. *Alternative:* a second header class. Rejected: the only difference is one button.

### 2. Account pages adopt the settings-page skeleton

`LoginPage` and `RegisterPage` set `Shell.NavBarIsVisible="False"` and `BackgroundColor="{DynamicResource PageBackgroundColor}"`, wrap their content in `Grid RowDefinitions="Auto,*"` with the header in row 0 and the existing `ScrollView` → `SizeLimit` form in row 1, and drop the `PageTitleLabel` at the top of the form (the header shows the title). Nothing else in the forms, view models or routes changes: `GoBackCommand` already pops one page, which is the page that opened the flow for `login` and the login page for `register`.

### 3. Remove the title-view code

With no page left on the platform bar, `ShellChrome.EnsureTitleView` and its call in `AppShell.RefreshChrome` (and the `OnNavigated` override that only existed for it, if nothing else uses it) are removed, and the class comment is reduced to the radio-button colours. `TitleBarText` (Windows window title) stays: it is the window, not a page bar.

### 4. Android back button

Shell handles the hardware back on pushed pages already (it pops), and `LoginPage`/`RegisterPage` keep being pushed routes, so no handler is needed; the manual run confirms it.

## Risks / Trade-offs

- [The Windows window still shows its own back arrow in the title bar on pushed pages] → unchanged behaviour, documented in the README; it does the same pop.
- [A page that pushes `login` while the stack is already deep] → `..` pops exactly one page, as before; no change in behaviour.
- [Removing `EnsureTitleView` breaks a page I missed] → the inventory shows every page sets `Shell.NavBarIsVisible="False"` after this change; the build and the manual run cover it.
