using Microsoft.EntityFrameworkCore;
using ParkingManager.Api.Domain;

namespace ParkingManager.Api.Services;

public sealed class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();

    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();

    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

    public DbSet<WeatherInterval> WeatherIntervals => Set<WeatherInterval>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParkingSpace>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Type).HasConversion<string>();
            entity.HasIndex(p => new { p.Floor, p.Type });
        });

        modelBuilder.Entity<ParkingSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SpaceType).HasConversion<string>();
            entity.Property(s => s.Status).HasConversion<string>();
            entity.HasIndex(s => s.VehiclePlate);
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.SpaceId);
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.SessionId);
            entity.HasIndex(p => p.PaidAtUtc);
        });

        modelBuilder.Entity<WeatherInterval>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => new { w.IsRainy, w.StartUtc, w.EndUtc });
        });
    }
}
