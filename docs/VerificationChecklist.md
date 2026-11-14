# Verification Checklist

Verified on Nov 14, 2026. Automated tests: `dotnet test` → 6 passed, 0 failed.
Manual checks were done in the running app, and persisted data was checked in DB Browser for SQLite.

| ID | How checked | Evidence | Done |
|---|---|---|---|
| AC-01 | Test `Availability_IsCalculatedFromPersistedBookings`; in the app, booked on Nov 15 and switched dates | After booking, CBC showed "3 of 5 left" on Nov 15 and "5 of 5 left" on Nov 14 (capacity is per date) | [x] |
| AC-02 | Test `MultiServiceBooking_RecordsOneBookingWithEveryService`; booked 3 services in the app; checked the database | `Bookings` count = 4; items per booking = 1, 2, 3, 1 (booking #3 has 3 items) | [x] |
| AC-03 | Tests `FullService_RejectsWholeRequest_NothingSaved` and `InvalidRequests_AreRejected_NothingSaved`; in the app, requested CBC + Urinalysis + ECG with ECG full, and tried a past date (Nov 13, 2026) | Message "Fully booked ... ECG. Nothing was booked."; CBC and Urinalysis counts unchanged. Past date showed "The booking date cannot be earlier than today." `Bookings` count stayed at 4 after both rejections. Other invalid inputs (no services, duplicate service, unknown patient) verified by automated test | [x] |
| AC-04 | Test `StaleScreen_IsRevalidated_NoOverbooking` (controlled stale-availability scenario) | The screen's old "1 left" was rechecked at confirm time; ECG items in the database = 2 (capacity 2), never 3 | [x] |
| AC-05 | Test `FailureBeforeCommit_RollsBackEverything`; source: `await using` transaction in `BookingService.CreateBookingAsync`, `CommitAsync` is the last step | After a failure injected after the rows were written, 0 bookings and 0 items remained | [x] |
| AC-06 | Manual: confirmed bookings, closed the app completely, reopened it | After restart, bookings #1 to #4 were still listed; selecting one showed its patient, date and services | [x] |

## Not verified / limitations
- AC-03: invalid inputs other than the past date (no services, duplicate service, unknown patient) were verified by automated test, not manually in the UI.
- AC-04 was verified by a controlled test, not with two real simultaneous users (allowed by the task).
- AC-05 was verified by a test and source evidence; it cannot be triggered from the UI.
- Daily capacity is enforced by a recheck inside a transaction, not by a database constraint.