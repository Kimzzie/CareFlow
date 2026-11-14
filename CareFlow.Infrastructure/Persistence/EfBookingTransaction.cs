using CareFlow.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CareFlow.Infrastructure.Persistence;

internal sealed class EfBookingTransaction : IBookingTransaction
{
    private readonly CareFlowDbContext _db;
    private readonly IDbContextTransaction _tx;

    public EfBookingTransaction(CareFlowDbContext db, IDbContextTransaction tx)
    {
        _db = db;
        _tx = tx;
    }

    public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);

    public async ValueTask DisposeAsync()
    {
        await _tx.DisposeAsync();     // not committed => rolled back
        _db.ChangeTracker.Clear();    // forget any failed/added entities
    }
}