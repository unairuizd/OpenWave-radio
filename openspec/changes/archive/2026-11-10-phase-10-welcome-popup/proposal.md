# Proposal

## Why

A new user opens OpenWave in Spanish and logged out, and nothing tells them that the app also speaks English or that an account lets them keep favourites: both live two taps away in `Ajustes`. A welcome pop-up on the first start puts those two choices in front of the user once, without getting in the way of anonymous listening.

## What Changes

- A **welcome pop-up** opens over `Inicio` the first time the app starts on a device. It has two parts:
  - **Language:** the same language list as `Personalización` (`Español`, `English`). Tapping a language applies it at once (the pop-up re-labels itself) and saves it exactly as `Personalización` does, so it persists until the user changes it in `Personalización`.
  - **Account:** an `Iniciar sesión` button and a `Crear cuenta` button that open the existing pages, plus a `Continuar sin cuenta` button that closes the pop-up.
- The pop-up is shown **only once**: leaving it by any of its three buttons (or by the Android back button / tapping outside) marks it as seen on the device. It never returns, not after a restart, not after logging out, and not after `Restablecer valores por defecto` in `Personalización` (that action resets appearance, not onboarding).
- The pop-up follows the current theme, font and text size (it is drawn with the semantic resource keys), fits the smallest window and the phone at `Enorme` by scrolling its content, and respects the component size limits of phase 9.
- All its texts come from the resource files, in Spanish by default and in English.
- No API change: the pop-up only reads and writes device preferences and navigates to pages that already exist.

## Capabilities

### New Capabilities

- `welcome-prompt`: the one-time welcome pop-up at first start: when it appears, what it offers (language choice and account entry points), how each action leaves it, and that it is never shown again.

### Modified Capabilities

None. The pop-up reuses the behaviour the existing specs already define:

- `app-language` ("Language choice", "Language applied immediately", "Language remembered on the device"): the pop-up writes the same stored language, so those requirements stay as they are.
- `user-session` ("Logging in", "Creating an account", "Anonymous use"): the pop-up opens the existing pages and keeps anonymous use possible.
- `app-navigation` ("Top-level destinations"): the app still opens on `Inicio`; the pop-up is shown on top of it.

## Impact

- **App (`src/OpenWave.App`) only:**
  - New `Views/WelcomePopup` (view) and `ViewModels/WelcomeViewModel`, registered in `MauiProgram.cs`.
  - A new device preference, the "welcome seen" flag, read and written through the existing preferences layer.
  - `InicioPage` (or the shell) triggers the pop-up once after the first page is shown.
  - New strings in `Resources/Strings/AppResources.resx` and `AppResources.en.resx`.
  - README: a "Welcome pop-up" section and manual check items.
- **Dependencies:** none new. `CommunityToolkit.Maui` (already referenced) provides the pop-up host.
- **API, Contracts, database:** unchanged.
