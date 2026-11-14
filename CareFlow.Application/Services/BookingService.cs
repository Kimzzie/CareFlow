using System.Diagnostics;
using CareFlow.Application.Abstractions;
using CareFlow.Application.Models;
using CareFlow.Domain;

namespace CareFlow.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IClock _clock;

    public BookingService(IBookingRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    // ---- Reads: simple pass-through ----
    public Task<IReadOnlyList<PatientOption>> GetPatientsAsync(CancellationToken ct = default)
        => _repository.GetPatientsAsync(ct);

    public Task<IReadOnlyList<ServiceAvailability>> GetAvailabilityAsync(
        DateOnly date, CancellationToken ct = default)
        => _repository.GetAvailabilityAsync(date, ct);

    public Task<IReadOnlyList<BookingSummary>> GetBookingsAsync(CancellationToken ct = default)
        => _repository.GetBookingsAsync(ct);

    public Task<BookingDetail?> GetBookingDetailAsync(int bookingId, CancellationToken ct = default)
        => _repository.GetBookingDetailAsync(bookingId, ct);

    // ---- The coordinated write ----
    public async Task<BookingResult> CreateBookingAsync(
        BookingRequest request, CancellationToken ct = default)
    {
        try
        {
            // 1. Domain rules: BR-02, BR-03, BR-06 (throws BookingRuleException)
            var booking = Booking.Create(
                request.PatientId,
                request.BookingDate,
                _clock.Today,
                request.ServiceIds,
                _clock.Now);

            // 2. Transaction boundary starts here
            await using var transaction = await _repository.BeginTransactionAsync(ct);

            // 3. BR-01
            if (!await _repository.PatientExistsAsync(request.PatientId, ct))
                return BookingResult.Fail("The selected patient does not exist.");

            // 4. BR-04 / BR-05 / BR-08: fresh capacity check, inside the transaction
            var availability = await _repository.GetAvailabilityAsync(request.BookingDate, ct);
            var fullServices = new List<string>();

            foreach (var serviceId in request.ServiceIds)
            {
                var service = availability.FirstOrDefault(a => a.ServiceId == serviceId);

                if (service is null)
                    return BookingResult.Fail("One of the selected services does not exist.");

                if (service.Remaining < 1)
                    fullServices.Add(service.Name);
            }

            if (fullServices.Count > 0)
                return BookingResult.Fail(
                    $"Fully booked on {request.BookingDate:MMM dd, yyyy}: " +
                    $"{string.Join(", ", fullServices)}. Nothing was booked.");

            // 5. Save booking + all items together
            await _repository.AddAsync(booking, ct);
            await _repository.SaveChangesAsync(ct);

            // 6. Commit is the LAST step. Every other exit rolls back on dispose.
            await transaction.CommitAsync(ct);

            return BookingResult.Ok(booking.Id);
        }
        catch (BookingRuleException ex)
        {
            return BookingResult.Fail(ex.Message);          // expected: user-friendly message
        }
        catch (OperationCanceledException)
        {
            throw;                                          // cancellation is not a failure
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Booking failed: {ex}");       // keep the details, don't lose them
            return BookingResult.Fail(
                "The booking could not be saved because of a database problem. " +
                "Nothing was booked. Please try again.");
        }
    }
}