using CareFlow.Application.Abstractions;
using CareFlow.Application.Services;
using CareFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Tests;

public sealed class FixedClock : IClock
{
    public DateOnly Today => new(2030, 1, 1);
    public DateTimeOffset Now => new(2030, 1, 1, 8, 0, 0, TimeSpan.Zero);
}

/// <summary>An in-memory SQLite database built from the real migrations (includes seed data).</summary>
public sealed class TestDb : IDisposable
{
    // The in-memory database lives only while this connection stays open
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDb()
    {
        _connection.Open();
        using var context = NewContext();
        context.Database.Migrate();
    }

    public CareFlowDbContext NewContext() =>
        new(new DbContextOptionsBuilder<CareFlowDbContext>().UseSqlite(_connection).Options);

    public BookingService NewService(
        Func<IBookingRepository, IBookingRepository>? decorate = null)
    {
        IBookingRepository repository = new EfBookingRepository(NewContext());
        if (decorate is not null) repository = decorate(repository);
        return new BookingService(repository, new FixedClock());
    }

    public void Dispose() => _connection.Dispose();
}