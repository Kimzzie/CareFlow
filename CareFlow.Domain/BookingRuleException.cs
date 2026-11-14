namespace CareFlow.Domain;

/// <summary>Thrown when a booking breaks a business rule. The message is user-friendly (BR-10).</summary>
public sealed class BookingRuleException : Exception
{
    public BookingRuleException(string message) : base(message) { }
}