namespace CareFlow.Application.Models;

public sealed record BookingRequest(
    int PatientId,
    DateOnly BookingDate,
    IReadOnlyCollection<int> ServiceIds);