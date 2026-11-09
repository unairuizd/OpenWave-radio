# Spec Delta

## Purpose

Lets the OpenWave API answer its own texts (error messages and content type labels) in the language the app asks for, with Spanish as the default, so the app can be shown entirely in the user's language.

## ADDED Requirements

### Requirement: Language of the request

The API SHALL read the language of each request from its `Accept-Language` header and SHALL use the first supported language in it. When the header is missing or names no supported language, the API SHALL use Spanish. The first release SHALL support Spanish and English.

#### Scenario: English request

- **WHEN** a request is sent with `Accept-Language: en`
- **THEN** the API answers its own texts in English

#### Scenario: Unsupported language

- **WHEN** a request is sent with `Accept-Language: fr`
- **THEN** the API answers its own texts in Spanish

#### Scenario: No header

- **WHEN** a request is sent without `Accept-Language`
- **THEN** the API answers exactly as before this change, in Spanish

### Requirement: Error messages in the request language

Every ProblemDetails `detail` the API writes SHALL be in the language of the request. Where a requirement of any capability quotes an exact `detail`, that quoted text SHALL be the Spanish text, and the status code SHALL be the same in every language.

#### Scenario: Wrong password in English

- **WHEN** a login with a wrong password is sent with `Accept-Language: en`
- **THEN** the API responds with HTTP 401 and an English `detail` with the same meaning as `Correo o contraseña incorrectos.`

#### Scenario: Duplicate email in Spanish

- **WHEN** a registration with an email that already exists is sent without `Accept-Language`
- **THEN** the API responds with HTTP 409 and the `detail` `El correo ya está registrado.`

### Requirement: Content type labels in the request language

The content type option list SHALL keep the same values in every language and SHALL return the labels in the language of the request. Country, language and genre options SHALL keep the labels the catalogue gives, in every language.

#### Scenario: Content types in English

- **WHEN** `GET /api/catalog/content-types` is sent with `Accept-Language: en`
- **THEN** it returns the values `music`, `news`, `talk` and `sports` with English labels

#### Scenario: Countries in English

- **WHEN** the country list is requested with `Accept-Language: en`
- **THEN** each option has the same value and label as in Spanish

### Requirement: Adding a language to the API

Adding a language to the API SHALL require only a translation file with every API text key and adding its code to the configured list of supported languages. A text missing from a translation SHALL fall back to its Spanish text.

#### Scenario: Missing API translation

- **WHEN** the translation for a supported language lacks one `detail`
- **THEN** that `detail` is returned in Spanish
