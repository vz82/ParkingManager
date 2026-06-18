namespace ParkingManager.Api.Domain;

public sealed class ParkingSpace
{
    public required string Id { get; init; }

    public required int Floor { get; init; }

    public required SpaceType Type { get; init; }

    public bool IsOccupied { get; set; }
}
