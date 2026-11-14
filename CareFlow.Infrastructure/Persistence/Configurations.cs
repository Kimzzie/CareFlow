using CareFlow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Persistence;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.Property(p => p.FullName).IsRequired().HasMaxLength(120);

        b.HasData(
            new Patient { Id = 1, FullName = "Maria Santos", DateOfBirth = new DateOnly(1990, 3, 14) },
            new Patient { Id = 2, FullName = "Juan Dela Cruz", DateOfBirth = new DateOnly(1985, 7, 2) },
            new Patient { Id = 3, FullName = "Ana Reyes", DateOfBirth = new DateOnly(2001, 11, 23) });
    }
}

public class DiagnosticServiceConfiguration : IEntityTypeConfiguration<DiagnosticService>
{
    public void Configure(EntityTypeBuilder<DiagnosticService> b)
    {
        b.Property(s => s.Name).IsRequired().HasMaxLength(80);
        b.HasIndex(s => s.Name).IsUnique();

        // Capacity must be positive (see note below if you get an obsolete warning)
        b.ToTable(t => t.HasCheckConstraint("CK_DiagnosticService_DailyCapacity", "DailyCapacity > 0"));

        // Small capacities on purpose, so "fully booked" is easy to demo
        b.HasData(
            new DiagnosticService { Id = 1, Name = "Complete Blood Count", DailyCapacity = 5 },
            new DiagnosticService { Id = 2, Name = "Urinalysis", DailyCapacity = 5 },
            new DiagnosticService { Id = 3, Name = "Chest X-Ray", DailyCapacity = 3 },
            new DiagnosticService { Id = 4, Name = "ECG", DailyCapacity = 2 });
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.HasOne(x => x.Patient)
         .WithMany()
         .HasForeignKey(x => x.PatientId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.BookingDate);   // the capacity query filters by date
    }
}

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> b)
    {
        b.HasKey(i => new { i.BookingId, i.DiagnosticServiceId });

        b.HasOne(i => i.Booking)
         .WithMany(x => x.Items)
         .HasForeignKey(i => i.BookingId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.DiagnosticService)
         .WithMany()
         .HasForeignKey(i => i.DiagnosticServiceId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}