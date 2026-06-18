using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParkingManager.Api.Services;

public sealed class ParkingDbContextFactory : IDesignTimeDbContextFactory<ParkingDbContext>
{
    public ParkingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkingDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=parking_manager;Username=postgres;Password=postgres");

        return new ParkingDbContext(optionsBuilder.Options);
    }
}
