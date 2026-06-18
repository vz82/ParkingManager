namespace ParkingManager.Api.Domain;

public sealed class PaymentRecord
{
    public required Guid Id { get; init; }

    public required Guid SessionId { get; init; }

    public required decimal BaseAmount { get; init; }

    public required decimal DiscountAmount { get; init; }

    public required decimal ChargedAmount { get; init; }

    public required string PaymentChannel { get; init; }

    public required DateTime PaidAtUtc { get; init; }
}
