# User Accounts Specification

## Purpose

Lets people create an OpenWave account and log in to it through the API, with unique email addresses and passwords that are never stored or exposed in plain text.

## Requirements

### Requirement: Registering an account

The API SHALL expose `POST /api/auth/register`, accepting a JSON body with `email` and `password`. When the input is valid and the email is not registered, the API SHALL create the user and respond with HTTP 201 and a body containing the new user's `id` and normalised `email`.

#### Scenario: Successful registration

- **WHEN** `POST /api/auth/register` is sent with `{"email":"ana@example.com","password":"secreta123"}` and that email is not registered
- **THEN** the response status is 201
- **AND** the body contains a numeric `id` and `email` equal to `ana@example.com`
- **AND** the body does not contain the password or its hash

### Requirement: Email normalisation

The API SHALL trim surrounding whitespace from email addresses and convert them to lower case before storing them and before comparing them, for both registration and login.

#### Scenario: Registering with mixed case and spaces

- **WHEN** a user registers with the email `"  Ana@Example.COM "`
- **THEN** the stored and returned email is `ana@example.com`

#### Scenario: Logging in with a different letter case

- **WHEN** a user registered as `ana@example.com` logs in with `ANA@example.com` and the correct password
- **THEN** the login succeeds

### Requirement: Unique email addresses

The system SHALL keep email addresses unique, ignoring letter case and surrounding whitespace. Registering an email that already exists SHALL return HTTP 409 with a ProblemDetails `detail` of exactly `El correo ya está registrado.` and SHALL NOT create or change any user.

#### Scenario: Registering an existing email in another letter case

- **WHEN** `ana@example.com` is registered and `POST /api/auth/register` is sent with `ANA@Example.com` and a valid password
- **THEN** the response status is 409
- **AND** the `detail` is `El correo ya está registrado.`
- **AND** the number of stored users does not change

### Requirement: Registration input validation

The API SHALL reject registration and login requests whose email is missing, longer than 255 characters or not a valid email format, and registration requests whose password has fewer than 8 characters. Invalid input SHALL return HTTP 400 with a ProblemDetails response whose messages are in Spanish, and nothing SHALL be stored.

#### Scenario: Invalid email format

- **WHEN** `POST /api/auth/register` is sent with the email `ana-at-example`
- **THEN** the response status is 400
- **AND** the response contains the message `El correo no es válido.`

#### Scenario: Password too short

- **WHEN** `POST /api/auth/register` is sent with a valid email and the password `corta`
- **THEN** the response status is 400
- **AND** the response contains the message `La contraseña debe tener al menos 8 caracteres.`
- **AND** no user is created

### Requirement: Logging in

The API SHALL expose `POST /api/auth/login`, accepting `email` and `password`. When they match a stored user, it SHALL respond with HTTP 200 and the user's `id` and `email`. When the email is unknown or the password is wrong, it SHALL respond with HTTP 401 and a ProblemDetails `detail` of exactly `Correo o contraseña incorrectos.`, using the same response for both cases.

#### Scenario: Correct credentials

- **WHEN** a registered user logs in with the correct email and password
- **THEN** the response status is 200
- **AND** the body contains that user's `id` and `email`

#### Scenario: Wrong password

- **WHEN** a registered email is sent with a wrong password
- **THEN** the response status is 401
- **AND** the `detail` is `Correo o contraseña incorrectos.`

#### Scenario: Unknown email

- **WHEN** an email that is not registered is sent with any password
- **THEN** the response status is 401
- **AND** the response is identical to the wrong-password response

### Requirement: Passwords stored only as hashes

The system SHALL store only a salted one-way hash of each password. Plain-text passwords SHALL NOT be stored, returned in any response, or written to logs.

#### Scenario: Inspecting a stored user

- **WHEN** a user registers with the password `secreta123`
- **THEN** the stored password value is not `secreta123` and does not contain it
- **AND** two users registered with the same password have different stored values

#### Scenario: Reviewing the API logs

- **WHEN** registration and login requests have been processed
- **THEN** no log entry contains a submitted password

### Requirement: Users persisted in the remote database

The API SHALL store users in the remote MySQL database, so that accounts remain available after the API restarts.

#### Scenario: Logging in after an API restart

- **WHEN** a user registers, the API is stopped and started again, and the user logs in with the same credentials
- **THEN** the login succeeds with the same `id`

### Requirement: Database unavailable during authentication

When the database cannot be reached while registering or logging in, the API SHALL respond with HTTP 503 and a ProblemDetails `detail` of exactly `No se pudo acceder a la base de datos.`, and SHALL log the technical cause without any password.

#### Scenario: Database is unreachable

- **WHEN** the database cannot be reached and `POST /api/auth/login` is sent
- **THEN** the response status is 503
- **AND** the `detail` is `No se pudo acceder a la base de datos.`
- **AND** the API keeps running
