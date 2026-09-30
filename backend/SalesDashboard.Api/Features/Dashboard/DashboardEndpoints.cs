using Microsoft.AspNetCore.Http.HttpResults;

namespace SalesDashboard.Api.Features.Dashboard;

public sealed record PeriodQuery(string? Period, DateOnly? From, DateOnly? To);

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/summary",
            ([AsParameters] PeriodQuery q, PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
                Handle(q, periods, [], r => svc.GetSummaryAsync(r, ct)));

        group.MapGet("/managers",
            ([AsParameters] PeriodQuery q, string? sortBy,
             PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
            {
                var errors = new Dictionary<string, string[]>();
                var metric = ParseMetric(sortBy, errors);
                return Handle(q, periods, errors, r => svc.GetManagerRatingAsync(r, metric, ct));
            });

        group.MapGet("/timeseries",
            ([AsParameters] PeriodQuery q, PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
                Handle(q, periods, [], r => svc.GetTimeseriesAsync(r, ct)));

        group.MapGet("/categories",
            ([AsParameters] PeriodQuery q, PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
                Handle(q, periods, [], r => svc.GetCategoriesAsync(r, ct)));

        group.MapGet("/products/top",
            ([AsParameters] PeriodQuery q, int? limit,
             PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
            {
                var errors = new Dictionary<string, string[]>();
                var take = ParseLimit(limit, 5, errors);
                return Handle(q, periods, errors, r => svc.GetTopProductsAsync(r, take, ct));
            });

        group.MapGet("/sales/recent",
            ([AsParameters] PeriodQuery q, int? limit,
             PeriodResolver periods, DashboardService svc, CancellationToken ct) =>
            {
                var errors = new Dictionary<string, string[]>();
                var take = ParseLimit(limit, 10, errors);
                return Handle(q, periods, errors, r => svc.GetRecentSalesAsync(r, take, ct));
            });
    }

    private static async Task<Results<Ok<T>, ValidationProblem>> Handle<T>(
        PeriodQuery q, PeriodResolver periods,
        Dictionary<string, string[]> errors,
        Func<DateRange, Task<T>> action)
    {
        periods.TryResolve(q.Period, q.From, q.To, errors, out var range);

        if (errors.Count > 0 || range is null)
            return TypedResults.ValidationProblem(errors);

        return TypedResults.Ok(await action(range));
    }

    private static RatingMetric ParseMetric(string? value, Dictionary<string, string[]> errors)
    {
        if (value is null) return RatingMetric.GrossProfit;
        if (Enum.TryParse<RatingMetric>(value, ignoreCase: true, out var metric) && Enum.IsDefined(metric))
            return metric;

        errors["sortBy"] = ["Допустимые значения: grossProfit, averageCheck."];
        return RatingMetric.GrossProfit;
    }

    private static int ParseLimit(int? value, int defaultValue, Dictionary<string, string[]> errors)
    {
        if (value is null) return defaultValue;
        if (value is >= 1 and <= 50) return value.Value;

        errors["limit"] = ["limit должен быть от 1 до 50."];
        return defaultValue;
    }
}