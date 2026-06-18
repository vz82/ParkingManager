namespace ParkingManager.Api.Contracts;

public sealed class WeatherIntervalRequest
{
    public required DateTime StartUtc { get; init; }

    public required DateTime EndUtc { get; init; }

    public bool IsRainy { get; init; } = true;
}
