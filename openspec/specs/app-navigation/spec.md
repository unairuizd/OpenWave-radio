# App Navigation Specification

## Purpose

Defines the three top-level destinations of the app and how the user moves between them, so every later feature has a stable place to live.

## Requirements

### Requirement: Three top-level destinations

The app SHALL present exactly three top-level destinations, in this order: `Inicio`, `Reproductor` and `Favoritos`. On Android they SHALL be reachable from a bottom tab bar; on Windows they SHALL be reachable from an equivalent always-visible tab control. The app SHALL open on `Inicio`.

#### Scenario: App start

- **WHEN** the app starts
- **THEN** the `Inicio` destination is shown
- **AND** all three destinations `Inicio`, `Reproductor` and `Favoritos` are visible in that order

#### Scenario: Switching destinations

- **WHEN** the user selects `Reproductor` or `Favoritos`
- **THEN** the corresponding page is shown without restarting the app
- **AND** the user can return to any other destination from the same control

#### Scenario: Returning to a destination

- **WHEN** the user leaves a destination and comes back to it
- **THEN** the destination is shown again with its current state, without requiring an app restart
