namespace ParkingManager.Api.Contracts;

public sealed class PaymentRequest
{
    public required string PaymentChannel { get; init; }

    public DateTime? PaidAtUtc { get; init; }
}
