using Microsoft.EntityFrameworkCore;
using ParkingManager.Api.Contracts;
using ParkingManager.Api.Domain;
using ParkingManager.Api.Reports;

namespace ParkingManager.Api.Services;

public sealed class ParkingService
{
    private static readonly object SyncRoot = new();

    private readonly ParkingDbContext _db;
    private readonly ParkingManagerOptions _options;

    public ParkingService(ParkingDbContext db, ParkingManagerOptions options)
    {
        _db = db;
        _options = options;
    }

    public object GetInventory()
    {
        lock (SyncRoot)
        {
            var spaces = _db.ParkingSpaces.AsNoTracking().ToList();

            var total = spaces.Count;
            var free = spaces.Count(s => !s.IsOccupied);

            var byFloor = spaces
                .GroupBy(s => s.Floor)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Floor = g.Key,
                    Total = g.Count(),
                    Free = g.Count(s => !s.IsOccupied),
                    CoveredFree = g.Count(s => !s.IsOccupied && s.Type == SpaceType.Covered),
                    UncoveredFree = g.Count(s => !s.IsOccupied && s.Type == SpaceType.Uncovered)
                })
                .ToArray();

            return new
            {
                TotalSpaces = total,
                FreeSpaces = free,
                CoveredFree = spaces.Count(s => !s.IsOccupied && s.Type == SpaceType.Covered),
                UncoveredFree = spaces.Count(s => !s.IsOccupied && s.Type == SpaceType.Uncovered),
                ByFloor = byFloor
            };
        }
    }

    public object RegisterEntry(EntryRequest request)
    {
        var now = request.EntryTimeUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        lock (SyncRoot)
        {
            if (_db.ParkingSessions.Any(s => s.VehiclePlate == request.VehiclePlate && s.Status != SessionStatus.Closed))
            {
                throw new InvalidOperationException("Vehicle already has an active session.");
            }

            var preferredType = request.PreferCoveredSpace ? SpaceType.Covered : SpaceType.Uncovered;
            var chosen = _db.ParkingSpaces
                .Where(s => s.IsOccupied == false)
                .OrderBy(s => s.Floor)
                .ThenBy(s => s.Id)
                .FirstOrDefault(s => s.Type == preferredType)
                ?? _db.ParkingSpaces
                    .Where(s => s.IsOccupied == false)
                    .OrderBy(s => s.Floor)
                    .ThenBy(s => s.Id)
                    .FirstOrDefault();

            if (chosen is null)
            {
                throw new InvalidOperationException("No free spaces available.");
            }

            chosen.IsOccupied = true;

            var session = new ParkingSession
            {
                Id = Guid.NewGuid(),
                VehiclePlate = request.VehiclePlate,
                UserId = request.UserId,
                IsContractUser = request.IsContractUser,
                SpaceId = chosen.Id,
                Floor = chosen.Floor,
                SpaceType = chosen.Type,
                EntryTimeUtc = now,
                Status = SessionStatus.Active
            };

            _db.ParkingSessions.Add(session);
            _db.SaveChanges();

            return new
            {
                SessionId = session.Id,
                session.VehiclePlate,
                session.UserId,
                session.IsContractUser,
                session.SpaceId,
                session.Floor,
                SpaceType = session.SpaceType.ToString(),
                session.EntryTimeUtc,
                Status = session.Status.ToString()
            };
        }
    }

    public object RegisterPayment(Guid sessionId, PaymentRequest request)
    {
        var paidAt = request.PaidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        lock (SyncRoot)
        {
            var session = FindSession(sessionId);
            if (session.Status == SessionStatus.Closed)
            {
                throw new InvalidOperationException("Cannot pay a closed session.");
            }

            var totalDueNow = CalculateTotalDue(session, paidAt);
            var additionalDue = Math.Max(totalDueNow - session.AmountPaid, 0m);

            var rainyRatio = CalculateRainyRatio(session, paidAt);
            var baseAmountForCharge = CalculateBaseCharge(session.SpaceType, session.EntryTimeUtc, paidAt);
            var discountForCharge = CalculateRainyDiscount(session.SpaceType, rainyRatio, baseAmountForCharge);

            if (additionalDue <= 0m)
            {
                session.Status = SessionStatus.PaidWaitingExit;
                session.PaidAtUtc = paidAt;
                session.PaidUntilUtc = paidAt.AddMinutes(_options.ExitGracePeriodMinutes);

                return new
                {
                    SessionId = session.Id,
                    ChargedAmount = 0m,
                    TotalPaid = session.AmountPaid,
                    RainyRatio = rainyRatio,
                    PaidUntilUtc = session.PaidUntilUtc,
                    Message = "No additional payment required. Exit grace period refreshed."
                };
            }

            session.AmountPaid += additionalDue;
            session.PaidAtUtc = paidAt;
            session.PaidUntilUtc = paidAt.AddMinutes(_options.ExitGracePeriodMinutes);
            session.Status = SessionStatus.PaidWaitingExit;

            _db.PaymentRecords.Add(new PaymentRecord
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                BaseAmount = baseAmountForCharge,
                DiscountAmount = discountForCharge,
                ChargedAmount = additionalDue,
                PaymentChannel = request.PaymentChannel,
                PaidAtUtc = paidAt
            });
            _db.SaveChanges();

            return new
            {
                SessionId = session.Id,
                ChargedAmount = additionalDue,
                TotalPaid = session.AmountPaid,
                BaseAmount = baseAmountForCharge,
                DiscountAmount = discountForCharge,
                RainyRatio = rainyRatio,
                PaidUntilUtc = session.PaidUntilUtc,
                Status = session.Status.ToString()
            };
        }
    }

    public ExitValidationResponse ValidateExit(Guid sessionId, DateTime? atUtc = null)
    {
        var now = atUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        lock (SyncRoot)
        {
            var session = FindSession(sessionId);
            if (session.Status == SessionStatus.Closed)
            {
                return new ExitValidationResponse
                {
                    CanExit = false,
                    AdditionalAmountRequired = 0m,
                    Message = "Session already closed."
                };
            }

            if (session.Status == SessionStatus.PaidWaitingExit && session.PaidUntilUtc is not null && now <= session.PaidUntilUtc.Value)
            {
                CloseSession(session, now);
                _db.SaveChanges();
                return new ExitValidationResponse
                {
                    CanExit = true,
                    AdditionalAmountRequired = 0m,
                    Message = "Exit validated. Gate opened."
                };
            }

            var totalDueNow = CalculateTotalDue(session, now);
            var additionalRequired = Math.Max(totalDueNow - session.AmountPaid, 0m);

            if (additionalRequired <= 0m)
            {
                CloseSession(session, now);
                _db.SaveChanges();
                return new ExitValidationResponse
                {
                    CanExit = true,
                    AdditionalAmountRequired = 0m,
                    Message = "Exit validated. Gate opened."
                };
            }

            return new ExitValidationResponse
            {
                CanExit = false,
                AdditionalAmountRequired = additionalRequired,
                Message = "Additional payment required before exit."
            };
        }
    }

    public object AddWeatherInterval(WeatherIntervalRequest request)
    {
        if (request.EndUtc <= request.StartUtc)
        {
            throw new InvalidOperationException("Weather interval end must be after start.");
        }

        lock (SyncRoot)
        {
            var interval = new WeatherInterval
            {
                Id = Guid.NewGuid(),
                StartUtc = request.StartUtc.ToUniversalTime(),
                EndUtc = request.EndUtc.ToUniversalTime(),
                IsRainy = request.IsRainy
            };

            _db.WeatherIntervals.Add(interval);
            _db.SaveChanges();

            return new
            {
                interval.Id,
                interval.StartUtc,
                interval.EndUtc,
                interval.IsRainy
            };
        }
    }

    public MonthlyReport BuildMonthlyReport(int year, int month)
    {
        lock (SyncRoot)
        {
            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var monthPayments = _db.PaymentRecords.AsNoTracking()
                .Where(p => p.PaidAtUtc >= monthStart && p.PaidAtUtc < monthEnd)
                .ToArray();

            var totalRevenue = monthPayments.Sum(p => p.ChargedAmount);
            var discountedPayments = monthPayments.Count(p => p.DiscountAmount > 0m);
            var promotionDiscountGiven = monthPayments.Sum(p => p.DiscountAmount);

            var spacesCount = _db.ParkingSpaces.Count();
            var sessions = _db.ParkingSessions.AsNoTracking().ToList();
            var capacityMinutes = spacesCount * (monthEnd - monthStart).TotalMinutes;
            var occupiedMinutes = sessions.Sum(s => SessionOverlapMinutes(s, monthStart, monthEnd));
            var avgOccupancy = capacityMinutes <= 0
                ? 0m
                : decimal.Round((decimal)(occupiedMinutes / capacityMinutes), 4);

            return new MonthlyReport
            {
                Year = year,
                Month = month,
                TotalRevenue = totalRevenue,
                TotalPayments = monthPayments.Length,
                DiscountedPayments = discountedPayments,
                PromotionDiscountGiven = promotionDiscountGiven,
                AverageOccupancyRatio = avgOccupancy
            };
        }
    }

    public object GetSession(Guid sessionId)
    {
        lock (SyncRoot)
        {
            var s = FindSession(sessionId);
            return new
            {
                SessionId = s.Id,
                s.VehiclePlate,
                s.UserId,
                s.IsContractUser,
                s.SpaceId,
                s.Floor,
                SpaceType = s.SpaceType.ToString(),
                s.EntryTimeUtc,
                s.PaidAtUtc,
                s.PaidUntilUtc,
                s.ExitTimeUtc,
                s.AmountPaid,
                Status = s.Status.ToString()
            };
        }
    }

    private ParkingSession FindSession(Guid sessionId)
    {
        return _db.ParkingSessions.FirstOrDefault(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found.");
    }

    private void CloseSession(ParkingSession session, DateTime exitAtUtc)
    {
        session.ExitTimeUtc = exitAtUtc;
        session.Status = SessionStatus.Closed;

        var space = _db.ParkingSpaces.First(s => s.Id == session.SpaceId);
        space.IsOccupied = false;
    }

    private decimal CalculateTotalDue(ParkingSession session, DateTime atUtc)
    {
        var baseAmount = CalculateBaseCharge(session.SpaceType, session.EntryTimeUtc, atUtc);
        var rainyRatio = CalculateRainyRatio(session, atUtc);
        var discountAmount = CalculateRainyDiscount(session.SpaceType, rainyRatio, baseAmount);
        return decimal.Round(baseAmount - discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    private decimal CalculateBaseCharge(SpaceType spaceType, DateTime entryUtc, DateTime atUtc)
    {
        var minutes = Math.Max((atUtc - entryUtc).TotalMinutes, 0d);
        var billableHours = (decimal)Math.Ceiling(minutes / 60d);
        if (billableHours <= 0m)
        {
            billableHours = 1m;
        }

        var rate = spaceType == SpaceType.Covered
            ? _options.CoveredHourlyRate
            : _options.UncoveredHourlyRate;

        return decimal.Round(billableHours * rate, 2, MidpointRounding.AwayFromZero);
    }

    private decimal CalculateRainyDiscount(SpaceType spaceType, decimal rainyRatio, decimal baseAmount)
    {
        if (spaceType != SpaceType.Uncovered)
        {
            return 0m;
        }

        if (rainyRatio < _options.RainyThresholdRatio)
        {
            return 0m;
        }

        return decimal.Round(baseAmount * _options.UncoveredRainyDiscountRatio, 2, MidpointRounding.AwayFromZero);
    }

    private decimal CalculateRainyRatio(ParkingSession session, DateTime atUtc)
    {
        var totalMinutes = Math.Max((atUtc - session.EntryTimeUtc).TotalMinutes, 0d);
        if (totalMinutes <= 0d)
        {
            return 0m;
        }

        var rainyMinutes = _db.WeatherIntervals.AsNoTracking()
            .Where(w => w.IsRainy)
            .Sum(w => OverlapMinutes(session.EntryTimeUtc, atUtc, w.StartUtc, w.EndUtc));

        var ratio = (decimal)(rainyMinutes / totalMinutes);
        return decimal.Round(ratio, 4, MidpointRounding.AwayFromZero);
    }

    private static double OverlapMinutes(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
    {
        var start = startA > startB ? startA : startB;
        var end = endA < endB ? endA : endB;

        if (end <= start)
        {
            return 0d;
        }

        return (end - start).TotalMinutes;
    }

    private static double SessionOverlapMinutes(ParkingSession session, DateTime monthStart, DateTime monthEnd)
    {
        var end = session.ExitTimeUtc ?? DateTime.UtcNow;
        return OverlapMinutes(session.EntryTimeUtc, end, monthStart, monthEnd);
    }
}
