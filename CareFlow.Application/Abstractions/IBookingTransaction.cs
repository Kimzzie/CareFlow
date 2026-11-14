namespace CareFlow.Application.Abstractions;

/// <summary>Disposing without calling CommitAsync rolls everything back.</summary>
public interface IBookingTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}