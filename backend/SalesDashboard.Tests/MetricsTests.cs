using SalesDashboard.Api.Features.Dashboard;
using Xunit;

namespace SalesDashboard.Tests;

public sealed class MetricsTests
{
    [Fact]
    public void Equal_values_share_rank_and_next_rank_is_skipped()
    {
        var ranks = RankCalculator.Rank(new (int, decimal?)[]
        {
            (1, 100m), (2, 50m), (3, 50m), (4, 10m), (5, null),
        });

        Assert.Equal(1, ranks[1]);
        Assert.Equal(2, ranks[2]);
        Assert.Equal(2, ranks[3]);
        Assert.Equal(4, ranks[4]);          // competition ranking: 1-2-2-4
        Assert.False(ranks.ContainsKey(5)); // без значения — без места
    }

    [Fact]
    public void Relative_change_is_null_when_previous_is_zero()
    {
        Assert.Null(Metrics.RelativeChange(100m, 0m));
        Assert.Null(Metrics.RelativeChange(100m, null));
    }

    [Fact]
    public void Relative_change_from_negative_value_keeps_correct_sign()
    {
        // Прибыль выросла с −100 до +50: это рост на 150%, а не падение
        Assert.Equal(1.5m, Metrics.RelativeChange(50m, -100m));
    }

    [Fact]
    public void Margin_and_average_check_are_null_without_sales()
    {
        Assert.Null(Metrics.Margin(0m, 0m));
        Assert.Null(Metrics.AverageCheck(0m, 0));
    }
}