# Spec Delta

## MODIFIED Requirements

### Requirement: Session errors in Spanish

The login and registration pages SHALL show every error in the app language, Spanish by default, and the app SHALL NOT crash. When the API cannot be reached the message SHALL be `No hay conexión con el servidor.`, and when the API reports that the database is unavailable the message SHALL be `No se pudo acceder a la base de datos.` Errors the API explains with its own `detail` SHALL be shown in the app language, as the API returns them for that language.

#### Scenario: API stopped during login

- **WHEN** the API is not running and the user tries to log in
- **THEN** the page shows `No hay conexión con el servidor.`
- **AND** the button is enabled again

#### Scenario: Database unavailable during registration

- **WHEN** the API answers 503 to a registration
- **THEN** the page shows `No se pudo acceder a la base de datos.`

#### Scenario: Wrong password in English

- **WHEN** the app is in `English` and the API rejects a login with a wrong password
- **THEN** the page shows the API's English message for it
