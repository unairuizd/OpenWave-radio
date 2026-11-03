# App Icon Specification

## Purpose

Defines the icon that the operating system shows for OpenWave, so the app is recognisable on the Android launcher and on Windows and its logo is never cut off by a platform's icon shape.

## Requirements

### Requirement: OpenWave logo as the app icon

The app icon SHALL be the OpenWave logo (the blue play button with a cable and the blue "W" with radio waves) on a white background, on Android and on Windows. The default .NET MAUI icon SHALL no longer appear anywhere the operating system shows the app.

#### Scenario: Android launcher

- **WHEN** the app is installed on Android and the user opens the app drawer
- **THEN** OpenWave is shown with the OpenWave logo on white

#### Scenario: Windows

- **WHEN** the app runs on Windows
- **THEN** the taskbar button and the Alt+Tab switcher show the OpenWave logo (the app's title bar shows only its name, with no icon)
- **AND** the executable file shows the same icon in File Explorer

### Requirement: Logo complete in every icon shape

The whole logo SHALL stay visible inside every shape the platform applies to the icon (circle, rounded square, squircle on Android; square on Windows): no part of the play button, the cable, the "W" or the waves SHALL be cut off.

#### Scenario: Round launcher icon

- **WHEN** the Android launcher shows icons as circles
- **THEN** the play button, the cable, the "W" and the waves are all fully inside the circle
