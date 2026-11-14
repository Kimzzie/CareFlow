# Verification Checklist

| ID | How checked | Evidence | Done |
|---|---|---|---|
| AC-01 | Test `Availability_IsCalculatedFromPersistedBookings`; changed the date in the UI | Numbers differ per date | [ ] |
| AC-02 | Test `MultiServiceBooking_...`; booked CBC+Urinalysis+ECG in the UI | 1 row in Bookings, 3 in BookingItems | [ ] |
| AC-03 | Tests `FullService_...`, `InvalidRequests_...`; booked ECG 3 times in the UI | "Fully booked... Nothing was booked"; counts unchanged | [ ] |
| AC-04 | Test `StaleScreen_IsRevalidated_NoOverbooking` | ECG count stays at capacity | [ ] |
| AC-05 | Test `FailureBeforeCommit_RollsBackEverything`; source: `await using` in `BookingService` | 0 rows after injected failure | [ ] |
| AC-06 | Manual: booked, closed app, reopened | Bookings still listed | [ ] |

**Not verified / limitations:** (write anything you skipped here)