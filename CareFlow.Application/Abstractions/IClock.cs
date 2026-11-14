namespace CareFlow.Application.Abstractions;

public interface IClock
{
    DateOnly Today { get; }          // local date, for BR-03
    DateTimeOffset Now { get; }      // for CreatedAt
}