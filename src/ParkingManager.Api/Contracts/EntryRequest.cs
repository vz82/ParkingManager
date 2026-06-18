namespace ParkingManager.Api.Contracts;

public sealed class EntryRequest
{
    public required string VehiclePlate { get; init; }

    public string? UserId { get; init; }

    public bool IsContractUser { get; init; }

    public bool PreferCoveredSpace { get; init; }

    public DateTime? EntryTimeUtc { get; init; }
}
