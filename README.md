# Project Title

HeyaAPI

## Description

HeyaAPI is the backend service for room booking management in a campus environment. It exists to manage rooms, booking requests, admin approval workflow, authentication, audit logs, and analytics.

## Features

- User registration and login endpoints
- Role model with `User` and `Admin`
- Room management (`create`, `update`, `activate/deactivate`, `list`)
- Booking request creation with recurrence support
- Approval workflow (`pending`, `approved`, `rejected`)
- Conflict checking for approved bookings
- Audit event recording and analytics endpoints

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core
- SQLite
- C#

## Installation

1. Open a terminal in the backend directory:

- `cd backend/CampusRooms.Api`

2. Restore dependencies:

- `dotnet restore`

3. Build the project:

- `dotnet build`

## Usage

1. Run the API:

- `dotnet run`

2. The API starts using launch settings
3. Main endpoint prefix is `/api`.

Example endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/rooms`
- `POST /api/requests`
- `GET /api/approvals/queue`
- `POST /api/approvals/{requestId}/decide`
