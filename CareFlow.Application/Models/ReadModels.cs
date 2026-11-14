namespace CareFlow.Application.Models;

public sealed record PatientOption(int Id, string FullName);

public sealed record ServiceAvailability(
    int ServiceId, string Name, int DailyCapacity, int Remaining);

public sealed record BookingSummary(
    int BookingId, string PatientName, DateOnly BookingDate, int ServiceCount);

public sealed record BookingDetail(
    int BookingId, string PatientName, DateOnly BookingDate,
    DateTimeOffset CreatedAt, IReadOnlyList<string> ServiceNames);