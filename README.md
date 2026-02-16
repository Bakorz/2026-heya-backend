# CampusRooms API

## Authentication

- `POST /api/auth/register`
  - Public endpoint for creating a `User` account.
  - Request body:
    - `name`
    - `nrp`
    - `email`
    - `password`
  - Password is stored as a secure hash (PBKDF2).

## Roles

- Available roles:
  - `User`
  - `Admin`
- Registration endpoint always creates role `User`.
- `Admin` accounts are manual-only (not creatable via public API endpoint).
