namespace ParkingManager.Api.Domain;

public sealed class ParkingSession
{
    public Guid Id { get; set; }

    public string VehiclePlate { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public bool IsContractUser { get; set; }

    public string SpaceId { get; set; } = string.Empty;

    public int Floor { get; set; }

    public SpaceType SpaceType { get; set; }

    public DateTime EntryTimeUtc { get; set; }

    public DateTime? ExitTimeUtc { get; set; }

    public SessionStatus Status { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime? PaidUntilUtc { get; set; }

    public decimal AmountPaid { get; set; }
}
