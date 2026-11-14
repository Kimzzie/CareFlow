using CareFlow.Application.Models;

namespace CareFlow.Application.Abstractions;

public interface IBookingService
{
    Task<IReadOnlyList<PatientOption>> GetPatientsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ServiceAvailability>> GetAvailabilityAsync(DateOnly date, CancellationToken ct = default);
    Task<BookingResult> CreateBookingAsync(BookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BookingSummary>> GetBookingsAsync(CancellationToken ct = default);
    Task<BookingDetail?> GetBookingDetailAsync(int bookingId, CancellationToken ct = default);
}