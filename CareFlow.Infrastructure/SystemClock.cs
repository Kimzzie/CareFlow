using CareFlow.Application.Abstractions;

namespace CareFlow.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);   // local date
    public DateTimeOffset Now => DateTimeOffset.Now;
}