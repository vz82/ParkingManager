namespace ParkingManager.Api.Domain;

public sealed class ParkingSpace
{
    public string Id { get; set; } = string.Empty;

    public int Floor { get; set; }

    public SpaceType Type { get; set; }

    public bool IsOccupied { get; set; }
}
