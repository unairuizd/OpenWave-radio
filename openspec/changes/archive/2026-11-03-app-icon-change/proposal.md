# Proposal

## Why

The app still uses the default .NET MAUI icon (a purple square with the ".NET" mark), so OpenWave is not recognisable on the Android launcher, in the Windows taskbar or in the window title bar. The project now has its own logo (`LogoFinalOW.png`: a blue play button with a cable and a blue "W" with radio waves) and it should be the app's icon.

## What Changes

- The app icon becomes the OpenWave logo on Android (launcher icon, including round and adaptive masks) and on Windows (executable, taskbar and window icon).
- The logo file is copied into the project's icon resources; the original in the user's Pictures folder is not changed.
- The icon background becomes white, the logo's own background, instead of the template purple.
- No change to the splash screen, the in-app icons, the themes or any behaviour.

## Capabilities

### New Capabilities

- `app-icon`: the image the operating system shows for OpenWave (launcher, taskbar, window) and how it must stay complete and recognisable on every platform mask.

### Modified Capabilities

None.

## Impact

- **App code:** `src/OpenWave.App/Resources/AppIcon/` (the logo as the icon foreground, a white background) and the `MauiIcon` item in `OpenWave.App.csproj`.
- **Unchanged:** splash screen, `Resources/Images` icons, themes, view models, services, the API and the database.
- **Verification:** manual, on Windows and the Android emulator. A reinstall may be needed for the launcher to refresh its cached icon.
