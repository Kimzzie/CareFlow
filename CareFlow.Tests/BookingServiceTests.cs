using CareFlow.Application.Abstractions;
using CareFlow.Application.Models;
using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Tests;

public class BookingServiceTests : IDisposable
{
    // Seed data from Stage 4: CBC 5/day, Urinalysis 5, Chest X-Ray 3, ECG 2. Patient 1 exists.
    private const int Cbc = 1, Urinalysis = 2, ChestXray = 3, Ecg = 4;
    private static readonly DateOnly Today = new(2030, 1, 1);   // matches FixedClock
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    private static BookingRequest Req(params int[] serviceIds) => new(1, Tomorrow, serviceIds);

    private async Task<int> CountBookings()
    {
        await using var ctx = _db.NewContext();
        return await ctx.Bookings.CountAsync();
    }

    private async Task<int> CountItems(int serviceId)
    {
        await using var ctx = _db.NewContext();
        return await ctx.BookingItems.CountAsync(i => i.DiagnosticServiceId == serviceId);
    }

    [Fact] // AC-01
    public async Task Availability_IsCalculatedFromPersistedBookings()
    {
        var service = _db.NewService();
        await service.CreateBookingAsync(Req(Cbc));

        var tomorrow = await service.GetAvailabilityAsync(Tomorrow);
        var otherDay = await service.GetAvailabilityAsync(Tomorrow.AddDays(1));

        Assert.Equal(4, tomorrow.Single(s => s.ServiceId == Cbc).Remaining);
        Assert.Equal(5, otherDay.Single(s => s.ServiceId == Cbc).Remaining);
    }

    [Fact] // AC-02
    public async Task MultiServiceBooking_RecordsOneBookingWithEveryService()
    {
        var service = _db.NewService();

        var result = await service.CreateBookingAsync(Req(Cbc, Urinalysis, Ecg));

        Assert.True(result.Success);
        Assert.Equal(1, await CountBookings());
        var detail = await service.GetBookingDetailAsync(result.BookingId!.Value);
        Assert.Equal(3, detail!.ServiceNames.Count);
    }

    [Fact] // AC-03 (full service)
    public async Task FullService_RejectsWholeRequest_NothingSaved()
    {
        var service = _db.NewService();
        await service.CreateBookingAsync(Req(Ecg));
        await service.CreateBookingAsync(Req(Ecg));              // ECG now full (2 of 2)

        var result = await service.CreateBookingAsync(Req(Cbc, Urinalysis, Ecg));

        Assert.False(result.Success);
        Assert.Contains("ECG", result.Message);
        Assert.Equal(2, await CountBookings());                  // no third booking
        Assert.Equal(0, await CountItems(Cbc));                  // CBC was NOT booked on its own
        Assert.Equal(0, await CountItems(Urinalysis));
    }

    [Fact] // AC-03 (invalid information)
    public async Task InvalidRequests_AreRejected_NothingSaved()
    {
        var service = _db.NewService();
        var bad = new[]
        {
            new BookingRequest(1,   Today.AddDays(-1), new[] { Cbc }),                        // BR-03
            new BookingRequest(1,   Tomorrow, new[] { Cbc, Cbc }),                            // BR-06
            new BookingRequest(1,   Tomorrow, Array.Empty<int>()),                            // BR-02 (0)
            new BookingRequest(1,   Tomorrow, new[] { Cbc, Urinalysis, ChestXray, Ecg }),     // BR-02 (4)
            new BookingRequest(999, Tomorrow, new[] { Cbc }),                                 // BR-01
        };

        foreach (var request in bad)
            Assert.False((await service.CreateBookingAsync(request)).Success);

        Assert.Equal(0, await CountBookings());
    }

    [Fact] // AC-04
    public async Task StaleScreen_IsRevalidated_NoOverbooking()
    {
        var screen = _db.NewService();
        var otherUser = _db.NewService();
        await otherUser.CreateBookingAsync(Req(Ecg));                    // 1 of 2 used

        var shown = await screen.GetAvailabilityAsync(Tomorrow);
        Assert.Equal(1, shown.Single(s => s.ServiceId == Ecg).Remaining); // screen says "1 left"

        await otherUser.CreateBookingAsync(Req(Ecg));                    // someone takes the last slot

        var result = await screen.CreateBookingAsync(Req(Ecg));          // stale screen confirms anyway

        Assert.False(result.Success);
        Assert.Equal(2, await CountItems(Ecg));                          // never exceeds capacity
    }

    [Fact] // AC-05
    public async Task FailureBeforeCommit_RollsBackEverything()
    {
        var service = _db.NewService(inner => new FailAfterSaveRepository(inner));

        var result = await service.CreateBookingAsync(Req(Cbc, Urinalysis, Ecg));

        Assert.False(result.Success);                                    // not presented as success (BR-10)
        Assert.Contains("Nothing was booked", result.Message);
        Assert.Equal(0, await CountBookings());                          // rows were written, then rolled back
        Assert.Equal(0, await CountItems(Cbc));
        Assert.Equal(0, await CountItems(Ecg));
    }

    /// <summary>Saves the rows, THEN throws, so only the rollback can remove them.</summary>
    private sealed class FailAfterSaveRepository(IBookingRepository inner) : IBookingRepository
    {
        public Task<IReadOnlyList<PatientOption>> GetPatientsAsync(CancellationToken ct = default) => inner.GetPatientsAsync(ct);
        public Task<IReadOnlyList<ServiceAvailability>> GetAvailabilityAsync(DateOnly d, CancellationToken ct = default) => inner.GetAvailabilityAsync(d, ct);
        public Task<IReadOnlyList<BookingSummary>> GetBookingsAsync(CancellationToken ct = default) => inner.GetBookingsAsync(ct);
        public Task<BookingDetail?> GetBookingDetailAsync(int id, CancellationToken ct = default) => inner.GetBookingDetailAsync(id, ct);
        public Task<bool> PatientExistsAsync(int id, CancellationToken ct = default) => inner.PatientExistsAsync(id, ct);
        public Task<IBookingTransaction> BeginTransactionAsync(CancellationToken ct = default) => inner.BeginTransactionAsync(ct);
        public Task AddAsync(Booking b, CancellationToken ct = default) => inner.AddAsync(b, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await inner.SaveChangesAsync(ct);
            throw new InvalidOperationException("Simulated failure after the rows were written.");
        }
    }
}