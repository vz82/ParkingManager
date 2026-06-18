namespace ParkingManager.Api.Reports;

public sealed class MonthlyReport
{
    public required int Year { get; init; }

    public required int Month { get; init; }

    public required decimal GrossRevenue { get; init; }

    public required decimal TotalRevenue { get; init; }

    public required int TotalPayments { get; init; }

    public required int DiscountedPayments { get; init; }

    public required decimal PromotionDiscountGiven { get; init; }

    public required decimal AverageOccupancyRatio { get; init; }
}
