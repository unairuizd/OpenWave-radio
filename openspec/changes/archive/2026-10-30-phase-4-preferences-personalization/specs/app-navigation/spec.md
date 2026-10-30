# Spec Delta

## RENAMED Requirements

- FROM: `### Requirement: Three top-level destinations`
- TO: `### Requirement: Top-level destinations`

## MODIFIED Requirements

### Requirement: Top-level destinations

The app SHALL present exactly four top-level destinations, in this order: `Inicio`, `Reproductor`, `Favoritos` and `Ajustes`. On Android they SHALL be reachable from a bottom tab bar; on Windows they SHALL be reachable from an equivalent always-visible tab control. The app SHALL open on `Inicio`.

#### Scenario: App start

- **WHEN** the app starts
- **THEN** the `Inicio` destination is shown
- **AND** all four destinations `Inicio`, `Reproductor`, `Favoritos` and `Ajustes` are visible in that order

#### Scenario: Switching destinations

- **WHEN** the user selects `Reproductor`, `Favoritos` or `Ajustes`
- **THEN** the corresponding page is shown without restarting the app
- **AND** the user can return to any other destination from the same control

#### Scenario: Returning to a destination

- **WHEN** the user leaves a destination and comes back to it
- **THEN** the destination is shown again with its current state, without requiring an app restart

## ADDED Requirements

### Requirement: Ajustes page

The `Ajustes` destination SHALL list two entries, `Preferencias` and `Personalización`. Activating an entry SHALL open its page inside the `Ajustes` tab, and a back action SHALL return to `Ajustes`. The login state and the `Cerrar sesión` action SHALL stay in `Favoritos`.

#### Scenario: Opening Personalización

- **WHEN** the user opens `Ajustes` and activates `Personalización`
- **THEN** the `Personalización` page is shown
- **WHEN** the user goes back
- **THEN** `Ajustes` is shown again

#### Scenario: Opening Preferencias

- **WHEN** the user opens `Ajustes` and activates `Preferencias`
- **THEN** the `Preferencias` page is shown, and going back returns to `Ajustes`
