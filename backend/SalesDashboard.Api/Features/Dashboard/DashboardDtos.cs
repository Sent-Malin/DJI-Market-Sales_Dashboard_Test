using SalesDashboard.Api.Domain;

namespace SalesDashboard.Api.Features.Dashboard;

public enum RatingMetric { GrossProfit, AverageCheck }

public sealed record PeriodDto(
    DateOnly From, DateOnly To, DateOnly PreviousFrom, DateOnly PreviousTo, Granularity Granularity)
{
    public static PeriodDto Create(DateRange r) =>
        new(r.From, r.To, r.PreviousFrom, r.PreviousTo, r.Granularity);
}

public sealed record MetricDto(decimal? Value, decimal? Previous, decimal? Change);

// --- summary
public sealed record RefundsDto(int Count, decimal Amount, int PreviousCount, decimal PreviousAmount);
public sealed record BestManagerDto(int Id, string FullName, string? AvatarUrl, decimal GrossProfit, decimal? Change);
public sealed record SummaryDto(
    PeriodDto Period,
    MetricDto Revenue, MetricDto GrossProfit, MetricDto Margin,
    MetricDto SalesCount, MetricDto AverageCheck,
    RefundsDto Refunds, int CancelledCount,
    BestManagerDto? BestManager);

// --- managers
public sealed record ManagerRatingDto(
    int? Rank, int? RankChange,
    int ManagerId, string FullName, string Team, string Position, string? AvatarUrl, bool IsActive,
    int SalesCount, decimal Revenue, decimal GrossProfit,
    decimal? AverageCheck, decimal? Margin,
    decimal? GrossProfitChange, decimal? AverageCheckChange);
public sealed record ManagerRatingResponse(PeriodDto Period, RatingMetric SortBy, List<ManagerRatingDto> Items);

// --- timeseries
public sealed record TimeseriesPointDto(DateOnly Date, decimal Revenue, decimal GrossProfit, int SalesCount);
public sealed record TimeseriesResponse(PeriodDto Period, List<TimeseriesPointDto> Points);

// --- categories / products
public sealed record CategoryStatsDto(
    int CategoryId, string Name, int Units, decimal Revenue, decimal GrossProfit, decimal? Margin, decimal Share);
public sealed record CategoriesResponse(PeriodDto Period, List<CategoryStatsDto> Items);

public sealed record ProductStatsDto(
    int ProductId, string Name, string CategoryName, int Units, decimal Revenue, decimal GrossProfit, decimal? Margin);
public sealed record TopProductsResponse(PeriodDto Period, List<ProductStatsDto> Items);

// --- recent sales
public sealed record RecentSaleItemDto(string ProductName, int Quantity, decimal UnitPrice);
public sealed record RecentSaleDto(
    int Id, DateTime SoldAt, SaleStatus Status,
    int ManagerId, string ManagerName,
    string CustomerName, string CustomerCompany,
    List<RecentSaleItemDto> Items,
    decimal Amount, decimal GrossProfit);
public sealed record RecentSalesResponse(PeriodDto Period, List<RecentSaleDto> Items);