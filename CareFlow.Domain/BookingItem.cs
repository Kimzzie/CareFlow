namespace CareFlow.Domain;

/// <summary>Junction row: one service inside one booking. One row = one unit of capacity (BR-04).</summary>
public class BookingItem
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int DiagnosticServiceId { get; set; }
    public DiagnosticService DiagnosticService { get; set; } = null!;
}