using ParkingManager.Api.Domain;

namespace ParkingManager.Api.Services;

public static class ParkingSeeder
{
    public static void EnsureSeeded(ParkingDbContext db, ParkingManagerOptions options)
    {
        if (db.ParkingSpaces.Any())
        {
            return;
        }

        if (options.UncoveredSpaceRatio > 0.15m)
        {
            throw new InvalidOperationException("Uncovered spaces cannot exceed 15% of total capacity.");
        }

        var spaces = new List<ParkingSpace>();

        for (var floor = 1; floor <= options.Floors; floor++)
        {
            var uncoveredCount = (int)Math.Floor(options.SpacesPerFloor * options.UncoveredSpaceRatio);
            var coveredCount = options.SpacesPerFloor - uncoveredCount;

            for (var i = 1; i <= coveredCount; i++)
            {
                spaces.Add(new ParkingSpace
                {
                    Id = $"F{floor}-C{i:00}",
                    Floor = floor,
                    Type = SpaceType.Covered,
                    IsOccupied = false
                });
            }

            for (var i = 1; i <= uncoveredCount; i++)
            {
                spaces.Add(new ParkingSpace
                {
                    Id = $"F{floor}-U{i:00}",
                    Floor = floor,
                    Type = SpaceType.Uncovered,
                    IsOccupied = false
                });
            }
        }

        db.ParkingSpaces.AddRange(spaces);
        db.SaveChanges();
    }
}
