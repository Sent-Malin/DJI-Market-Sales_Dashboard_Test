using SalesDashboard.Api.Domain;
using SalesDashboard.Tests.Infrastructure;
using Xunit;

namespace SalesDashboard.Tests;

[Collection(PostgresCollection.Name)]
public sealed class TimeseriesTests(PostgresFixture postgres) : DashboardTestBase(postgres)
{
    [Fact]
    public async Task Returns_continuous_daily_series_grouped_by_business_date()
    {
        var anna = Data.Manager("Анна");
        // 25.09 00:30 МСК = 24.09 21:30 UTC → должна попасть в точку 25.09, а не 24.09
        Data.Sale(anna, new DateTime(2026, 9, 25, 0, 30, 0), SaleStatus.Paid, 1000m);
        Data.Sale(anna, Sep(28, 15), SaleStatus.Paid, 500m);
        Data.Sale(anna, Sep(28, 16), SaleStatus.Cancelled, 9000m);
        await Data.SaveAsync();

        var ts = await Service.GetTimeseriesAsync(Range("7d"), default);

        Assert.Equal(7, ts.Points.Count);
        Assert.Equal(new DateOnly(2026, 9, 24), ts.Points[0].Date);

        var sep25 = ts.Points.Single(p => p.Date == new DateOnly(2026, 9, 25));
        Assert.Equal(1000m, sep25.Revenue);
        Assert.Equal(1, sep25.SalesCount);

        var sep28 = ts.Points.Single(p => p.Date == new DateOnly(2026, 9, 28));
        Assert.Equal(500m, sep28.Revenue);          // отмена не учитывается
        Assert.Equal(1, sep28.SalesCount);

        Assert.Equal(5, ts.Points.Count(p => p.SalesCount == 0)); // пропуски заполнены нулями
    }
}