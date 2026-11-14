using CareFlow.Application.Models;
using CareFlow.Domain;

namespace CareFlow.Application.Abstractions;

public interface IBookingRepository
{
    // Reads
    Task<IReadOnlyList<PatientOption>> GetPatientsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ServiceAvailability>> GetAvailabilityAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<BookingSummary>> GetBookingsAsync(CancellationToken ct = default);
    Task<BookingDetail?> GetBookingDetailAsync(int bookingId, CancellationToken ct = default);

    // Checks used while confirming
    Task<bool> PatientExistsAsync(int patientId, CancellationToken ct = default);

    // Writes
    Task<IBookingTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}