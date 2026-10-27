# Spec Delta

## Purpose

Lets the developer and the app confirm that the backend is running and reachable before any station data is requested.

## ADDED Requirements

### Requirement: Health endpoint

The API SHALL expose `GET /api/health`. When the API is running, the endpoint SHALL respond with HTTP 200 and a JSON body containing `status` with the value `ok` and `timeUtc` with the current server time in ISO 8601 UTC format.

#### Scenario: API running

- **WHEN** the API is running and `GET /api/health` is requested
- **THEN** the response status is 200
- **AND** the response body has `status` equal to `ok`
- **AND** `timeUtc` is a valid ISO 8601 UTC timestamp

#### Scenario: Health check does not depend on other services

- **WHEN** the API is running without a database connection or internet access
- **THEN** `GET /api/health` still returns 200 with `status` equal to `ok`
