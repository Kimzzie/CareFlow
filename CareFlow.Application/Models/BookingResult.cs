namespace CareFlow.Application.Models;

public sealed record BookingResult(bool Success, int? BookingId, string Message)
{
    public static BookingResult Ok(int bookingId) =>
        new(true, bookingId, "Booking confirmed.");

    public static BookingResult Fail(string message) =>
        new(false, null, message);
}