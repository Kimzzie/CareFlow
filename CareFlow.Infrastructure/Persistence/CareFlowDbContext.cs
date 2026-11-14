using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Persistence;

public class CareFlowDbContext : DbContext
{
    public CareFlowDbContext(DbContextOptions<CareFlowDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DiagnosticService> DiagnosticServices => Set<DiagnosticService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareFlowDbContext).Assembly);
}