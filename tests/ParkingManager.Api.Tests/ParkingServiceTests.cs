using Microsoft.EntityFrameworkCore;
using ParkingManager.Api.Contracts;
using ParkingManager.Api.Reports;
using ParkingManager.Api.Services;

namespace ParkingManager.Api.Tests;

public sealed class ParkingServiceTests
{
    [Fact]
    public void UncoveredSession_AppliesRainyDiscount_WhenRatioAtLeastThreshold()
    {
        var service = CreateService();

        var entryTime = DateTime.UtcNow.AddHours(-3);
        var entry = service.RegisterEntry(new EntryRequest
        {
            VehiclePlate = "TEST-RAIN-1",
            PreferCoveredSpace = false,
            EntryTimeUtc = entryTime
        });

        var sessionId = ExtractGuid(entry, "SessionId");

        service.AddWeatherInterval(new WeatherIntervalRequest
        {
            StartUtc = entryTime,
            EndUtc = entryTime.AddMinutes(90),
            IsRainy = true
        });

        var payment = service.RegisterPayment(sessionId, new PaymentRequest
        {
            PaymentChannel = "MACHINE",
            PaidAtUtc = entryTime.AddHours(3)
        });

        var charged = ExtractDecimal(payment, "ChargedAmount");
        var discount = ExtractDecimal(payment, "DiscountAmount");

        Assert.True(discount > 0m);
        Assert.Equal(6m, charged);
    }

    [Fact]
    public void ExitAfterGracePeriod_RequiresAdditionalPayment()
    {
        var service = CreateService();

        var entryTime = DateTime.UtcNow.AddHours(-2);
        var entry = service.RegisterEntry(new EntryRequest
        {
            VehiclePlate = "TEST-GRACE-1",
            PreferCoveredSpace = true,
            EntryTimeUtc = entryTime
        });

        var sessionId = ExtractGuid(entry, "SessionId");
        var paidAt = entryTime.AddHours(1);

        service.RegisterPayment(sessionId, new PaymentRequest
        {
            PaymentChannel = "MACHINE",
            PaidAtUtc = paidAt
        });

        var exitAttempt = service.ValidateExit(sessionId, paidAt.AddMinutes(15));

        Assert.False(exitAttempt.CanExit);
        Assert.True(exitAttempt.AdditionalAmountRequired > 0m);
    }

    [Fact]
    public void MonthlyReport_IncludesRevenueAndDiscountCounts()
    {
        var service = CreateService();

        var now = DateTime.UtcNow;
        var entry = service.RegisterEntry(new EntryRequest
        {
            VehiclePlate = "TEST-REPORT-1",
            PreferCoveredSpace = false,
            EntryTimeUtc = now.AddHours(-2)
        });

        var sessionId = ExtractGuid(entry, "SessionId");

        service.AddWeatherInterval(new WeatherIntervalRequest
        {
            StartUtc = now.AddHours(-2),
            EndUtc = now.AddHours(-1),
            IsRainy = true
        });

        service.RegisterPayment(sessionId, new PaymentRequest
        {
            PaymentChannel = "MACHINE",
            PaidAtUtc = now
        });

        MonthlyReport report = service.BuildMonthlyReport(now.Year, now.Month);

        Assert.True(report.TotalRevenue > 0m);
        Assert.True(report.TotalPayments >= 1);
        Assert.True(report.DiscountedPayments >= 1);
    }

    private static ParkingService CreateService()
    {
        var dbOptions = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseInMemoryDatabase($"parking-tests-{Guid.NewGuid()}")
            .Options;

        var options = new ParkingManagerOptions();
        var db = new ParkingDbContext(dbOptions);
        ParkingSeeder.EnsureSeeded(db, options);

        return new ParkingService(db, options);
    }

    private static Guid ExtractGuid(object obj, string propertyName)
    {
        var value = obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        return value is Guid guid
            ? guid
            : throw new InvalidOperationException($"Property '{propertyName}' was not found as Guid.");
    }

    private static decimal ExtractDecimal(object obj, string propertyName)
    {
        var value = obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        return value is decimal dec
            ? dec
            : throw new InvalidOperationException($"Property '{propertyName}' was not found as decimal.");
    }
}
