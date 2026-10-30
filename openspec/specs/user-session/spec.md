# User Session Specification

## Purpose

Lets a user log in, create an account and log out in the app, keeping the session only in memory while the app is open, without blocking anonymous browsing and listening.

## Requirements

### Requirement: Session kept in memory

While the app is open, a successful login or registration SHALL keep the user logged in across all screens. The session SHALL NOT be saved: every app start SHALL begin logged out.

#### Scenario: Navigating while logged in

- **WHEN** the user logs in and switches between `Inicio`, `Reproductor` and `Favoritos`
- **THEN** the user stays logged in on every screen

#### Scenario: Restarting the app

- **WHEN** a logged-in user closes the app and opens it again
- **THEN** the app starts logged out

### Requirement: Anonymous use

A user who is not logged in SHALL still be able to browse, search, select and play stations.

#### Scenario: Listening without an account

- **WHEN** the app starts and the user selects a station in `Inicio` without logging in
- **THEN** `Reproductor` opens and the station plays

### Requirement: Logging in

The app SHALL provide an `Iniciar sesión` page with email and password fields, an `Iniciar sesión` button and a way to open `Crear cuenta`. Before sending, it SHALL check the same rules as the API (valid email, password not empty). While the request runs, the button SHALL be disabled and a loading indicator shown. On success, the app SHALL log the user in and return to the screen that opened the page.

#### Scenario: Successful login

- **WHEN** the user enters a registered email and the correct password and activates `Iniciar sesión`
- **THEN** a loading indicator is shown and the button is disabled while the request runs
- **AND** the user is logged in and returned to the previous screen

#### Scenario: Wrong credentials

- **WHEN** the API rejects the credentials
- **THEN** the page shows `Correo o contraseña incorrectos.`
- **AND** the entered email is kept

#### Scenario: Invalid email entered

- **WHEN** the user enters `ana-at-example` and activates `Iniciar sesión`
- **THEN** the page shows `El correo no es válido.` without contacting the API

### Requirement: Creating an account

The app SHALL provide a `Crear cuenta` page with email, password and confirm-password fields and a `Crear cuenta` button. Before sending, it SHALL check that the email is valid, the password has at least 8 characters and both passwords match. On success, the app SHALL log the new user in straight away and return to the screen that opened the login flow.

#### Scenario: Successful registration

- **WHEN** the user enters a new valid email and two matching passwords of at least 8 characters and activates `Crear cuenta`
- **THEN** the account is created
- **AND** the user is logged in and returned to the screen that opened the login flow

#### Scenario: Passwords do not match

- **WHEN** the two password fields differ
- **THEN** the page shows `Las contraseñas no coinciden.` without contacting the API

#### Scenario: Email already registered

- **WHEN** the API reports that the email is already registered
- **THEN** the page shows `El correo ya está registrado.`

### Requirement: Session errors in Spanish

The login and registration pages SHALL show every error in Spanish and the app SHALL NOT crash. When the API cannot be reached the message SHALL be `No hay conexión con el servidor.`, and when the API reports that the database is unavailable the message SHALL be `No se pudo acceder a la base de datos.`

#### Scenario: API stopped during login

- **WHEN** the API is not running and the user tries to log in
- **THEN** the page shows `No hay conexión con el servidor.`
- **AND** the button is enabled again

#### Scenario: Database unavailable during registration

- **WHEN** the API answers 503 to a registration
- **THEN** the page shows `No se pudo acceder a la base de datos.`

### Requirement: Logging out

While logged in, the app SHALL show the logged-in email and a `Cerrar sesión` action in `Favoritos`. Activating `Cerrar sesión` SHALL end the session and forget the user's favourites in the app. Logging out SHALL NOT stop playback or change the selected station.

#### Scenario: Logging out

- **WHEN** a logged-in user opens `Favoritos`
- **THEN** the page shows the user's email and `Cerrar sesión`
- **WHEN** the user activates `Cerrar sesión`
- **THEN** the user is logged out and `Favoritos` shows the logged-out state
- **AND** no station is shown as a favourite in any screen

#### Scenario: Selected station after logging out

- **WHEN** a station is selected in `Reproductor` and the user logs out in `Favoritos`
- **THEN** `Reproductor` still shows the same station
- **AND** the user can play it without logging in again
