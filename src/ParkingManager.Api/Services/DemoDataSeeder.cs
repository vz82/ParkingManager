using ParkingManager.Api.Domain;

namespace ParkingManager.Api.Services;

public static class DemoDataSeeder
{
    public static void EnsureSeeded(ParkingDbContext db, ParkingManagerOptions options)
    {
        if (db.ParkingSessions.Any() || db.PaymentRecords.Any() || db.WeatherIntervals.Any())
        {
            return;
        }

        var spaces = db.ParkingSpaces
            .OrderBy(s => s.Floor)
            .ThenBy(s => s.Id)
            .ToList();

        if (spaces.Count < 4)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var uncoveredSpaces = spaces.Where(s => s.Type == SpaceType.Uncovered).ToList();
        var coveredSpaces = spaces.Where(s => s.Type == SpaceType.Covered).ToList();

        var rainySpace = uncoveredSpaces[0];
        var drySpace = uncoveredSpaces.Count > 1 ? uncoveredSpaces[1] : coveredSpaces[0];
        var coveredSpace = coveredSpaces[0];
        var activeSpace = coveredSpaces.Count > 1 ? coveredSpaces[1] : spaces[1];

        var rainyEntry = monthStart.AddDays(2).AddHours(7);
        var rainyExit = rainyEntry.AddHours(3);
        var rainyPaidAt = rainyExit;
        var rainyBaseAmount = CalculateBaseCharge(rainySpace.Type, rainyEntry, rainyPaidAt, options);
        var rainyDiscount = decimal.Round(rainyBaseAmount * options.UncoveredRainyDiscountRatio, 2, MidpointRounding.AwayFromZero);

        var coveredEntry = monthStart.AddDays(6).AddHours(9);
        var coveredExit = coveredEntry.AddHours(2).AddMinutes(20);
        var coveredPaidAt = coveredExit;
        var coveredBaseAmount = CalculateBaseCharge(coveredSpace.Type, coveredEntry, coveredPaidAt, options);

        var dryEntry = monthStart.AddDays(10).AddHours(12);
        var dryExit = dryEntry.AddHours(1).AddMinutes(10);
        var dryPaidAt = dryExit;
        var dryBaseAmount = CalculateBaseCharge(drySpace.Type, dryEntry, dryPaidAt, options);

        var activeEntry = now.AddMinutes(-25);
        var activePaidAt = now.AddMinutes(-5);
        var activeBaseAmount = CalculateBaseCharge(activeSpace.Type, activeEntry, activePaidAt, options);

        var rainySession = CreateClosedSession(
            rainySpace,
            "DEMO-RAIN-1001",
            "demo-rain-user",
            false,
            rainyEntry,
            rainyExit,
            rainyPaidAt,
            rainyBaseAmount - rainyDiscount,
            options.ExitGracePeriodMinutes);

        var coveredSession = CreateClosedSession(
            coveredSpace,
            "DEMO-COVER-2001",
            "demo-covered-user",
            false,
            coveredEntry,
            coveredExit,
            coveredPaidAt,
            coveredBaseAmount,
            options.ExitGracePeriodMinutes);

        var drySession = CreateClosedSession(
            drySpace,
            "DEMO-DRY-3001",
            "demo-dry-user",
            false,
            dryEntry,
            dryExit,
            dryPaidAt,
            dryBaseAmount,
            options.ExitGracePeriodMinutes);

        var activeSession = new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehiclePlate = "DEMO-ACTIVE-4001",
            UserId = "demo-active-user",
            IsContractUser = false,
            SpaceId = activeSpace.Id,
            Floor = activeSpace.Floor,
            SpaceType = activeSpace.Type,
            EntryTimeUtc = activeEntry,
            ExitTimeUtc = null,
            Status = SessionStatus.PaidWaitingExit,
            PaidAtUtc = activePaidAt,
            PaidUntilUtc = activePaidAt.AddMinutes(options.ExitGracePeriodMinutes),
            AmountPaid = activeBaseAmount
        };

        var rainyInterval = new WeatherInterval
        {
            Id = Guid.NewGuid(),
            StartUtc = rainyEntry.AddHours(1),
            EndUtc = rainyEntry.AddHours(2).AddMinutes(30),
            IsRainy = true
        };

        db.ParkingSessions.AddRange(rainySession, coveredSession, drySession, activeSession);
        db.PaymentRecords.AddRange(
            CreatePaymentRecord(rainySession.Id, rainyBaseAmount, rainyDiscount, rainyBaseAmount - rainyDiscount, "FLOOR_MACHINE", rainyPaidAt),
            CreatePaymentRecord(coveredSession.Id, coveredBaseAmount, 0m, coveredBaseAmount, "FLOOR_MACHINE", coveredPaidAt),
            CreatePaymentRecord(drySession.Id, dryBaseAmount, 0m, dryBaseAmount, "MOBILE_APP", dryPaidAt),
            CreatePaymentRecord(activeSession.Id, activeBaseAmount, 0m, activeBaseAmount, "MOBILE_APP", activePaidAt));
        db.WeatherIntervals.Add(rainyInterval);

        db.SaveChanges();
    }

    private static ParkingSession CreateClosedSession(
        ParkingSpace space,
        string plate,
        string? userId,
        bool isContractUser,
        DateTime entryTimeUtc,
        DateTime exitTimeUtc,
        DateTime paidAtUtc,
        decimal amountPaid,
        int gracePeriodMinutes)
    {
        return new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehiclePlate = plate,
            UserId = userId,
            IsContractUser = isContractUser,
            SpaceId = space.Id,
            Floor = space.Floor,
            SpaceType = space.Type,
            EntryTimeUtc = entryTimeUtc,
            ExitTimeUtc = exitTimeUtc,
            Status = SessionStatus.Closed,
            PaidAtUtc = paidAtUtc,
            PaidUntilUtc = paidAtUtc.AddMinutes(gracePeriodMinutes),
            AmountPaid = amountPaid
        };
    }

    private static PaymentRecord CreatePaymentRecord(
        Guid sessionId,
        decimal baseAmount,
        decimal discountAmount,
        decimal chargedAmount,
        string paymentChannel,
        DateTime paidAtUtc)
    {
        return new PaymentRecord
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            BaseAmount = baseAmount,
            DiscountAmount = discountAmount,
            ChargedAmount = chargedAmount,
            PaymentChannel = paymentChannel,
            PaidAtUtc = paidAtUtc
        };
    }

    private static decimal CalculateBaseCharge(SpaceType spaceType, DateTime entryUtc, DateTime atUtc, ParkingManagerOptions options)
    {
        var minutes = Math.Max((atUtc - entryUtc).TotalMinutes, 0d);
        var billableHours = (decimal)Math.Ceiling(minutes / 60d);

        if (billableHours <= 0m)
        {
            billableHours = 1m;
        }

        var rate = spaceType == SpaceType.Covered
            ? options.CoveredHourlyRate
            : options.UncoveredHourlyRate;

        return decimal.Round(billableHours * rate, 2, MidpointRounding.AwayFromZero);
    }
}