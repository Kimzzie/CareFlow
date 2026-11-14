# CareFlow – Diagnostic Booking Management

**Team:** Kimberly Bautista and (BSIT-3D)
**Course:** ITSD81 Desktop Application Development – Performance Task 4

Desktop app (C#/.NET 10, Avalonia, MVVM, EF Core, SQLite) for booking one to three
diagnostic services for an existing patient, all-or-nothing.

## Requirements
- .NET 10 SDK
- EF Core tools: `dotnet tool install --global dotnet-ef --version 10.0.15`

## Database
- SQLite file: `%LocalAppData%\CareFlow\careflow.db` (created automatically).
- The app applies migrations on startup (`Database.Migrate()`).
- Manual alternative:
  `dotnet ef database update -p CareFlow.Infrastructure -s CareFlow.Desktop`
- Fictional sample data is seeded by the migration: 3 patients and 4 services
  (CBC 5/day, Urinalysis 5, Chest X-Ray 3, ECG 2).
- To reset: close the app and delete `careflow.db`.

## Run
```
dotnet run --project CareFlow.Desktop
```

## Test
```
dotnet test
```

## Architecture
Domain ← Application ← Infrastructure ← Desktop. ViewModels depend only on
`IBookingService`. The transaction boundary is `BookingService.CreateBookingAsync`
(begin transaction → recheck availability → save → commit; any other exit rolls back).

Relational design: see `docs/ERD.png`. Verification: see `docs/VerificationChecklist.md`.

## Known limitations
- Capacity is enforced by a recheck inside a transaction, not by a database constraint.
- `Database.Migrate()` runs synchronously at startup (small SQLite file).
- Full services stay selectable in the UI on purpose, so AC-03 can be demonstrated.

## Acknowledgments
Design guidance and code drafts were produced with AI assistance (Claude), then
reviewed, built, and tested by the team.