namespace ParkingManager.Api.Domain;

public sealed class ParkingSession
{
    public required Guid Id { get; init; }

    public required string VehiclePlate { get; init; }

    public string? UserId { get; init; }

    public bool IsContractUser { get; init; }

    public required string SpaceId { get; init; }

    public required int Floor { get; init; }

    public required SpaceType SpaceType { get; init; }

    public required DateTime EntryTimeUtc { get; init; }

    public DateTime? ExitTimeUtc { get; set; }

    public SessionStatus Status { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime? PaidUntilUtc { get; set; }

    public decimal AmountPaid { get; set; }
}
