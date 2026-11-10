# Welcome Prompt Specification

## Purpose

Greets a new user once, on the first start of the app on a device, with a pop-up that offers the app language and the way into an account, without blocking anonymous use.

## Requirements

### Requirement: Welcome pop-up on the first start

The first time the app starts on a device, a `Bienvenido a OpenWave` pop-up SHALL open over `Inicio` once `Inicio` is shown. The station list SHALL load behind it as usual. While the pop-up is open, the page behind it SHALL NOT react to taps.

#### Scenario: Fresh install

- **WHEN** the app is started for the first time on a device
- **THEN** `Inicio` is shown and the `Bienvenido a OpenWave` pop-up is open on top of it
- **AND** the station list loads behind it

#### Scenario: Tap behind the pop-up

- **WHEN** the pop-up is open and the user taps where a station card is
- **THEN** no station is selected and `Reproductor` does not open

### Requirement: Language choice in the pop-up

The pop-up SHALL show an `Idioma` choice with the same languages, names, order and default as `Personalización`. Choosing a language SHALL apply it at once to the pop-up and the whole app, and SHALL store it as the app language, so it is kept exactly as a language chosen in `Personalización`.

#### Scenario: Choosing English in the pop-up

- **WHEN** the user taps `English` in the pop-up
- **THEN** the pop-up texts change to English at once, including its title and buttons
- **AND** the pages behind it are in English too

#### Scenario: Language kept after the pop-up

- **WHEN** the user chose `English` in the pop-up, closed it and restarts the app
- **THEN** the app opens in English, with no pop-up
- **AND** `Personalización` shows `English` selected

#### Scenario: Changing it later

- **WHEN** the user later selects `Español` in `Personalización`
- **THEN** the app is in Spanish again and the pop-up does not come back

### Requirement: Account entry points in the pop-up

The pop-up SHALL offer an `Iniciar sesión` button, a `Crear cuenta` button and a `Continuar sin cuenta` button. `Iniciar sesión` SHALL close the pop-up and open the existing `Iniciar sesión` page; `Crear cuenta` SHALL close the pop-up and open the existing `Crear cuenta` page; `Continuar sin cuenta` SHALL close the pop-up and leave the user on `Inicio`, logged out.

#### Scenario: Log in from the pop-up

- **WHEN** the user taps `Iniciar sesión` in the pop-up
- **THEN** the pop-up closes and the `Iniciar sesión` page opens
- **WHEN** the user logs in successfully
- **THEN** the app returns to `Inicio`, logged in, with no pop-up

#### Scenario: Create an account from the pop-up

- **WHEN** the user taps `Crear cuenta` in the pop-up
- **THEN** the pop-up closes and the `Crear cuenta` page opens
- **WHEN** the user registers successfully
- **THEN** the app returns to `Inicio`, logged in, with no pop-up

#### Scenario: Back from the login page without logging in

- **WHEN** the user opened `Iniciar sesión` from the pop-up and goes back without logging in
- **THEN** `Inicio` is shown, logged out, with no pop-up

#### Scenario: Continue without an account

- **WHEN** the user taps `Continuar sin cuenta`
- **THEN** the pop-up closes, `Inicio` stays logged out and the user can select and play a station

### Requirement: Shown only once per device

Leaving the pop-up by any means (one of its three buttons, the Android back button or a tap outside it) SHALL mark it as seen on the device. From then on the app SHALL NOT show it again: not after a restart, not after logging out, and not after `Restablecer valores por defecto` in `Personalización`. Closing the app while the pop-up is still open SHALL NOT mark it as seen.

#### Scenario: Second start

- **WHEN** the user closed the pop-up once and starts the app again
- **THEN** `Inicio` is shown with no pop-up

#### Scenario: Back button on Android

- **WHEN** the pop-up is open on Android and the user presses the system back button
- **THEN** the pop-up closes, the user stays on `Inicio`, and the pop-up is not shown on the next start

#### Scenario: Killed while open

- **WHEN** the app is closed from the task switcher while the pop-up is open
- **THEN** the pop-up is shown again on the next start

#### Scenario: Reset of the appearance

- **WHEN** the user activates `Restablecer valores por defecto` in `Personalización` and restarts the app
- **THEN** the app opens in `Español` with the default theme and no pop-up

### Requirement: Pop-up follows the appearance

The pop-up SHALL be drawn with the current theme's colours, font and text size, with the same contrast guarantees as the themed pages. At every text size and window size down to the minimum, every control of the pop-up SHALL be reachable by scrolling inside it, and the pop-up SHALL stay within the width limits of a form.

#### Scenario: Enorme on a phone

- **WHEN** the stored text size is `Enorme` and the app starts for the first time on an Android phone
- **THEN** the pop-up shows its title, the language choice and the three buttons, scrolling inside the pop-up if needed, with no clipped text

#### Scenario: Smallest window

- **WHEN** the Windows window is at its minimum size and the pop-up is open
- **THEN** every control of the pop-up can be reached and nothing scrolls sideways

#### Scenario: Wide window

- **WHEN** the Windows window is maximised and the pop-up is open
- **THEN** the pop-up is centred and no wider than a form's maximum width

### Requirement: Pop-up texts in the app language

Every text of the pop-up SHALL come from the app's resource files, in Spanish by default and in English, following the same rule as every other text of the app.

#### Scenario: Spanish texts

- **WHEN** the app starts for the first time with no stored language
- **THEN** the pop-up reads `Bienvenido a OpenWave`, `Idioma`, `Iniciar sesión`, `Crear cuenta` and `Continuar sin cuenta`

#### Scenario: English texts

- **WHEN** `English` is selected in the pop-up
- **THEN** every text of the pop-up shows its English equivalent
