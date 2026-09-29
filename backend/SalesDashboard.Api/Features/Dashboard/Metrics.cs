namespace SalesDashboard.Api.Features.Dashboard;

public static class Metrics
{
    public static decimal? Margin(decimal revenue, decimal grossProfit) =>
        revenue == 0 ? null : Math.Round(grossProfit / revenue, 4);

    public static decimal? AverageCheck(decimal revenue, int salesCount) =>
        salesCount == 0 ? null : Math.Round(revenue / salesCount, 2);

    /// Относительное изменение. Abs в знаменателе — прибыль может быть отрицательной.
    public static decimal? RelativeChange(decimal? current, decimal? previous) =>
        current is null || previous is null or 0m
            ? null
            : Math.Round((current.Value - previous.Value) / Math.Abs(previous.Value), 4);

    /// Абсолютная разница — для маржи (в долях, т.е. п.п. / 100).
    public static decimal? AbsoluteChange(decimal? current, decimal? previous) =>
        current is null || previous is null ? null : current - previous;
}

public static class RankCalculator
{
    /// <summary>
    /// Competition ranking («1-2-2-4»): равные значения делят место.
    /// Элементы с Value = null (нет продаж) места не получают.
    /// </summary>
    public static Dictionary<int, int> Rank(IEnumerable<(int Id, decimal? Value)> items)
    {
        var ordered = items.Where(x => x.Value is not null)
                           .OrderByDescending(x => x.Value)
                           .ToList();
        var ranks = new Dictionary<int, int>(ordered.Count);
        for (var i = 0; i < ordered.Count; i++)
        {
            ranks[ordered[i].Id] = i > 0 && ordered[i].Value == ordered[i - 1].Value
                ? ranks[ordered[i - 1].Id]
                : i + 1;
        }
        return ranks;
    }
}