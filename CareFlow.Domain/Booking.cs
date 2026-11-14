namespace CareFlow.Domain;

public class Booking
{
    public const int MaxServices = 3;

    public int Id { get; private set; }
    public int PatientId { get; private set; }
    public Patient Patient { get; private set; } = null!;
    public DateOnly BookingDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public List<BookingItem> Items { get; private set; } = new();

    private Booking() { }   // required by EF Core

    public static Booking Create(
        int patientId,
        DateOnly bookingDate,
        DateOnly today,
        IReadOnlyCollection<int> serviceIds,
        DateTimeOffset createdAt)
    {
        if (patientId <= 0)
            throw new BookingRuleException("Please select a patient.");

        // BR-02
        if (serviceIds.Count is < 1 or > MaxServices)
            throw new BookingRuleException(
                $"A booking must contain 1 to {MaxServices} services.");

        // BR-06
        if (serviceIds.Distinct().Count() != serviceIds.Count)
            throw new BookingRuleException(
                "The same service cannot be selected more than once.");

        // BR-03
        if (bookingDate < today)
            throw new BookingRuleException(
                "The booking date cannot be earlier than today.");

        var booking = new Booking
        {
            PatientId = patientId,
            BookingDate = bookingDate,
            CreatedAt = createdAt
        };

        foreach (var serviceId in serviceIds)
            booking.Items.Add(new BookingItem { DiagnosticServiceId = serviceId });

        return booking;
    }
}