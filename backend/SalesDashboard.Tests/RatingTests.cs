using SalesDashboard.Api.Domain;
using SalesDashboard.Api.Features.Dashboard;
using SalesDashboard.Tests.Infrastructure;
using Xunit;

namespace SalesDashboard.Tests;

[Collection(PostgresCollection.Name)]
public sealed class RatingTests(PostgresFixture postgres) : DashboardTestBase(postgres)
{
    [Fact]
    public async Task Equal_results_share_rank_and_managers_without_sales_go_last()
    {
        var boris = Data.Manager("Борис");
        var anna = Data.Manager("Анна");
        var vera = Data.Manager("Вера");
        Data.Manager("Григорий");                                  // без продаж
        Data.Sale(boris, Sep(28), SaleStatus.Paid, 1000m);         // GP 400
        Data.Sale(anna, Sep(27), SaleStatus.Paid, 1000m);          // GP 400 — ничья с Борисом
        Data.Sale(vera, Sep(26), SaleStatus.Paid, 700m);           // GP 100
        await Data.SaveAsync();

        var r = await Service.GetManagerRatingAsync(Range("7d"), RatingMetric.GrossProfit, default);

        // При равенстве порядок стабилен: по выручке, затем по имени
        Assert.Equal(new[] { "Анна", "Борис", "Вера", "Григорий" }, r.Items.Select(i => i.FullName));
        Assert.Equal(new int?[] { 1, 1, 3, null }, r.Items.Select(i => i.Rank));
        Assert.Null(r.Items[3].AverageCheck);
        Assert.Null(r.Items[3].Margin);
    }

    [Fact]
    public async Task Switching_metric_to_average_check_reorders_managers()
    {
        var anna = Data.Manager("Анна");
        var boris = Data.Manager("Борис");
        Data.Sale(anna, Sep(25), SaleStatus.Paid, 1000m);
        Data.Sale(anna, Sep(26), SaleStatus.Paid, 1000m);
        Data.Sale(anna, Sep(27), SaleStatus.Paid, 1000m);          // GP 1200, средний чек 1000
        Data.Sale(boris, Sep(28), SaleStatus.Paid, 1500m);         // GP 900,  средний чек 1500
        await Data.SaveAsync();

        var byProfit = await Service.GetManagerRatingAsync(Range("7d"), RatingMetric.GrossProfit, default);
        var byCheck = await Service.GetManagerRatingAsync(Range("7d"), RatingMetric.AverageCheck, default);

        Assert.Equal("Анна", byProfit.Items[0].FullName);
        Assert.Equal("Борис", byCheck.Items[0].FullName);
        Assert.Equal(1500m, byCheck.Items[0].AverageCheck);
    }

    [Fact]
    public async Task Cancelled_and_refunded_sales_do_not_affect_rating()
    {
        var anna = Data.Manager("Анна");
        var boris = Data.Manager("Борис");
        Data.Sale(anna, Sep(27), SaleStatus.Paid, 1000m);          // GP 400
        Data.Sale(boris, Sep(26), SaleStatus.Refunded, 50_000m);
        Data.Sale(boris, Sep(27), SaleStatus.Cancelled, 50_000m);
        Data.Sale(boris, Sep(28), SaleStatus.Paid, 800m);          // GP 200
        await Data.SaveAsync();

        var r = await Service.GetManagerRatingAsync(Range("7d"), RatingMetric.GrossProfit, default);

        Assert.Equal("Анна", r.Items[0].FullName);
        var borisRow = r.Items.Single(i => i.FullName == "Борис");
        Assert.Equal(1, borisRow.SalesCount);
        Assert.Equal(800m, borisRow.Revenue);
    }

    [Fact]
    public async Task Inactive_manager_is_shown_only_when_he_sold_in_period()
    {
        Data.Manager("Анна");                                      // активна, без продаж → в списке
        var boris = Data.Manager("Борис", isActive: false);        // уволен, но продавал → в списке
        Data.Manager("Вера", isActive: false);                     // уволена, без продаж → скрыта
        Data.Sale(boris, Sep(28), SaleStatus.Paid, 1000m);
        await Data.SaveAsync();

        var r = await Service.GetManagerRatingAsync(Range("7d"), RatingMetric.GrossProfit, default);

        Assert.Equal(new[] { "Борис", "Анна" }, r.Items.Select(i => i.FullName));
    }
}