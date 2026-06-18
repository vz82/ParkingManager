namespace ParkingManager.Api.Services;

public sealed class ParkingManagerOptions
{
    public int Floors { get; init; } = 3;

    public int SpacesPerFloor { get; init; } = 20;

    public decimal UncoveredSpaceRatio { get; init; } = 0.15m;

    public decimal CoveredHourlyRate { get; init; } = 5.00m;

    public decimal UncoveredHourlyRate { get; init; } = 4.00m;

    public decimal UncoveredRainyDiscountRatio { get; init; } = 0.50m;

    public decimal RainyThresholdRatio { get; init; } = 0.33m;

    public int ExitGracePeriodMinutes { get; init; } = 10;
}
