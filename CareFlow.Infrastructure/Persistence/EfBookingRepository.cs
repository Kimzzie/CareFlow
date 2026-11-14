using CareFlow.Application.Abstractions;
using CareFlow.Application.Models;
using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Persistence;

public sealed class EfBookingRepository : IBookingRepository
{
    private readonly CareFlowDbContext _db;

    public EfBookingRepository(CareFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<PatientOption>> GetPatientsAsync(CancellationToken ct = default)
        => await _db.Patients.AsNoTracking()
            .OrderBy(p => p.FullName)
            .Select(p => new PatientOption(p.Id, p.FullName))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ServiceAvailability>> GetAvailabilityAsync(
        DateOnly date, CancellationToken ct = default)
    {
        // Remaining = DailyCapacity - confirmed items on that date (never a stored counter)
        var used = await _db.BookingItems.AsNoTracking()
            .Where(i => i.Booking.BookingDate == date)
            .GroupBy(i => i.DiagnosticServiceId)
            .Select(g => new { ServiceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ServiceId, x => x.Count, ct);

        var services = await _db.DiagnosticServices.AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services
            .Select(s => new ServiceAvailability(
                s.Id, s.Name, s.DailyCapacity,
                Math.Max(0, s.DailyCapacity - used.GetValueOrDefault(s.Id))))
            .ToList();
    }

    public async Task<IReadOnlyList<BookingSummary>> GetBookingsAsync(CancellationToken ct = default)
        => await _db.Bookings.AsNoTracking()
            .OrderByDescending(b => b.Id)   // not CreatedAt: SQLite can't order DateTimeOffset
            .Select(b => new BookingSummary(
                b.Id, b.Patient.FullName, b.BookingDate, b.Items.Count))
            .ToListAsync(ct);

    public async Task<BookingDetail?> GetBookingDetailAsync(int bookingId, CancellationToken ct = default)
    {
        var row = await _db.Bookings.AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.Id,
                PatientName = b.Patient.FullName,
                b.BookingDate,
                b.CreatedAt,
                Services = b.Items.Select(i => i.DiagnosticService.Name).OrderBy(n => n).ToList()
            })
            .FirstOrDefaultAsync(ct);

        return row is null
            ? null
            : new BookingDetail(row.Id, row.PatientName, row.BookingDate, row.CreatedAt, row.Services);
    }

    public Task<bool> PatientExistsAsync(int patientId, CancellationToken ct = default)
        => _db.Patients.AnyAsync(p => p.Id == patientId, ct);

    public async Task<IBookingTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => new EfBookingTransaction(_db, await _db.Database.BeginTransactionAsync(ct));

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
        => await _db.Bookings.AddAsync(booking, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}