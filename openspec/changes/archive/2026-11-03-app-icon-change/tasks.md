# Tasks

All work happens in the `feature/app-icon` worktree (`OpenWave-app-icon`). Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android -p:AndroidSdkDirectory=$env:LOCALAPPDATA\Android\Sdk`) and a short summary.

## 1. Icon resources

- [x] 1.1 Copy `C:\Users\unair\Pictures\Screenshots\LogoFinalOW.png` to `src/OpenWave.App/Resources/AppIcon/appiconfg.png` and delete `appiconfg.svg`; verify the copy is byte-identical to the original and that the original is unchanged
- [x] 1.2 Change the fill of `Resources/AppIcon/appicon.svg` to `#FFFFFF`, and the `MauiIcon` item in `OpenWave.App.csproj` to `ForegroundFile="Resources\AppIcon\appiconfg.png"` with `ForegroundScale="0.7"` and `Color="#FFFFFF"`; leave `MauiSplashScreen` unchanged
- [x] 1.3 Run the three builds with 0 errors; verify that the generated Android launcher layers (`obj/.../resizetizer`) and the Windows `appicon.ico` contain the logo, and that the drawing of the adaptive foreground stays inside the central 66%

## 2. Verification on the devices

- [x] 2.1 Windows: run the app and check the taskbar button, the title bar and the `.exe` in File Explorer show the logo on white (clear the icon cache if the old icon is still shown)
- [x] 2.2 Android emulator: uninstall the app, deploy it again, and check the app drawer and the home screen show the logo on white; switch the launcher icon shape to circle (or use a round-icon launcher) and check no part of the logo is cut off
- [x] 2.3 Add a short note to the README (Solution layout or a "App icon" line) saying the icon is the OpenWave logo in `Resources/AppIcon/appiconfg.png`

## Workflow follow-up

- Archive the change with `/opsx:archive` after the developer has reviewed it.
- Committing and merging `feature/app-icon` into `main` is left to the developer.
