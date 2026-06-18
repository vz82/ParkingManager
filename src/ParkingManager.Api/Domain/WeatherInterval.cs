namespace ParkingManager.Api.Domain;

public sealed class WeatherInterval
{
    public Guid Id { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public bool IsRainy { get; set; }
}
