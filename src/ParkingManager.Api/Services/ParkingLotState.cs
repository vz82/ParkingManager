using ParkingManager.Api.Domain;

namespace ParkingManager.Api.Services;

public sealed class ParkingLotState
{
    public ParkingLotState(ParkingManagerOptions options)
    {
        if (options.UncoveredSpaceRatio > 0.15m)
        {
            throw new InvalidOperationException("Uncovered spaces cannot exceed 15% of total capacity.");
        }

        for (var floor = 1; floor <= options.Floors; floor++)
        {
            var uncoveredCount = (int)Math.Floor(options.SpacesPerFloor * options.UncoveredSpaceRatio);
            var coveredCount = options.SpacesPerFloor - uncoveredCount;

            for (var i = 1; i <= coveredCount; i++)
            {
                Spaces.Add(new ParkingSpace
                {
                    Id = $"F{floor}-C{i:00}",
                    Floor = floor,
                    Type = SpaceType.Covered,
                    IsOccupied = false
                });
            }

            for (var i = 1; i <= uncoveredCount; i++)
            {
                Spaces.Add(new ParkingSpace
                {
                    Id = $"F{floor}-U{i:00}",
                    Floor = floor,
                    Type = SpaceType.Uncovered,
                    IsOccupied = false
                });
            }
        }
    }

    public List<ParkingSpace> Spaces { get; } = new();

    public List<ParkingSession> Sessions { get; } = new();

    public List<PaymentRecord> Payments { get; } = new();

    public List<WeatherInterval> WeatherIntervals { get; } = new();

    public object SyncRoot { get; } = new();
}
