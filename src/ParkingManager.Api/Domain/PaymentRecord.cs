namespace ParkingManager.Api.Domain;

public sealed class PaymentRecord
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public decimal BaseAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ChargedAmount { get; set; }

    public string PaymentChannel { get; set; } = string.Empty;

    public DateTime PaidAtUtc { get; set; }
}
