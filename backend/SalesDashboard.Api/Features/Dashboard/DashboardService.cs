using Dapper;
using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Data;
using SalesDashboard.Api.Domain;

namespace SalesDashboard.Api.Features.Dashboard;

public sealed class DashboardService(SalesDbContext db, PeriodResolver periods)
{
    private sealed record Agg(decimal Revenue, decimal Cost, int Count)
    {
        public static readonly Agg Empty = new(0, 0, 0);
        public decimal GrossProfit => Revenue - Cost;
        public decimal? AverageCheck => Metrics.AverageCheck(Revenue, Count);
        public decimal? Margin => Metrics.Margin(Revenue, GrossProfit);
    }

    // ---------------------------------------------------------------- summary
    public async Task<SummaryDto> GetSummaryAsync(DateRange r, CancellationToken ct)
    {
        // Один запрос на оба периода: GROUP BY (период, статус) → максимум 6 строк
        var rows = await db.Sales
            .Where(s => (s.SoldAt >= r.FromUtc && s.SoldAt < r.ToUtc) ||
                        (s.SoldAt >= r.PreviousFromUtc && s.SoldAt < r.PreviousToUtc))
            .GroupBy(s => new { IsCurrent = s.SoldAt >= r.FromUtc, s.Status })
            .Select(g => new
            {
                g.Key.IsCurrent,
                g.Key.Status,
                Amount = g.Sum(s => s.TotalAmount),
                Cost = g.Sum(s => s.TotalCost),
                Count = g.Count()
            })
            .ToListAsync(ct);

        Agg Pick(bool current, SaleStatus status)
        {
            var x = rows.FirstOrDefault(row => row.IsCurrent == current && row.Status == status);
            return x is null ? Agg.Empty : new Agg(x.Amount, x.Cost, x.Count);
        }

        var c = Pick(true, SaleStatus.Paid);
        var p = Pick(false, SaleStatus.Paid);
        var refunds = Pick(true, SaleStatus.Refunded);
        var prevRefunds = Pick(false, SaleStatus.Refunded);
        var cancelled = Pick(true, SaleStatus.Cancelled);

        var rating = await GetManagerRatingAsync(r, RatingMetric.GrossProfit, ct);
        var best = rating.Items.FirstOrDefault(x => x.Rank == 1);

        return new SummaryDto(
            PeriodDto.Create(r),
            Relative(c.Revenue, p.Revenue),
            Relative(c.GrossProfit, p.GrossProfit),
            new MetricDto(c.Margin, p.Margin, Metrics.AbsoluteChange(c.Margin, p.Margin)),
            Relative(c.Count, p.Count),
            Relative(c.AverageCheck, p.AverageCheck),
            new RefundsDto(refunds.Count, refunds.Revenue, prevRefunds.Count, prevRefunds.Revenue),
            cancelled.Count,
            best is null
                ? null
                : new BestManagerDto(best.ManagerId, best.FullName, best.AvatarUrl,
                                     best.GrossProfit, best.GrossProfitChange));
    }


    private static MetricDto Relative(decimal? current, decimal? previous) =>
        new(current, previous, Metrics.RelativeChange(current, previous));

    // ---------------------------------------------------------------- managers
    public async Task<ManagerRatingResponse> GetManagerRatingAsync(
        DateRange r, RatingMetric sortBy, CancellationToken ct)
    {
        // Агрегаты по менеджеру за оба периода — один запрос, без N+1
        var stats = await db.Sales
            .Where(s => s.Status == SaleStatus.Paid &&
                        ((s.SoldAt >= r.FromUtc && s.SoldAt < r.ToUtc) ||
                         (s.SoldAt >= r.PreviousFromUtc && s.SoldAt < r.PreviousToUtc)))
            .GroupBy(s => new { s.ManagerId, IsCurrent = s.SoldAt >= r.FromUtc })
            .Select(g => new
            {
                g.Key.ManagerId,
                g.Key.IsCurrent,
                Revenue = g.Sum(s => s.TotalAmount),
                Cost = g.Sum(s => s.TotalCost),
                Count = g.Count()
            })
            .ToListAsync(ct);

        var current = stats.Where(x => x.IsCurrent)
            .ToDictionary(x => x.ManagerId, x => new Agg(x.Revenue, x.Cost, x.Count));
        var previous = stats.Where(x => !x.IsCurrent)
            .ToDictionary(x => x.ManagerId, x => new Agg(x.Revenue, x.Cost, x.Count));

        var managers = await db.Managers
            .Select(m => new { m.Id, m.FullName, m.Team, m.Position, m.AvatarUrl, m.IsActive })
            .ToListAsync(ct);

        // Активные — всегда (в т.ч. без продаж); неактивные — только если продавали в периоде
        var visible = managers.Where(m => m.IsActive || current.ContainsKey(m.Id)).ToList();

        Agg Cur(int id) => current.GetValueOrDefault(id, Agg.Empty);
        Agg Prev(int id) => previous.GetValueOrDefault(id, Agg.Empty);

        static decimal? MetricOf(Agg a, RatingMetric m) =>
            a.Count == 0 ? null : m == RatingMetric.GrossProfit ? a.GrossProfit : a.AverageCheck;

        var ranks = RankCalculator.Rank(visible.Select(m => (m.Id, MetricOf(Cur(m.Id), sortBy))));
        var prevRanks = RankCalculator.Rank(visible.Select(m => (m.Id, MetricOf(Prev(m.Id), sortBy))));

        var items = visible
            .Select(m =>
            {
                var c = Cur(m.Id);
                var p = Prev(m.Id);
                int? rank = ranks.TryGetValue(m.Id, out var rk) ? rk : null;
                int? prevRank = prevRanks.TryGetValue(m.Id, out var pr) ? pr : null;

                return new ManagerRatingDto(
                    rank,
                    rank is not null && prevRank is not null ? prevRank - rank : null, // >0 — поднялся
                    m.Id, m.FullName, m.Team, m.Position, m.AvatarUrl, m.IsActive,
                    c.Count, c.Revenue, c.GrossProfit, c.AverageCheck, c.Margin,
                    Metrics.RelativeChange(c.GrossProfit, p.Count == 0 ? null : p.GrossProfit),
                    Metrics.RelativeChange(c.AverageCheck, p.AverageCheck));
            })
            .OrderBy(x => x.Rank ?? int.MaxValue)   // без продаж — в конец
            .ThenByDescending(x => x.Revenue)       // стабильный порядок при равенстве
            .ThenBy(x => x.FullName)
            .ToList();

        return new ManagerRatingResponse(PeriodDto.Create(r), sortBy, items);
    }

    // ---------------------------------------------------------------- timeseries
    private sealed class TimeseriesRow
    {
        public DateTime Bucket { get; init; }
        public decimal Revenue { get; init; }
        public decimal GrossProfit { get; init; }
        public long SalesCount { get; init; }
    }

    public async Task<TimeseriesResponse> GetTimeseriesAsync(DateRange r, CancellationToken ct)
    {
        // Raw SQL: generate_series даёт непрерывную шкалу (дни без продаж = 0),
        // группировка — по локальной дате бизнеса, а не по UTC
        const string sql = """
            WITH buckets AS (
                SELECT generate_series(
                           date_trunc(@Unit, @FromLocal),
                           date_trunc(@Unit, @LastLocal),
                           ('1 ' || @Unit)::interval) AS bucket
            ),
            agg AS (
                SELECT date_trunc(@Unit, s.sold_at AT TIME ZONE @Tz) AS bucket,
                       SUM(s.total_amount)                AS revenue,
                       SUM(s.total_amount - s.total_cost) AS gross_profit,
                       COUNT(*)                           AS sales_count
                FROM sales s
                WHERE s.status = 'Paid'
                  AND s.sold_at >= @FromUtc
                  AND s.sold_at <  @ToUtc
                GROUP BY 1
            )
            SELECT b.bucket                    AS bucket,
                   COALESCE(a.revenue, 0)      AS revenue,
                   COALESCE(a.gross_profit, 0) AS grossprofit,
                   COALESCE(a.sales_count, 0)  AS salescount
            FROM buckets b
            LEFT JOIN agg a ON a.bucket = b.bucket
            ORDER BY b.bucket;
            """;

        var unit = r.Granularity switch
        {
            Granularity.Day => "day",
            Granularity.Week => "week",
            _ => "month"
        };

        var rows = await db.Database.GetDbConnection().QueryAsync<TimeseriesRow>(
            new CommandDefinition(sql, new
            {
                Unit = unit,
                Tz = periods.TimeZone.Id,
                FromLocal = r.From.ToDateTime(TimeOnly.MinValue),  // Kind=Unspecified → timestamp
                LastLocal = r.To.ToDateTime(TimeOnly.MinValue),
                r.FromUtc,                                         // Kind=Utc → timestamptz
                r.ToUtc
            }, cancellationToken: ct));

        var points = rows
            .Select(x => new TimeseriesPointDto(
                DateOnly.FromDateTime(x.Bucket), x.Revenue, x.GrossProfit, (int)x.SalesCount))
            .ToList();

        return new TimeseriesResponse(PeriodDto.Create(r), points);
    }

    // ---------------------------------------------------------------- categories
    public async Task<CategoriesResponse> GetCategoriesAsync(DateRange r, CancellationToken ct)
    {
        var stats = await db.SaleItems
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= r.FromUtc && i.Sale.SoldAt < r.ToUtc)
            .GroupBy(i => i.Product.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Units = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Cost = g.Sum(i => i.Quantity * i.UnitCost)
            })
            .ToDictionaryAsync(x => x.CategoryId, ct);

        var categories = await db.Categories
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        var total = stats.Values.Sum(x => x.Revenue);

        var items = categories
            .Select(c =>
            {
                var s = stats.GetValueOrDefault(c.Id);
                var revenue = s?.Revenue ?? 0;
                var profit = revenue - (s?.Cost ?? 0);
                return new CategoryStatsDto(
                    c.Id, c.Name, s?.Units ?? 0, revenue, profit,
                    Metrics.Margin(revenue, profit),
                    total == 0 ? 0 : Math.Round(revenue / total, 4));
            })
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.Name)
            .ToList();

        return new CategoriesResponse(PeriodDto.Create(r), items);
    }

    // ---------------------------------------------------------------- top products
    public async Task<TopProductsResponse> GetTopProductsAsync(DateRange r, int limit, CancellationToken ct)
    {
        var rows = await db.SaleItems
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= r.FromUtc && i.Sale.SoldAt < r.ToUtc)
            .GroupBy(i => new { i.ProductId, i.Product.Name, CategoryName = i.Product.Category.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                g.Key.CategoryName,
                Units = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Cost = g.Sum(i => i.Quantity * i.UnitCost)
            })
            .OrderByDescending(x => x.Revenue - x.Cost)
            .ThenBy(x => x.Name)
            .Take(limit)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new ProductStatsDto(
                x.ProductId, x.Name, x.CategoryName, x.Units, x.Revenue, x.Revenue - x.Cost,
                Metrics.Margin(x.Revenue, x.Revenue - x.Cost)))
            .ToList();

        return new TopProductsResponse(PeriodDto.Create(r), items);
    }

    // ---------------------------------------------------------------- recent sales
    public async Task<RecentSalesResponse> GetRecentSalesAsync(DateRange r, int limit, CancellationToken ct)
    {
        // Проекция в DTO: EF строит один SQL (LIMIT + LEFT JOIN позиций), N+1 нет
        var items = await db.Sales
            .Where(s => s.SoldAt >= r.FromUtc && s.SoldAt < r.ToUtc)
            .OrderByDescending(s => s.SoldAt)
            .ThenByDescending(s => s.Id)
            .Take(limit)
            .Select(s => new RecentSaleDto(
                s.Id, s.SoldAt, s.Status,
                s.ManagerId, s.Manager.FullName,
                s.Customer.Name, s.Customer.Company,
                s.Items
                    .OrderByDescending(i => i.Quantity * i.UnitPrice)
                    .Select(i => new RecentSaleItemDto(i.Product.Name, i.Quantity, i.UnitPrice))
                    .ToList(),
                s.TotalAmount,
                s.TotalAmount - s.TotalCost))
            .ToListAsync(ct);

        return new RecentSalesResponse(PeriodDto.Create(r), items);
    }
}