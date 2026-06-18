# ParkingManager

ParkingManager is an ASP.NET Core application for parking operations with:

- Blazor dashboard UI
- Minimal API backend
- PostgreSQL persistence via EF Core
- Billing, rainy discount logic, and monthly reporting

## Current status

- App project target: .NET 8 (`src/ParkingManager.Api`)
- Test project target: .NET 10 (`tests/ParkingManager.Api.Tests`)
- Persistence: PostgreSQL (`parking_manager_dev`)
- Dashboard duplication issue fixed by removing duplicated router markup in `src/ParkingManager.Api/Components/Routes.razor`
- HTTPS configured and working on port `5000`

## Implemented features

- Vehicle entry and automatic space assignment
- Inventory totals and per-floor availability (covered/uncovered)
- Time-based billing
- Rain promotion for uncovered spaces (50% discount when rainy exposure is at least 33% at payment time)
- Payment registration and 10-minute grace period for exit
- Exit validation with additional amount when grace period is exceeded
- Monthly report with revenue/discount/occupancy metrics

## Tech stack

- ASP.NET Core minimal APIs + Blazor Server components
- EF Core 8 + Npgsql provider
- PostgreSQL
- Swagger / OpenAPI
- xUnit tests

## Prerequisites

1. .NET SDK 8+ installed
2. PostgreSQL running locally on port `5432`
3. Database credentials matching `src/ParkingManager.Api/appsettings.json`

Current connection string:

```json
Host=localhost;Port=5432;Database=parking_manager_dev;Username=postgres;Password=postgres
```

## Quick Start

Copy/paste these commands from repository root.

Start PostgreSQL (Docker):

```powershell
docker run --name parkingmanager-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=parking_manager_dev -p 5432:5432 -d postgres:16
```

If the container already exists, start it again:

```powershell
docker start parkingmanager-postgres
```

Run the application:

```powershell
dotnet run --project src/ParkingManager.Api/ParkingManager.Api.csproj
```

Open:

- Dashboard: `https://localhost:5000/`
- Swagger: `https://localhost:5000/swagger`

## Quick Start (No Docker, Windows PostgreSQL Service)

Use this path if PostgreSQL is installed directly on Windows.

Start PostgreSQL service (service name may vary):

```powershell
Get-Service *postgres* | Select-Object Name, Status
Start-Service -Name postgresql-x64-16
```

Create the application database (run once):

```powershell
psql -U postgres -h localhost -p 5432 -c "CREATE DATABASE parking_manager_dev;"
```

Run the application:

```powershell
dotnet run --project src/ParkingManager.Api/ParkingManager.Api.csproj
```

Open:

- Dashboard: `https://localhost:5000/`
- Swagger: `https://localhost:5000/swagger`

## Run the app

From repository root:

```powershell
dotnet run --project src/ParkingManager.Api/ParkingManager.Api.csproj
```

App URLs:

- Dashboard/UI: `https://localhost:5000/`
- Swagger UI: `https://localhost:5000/swagger`
- HTTP endpoint (non-TLS): `http://localhost:5001/`

## API endpoints

- `GET /api/inventory`
- `GET /api/sessions?take=12`
- `POST /api/sessions/entry`
- `GET /api/sessions/{sessionId}`
- `POST /api/sessions/{sessionId}/payment`
- `POST /api/sessions/{sessionId}/exit`
- `POST /api/weather/interval`
- `GET /api/reports/monthly?year=2026&month=6`

## Run tests

```powershell
dotnet test tests/ParkingManager.Api.Tests/ParkingManager.Api.Tests.csproj
```

Included tests cover:

- rainy discount application
- grace-period exit validation
- monthly report revenue/discount assertions

## Troubleshooting notes

### Port already in use (`5000` or `5001`)

Stop old process and rerun `dotnet run`.

### SSL error on `https://localhost:5000`

The app expects HTTPS on `5000`. If browser cert issues appear, trust/regenerate dev certs.

### Blazor dashboard renders duplicated sections

Ensure `src/ParkingManager.Api/Components/Routes.razor` contains only a single `<Router ...>` block.