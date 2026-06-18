namespace ParkingManager.Api.Domain;

public sealed class WeatherInterval
{
    public required Guid Id { get; init; }

    public required DateTime StartUtc { get; init; }

    public required DateTime EndUtc { get; init; }

    public bool IsRainy { get; init; }
}
