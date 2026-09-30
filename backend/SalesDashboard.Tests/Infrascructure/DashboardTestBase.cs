using SalesDashboard.Api.Data;
using SalesDashboard.Api.Features.Dashboard;
using Xunit;

namespace SalesDashboard.Tests.Infrastructure;

public abstract class DashboardTestBase(PostgresFixture postgres) : IAsyncLifetime
{
    protected static readonly TimeZoneInfo Moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    /// <summary>«Сейчас» во всех тестах: 30.09.2026 12:00 МСК.</summary>
    protected static readonly DateTimeOffset Now = new(2026, 9, 30, 12, 0, 0, TimeSpan.FromHours(3));

    protected SalesDbContext Db { get; private set; } = null!;
    protected TestData Data { get; private set; } = null!;
    protected PeriodResolver Periods { get; } = new(new FixedTimeProvider(Now), Moscow);
    protected DashboardService Service => new(Db, Periods);

    public async Task InitializeAsync()
    {
        Db = await postgres.CreateDatabaseAsync();
        Data = new TestData(Db, Moscow);
    }

    public async Task DisposeAsync() => await Db.DisposeAsync();

    protected DateRange Range(string period, DateOnly? from = null, DateOnly? to = null)
    {
        var errors = new Dictionary<string, string[]>();
        Assert.True(Periods.TryResolve(period, from, to, errors, out var range), string.Join("; ", errors.Keys));
        return range!;
    }

    /// <summary>Сентябрь 2026, по умолчанию полдень по Москве.</summary>
    protected static DateTime Sep(int day, int hour = 12) => new(2026, 9, day, hour, 0, 0);
}