using SalesDashboard.Api.Domain;
using SalesDashboard.Tests.Infrastructure;
using Xunit;

namespace SalesDashboard.Tests;

[Collection(PostgresCollection.Name)]
public sealed class SummaryTests(PostgresFixture postgres) : DashboardTestBase(postgres)
{
    [Fact]
    public async Task Only_paid_sales_count_while_refunds_and_cancellations_are_reported_separately()
    {
        var anna = Data.Manager("Анна");
        Data.Sale(anna, Sep(29), SaleStatus.Paid, 1000m);
        Data.Sale(anna, Sep(28), SaleStatus.Paid, 1500m);
        Data.Sale(anna, Sep(27), SaleStatus.Cancelled, 10_000m);
        Data.Sale(anna, Sep(26), SaleStatus.Refunded, 2000m);
        await Data.SaveAsync();

        var s = await Service.GetSummaryAsync(Range("7d"), default);

        Assert.Equal(2500m, s.Revenue.Value);
        Assert.Equal(1300m, s.GrossProfit.Value);   // 2500 − 2 × 600
        Assert.Equal(0.52m, s.Margin.Value);        // 1300 / 2500
        Assert.Equal(2m, s.SalesCount.Value);
        Assert.Equal(1250m, s.AverageCheck.Value);  // 2500 / 2
        Assert.Equal(1, s.Refunds.Count);
        Assert.Equal(2000m, s.Refunds.Amount);
        Assert.Equal(1, s.CancelledCount);
    }

    [Fact]
    public async Task Period_boundaries_are_half_open_in_business_time_zone()
    {
        var anna = Data.Manager("Анна");
        Data.Sale(anna, new DateTime(2026, 9, 30, 0, 0, 0), SaleStatus.Paid, 1000m);    // ровно начало «сегодня» → текущий
        Data.Sale(anna, new DateTime(2026, 9, 29, 23, 59, 59), SaleStatus.Paid, 400m);  // последняя секунда вчера → предыдущий
        Data.Sale(anna, new DateTime(2026, 10, 1, 0, 0, 0), SaleStatus.Paid, 9999m);    // начало завтра → никуда
        await Data.SaveAsync();

        var s = await Service.GetSummaryAsync(Range("today"), default);

        Assert.Equal(1000m, s.Revenue.Value);
        Assert.Equal(400m, s.Revenue.Previous);
        Assert.Equal(1.5m, s.Revenue.Change);       // (1000 − 400) / 400
    }

    [Fact]
    public async Task Empty_period_returns_zeros_and_nulls_instead_of_division_by_zero()
    {
        Data.Manager("Анна");
        await Data.SaveAsync();

        var s = await Service.GetSummaryAsync(Range("today"), default);

        Assert.Equal(0m, s.Revenue.Value);
        Assert.Null(s.Revenue.Change);
        Assert.Null(s.Margin.Value);
        Assert.Null(s.AverageCheck.Value);
        Assert.Null(s.BestManager);
    }
}