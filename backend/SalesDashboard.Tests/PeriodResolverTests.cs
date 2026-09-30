using System.Globalization;
using SalesDashboard.Api.Features.Dashboard;
using SalesDashboard.Tests.Infrastructure;
using Xunit;

namespace SalesDashboard.Tests;

public sealed class PeriodResolverTests
{
    private static readonly TimeZoneInfo Moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    private static PeriodResolver At(int year, int month, int day, int hour = 12, int minute = 0) =>
        new(new FixedTimeProvider(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(3))), Moscow);

    private static DateRange Resolve(PeriodResolver resolver, string period, DateOnly? from = null, DateOnly? to = null)
    {
        var errors = new Dictionary<string, string[]>();
        Assert.True(resolver.TryResolve(period, from, to, errors, out var range), string.Join("; ", errors.Keys));
        return range!;
    }

    private static DateOnly D(int year, int month, int day) => new(year, month, day);

    [Fact]
    public void Seven_days_include_today_and_previous_period_has_same_length()
    {
        var r = Resolve(At(2026, 9, 30), "7d");

        Assert.Equal(D(2026, 9, 24), r.From);
        Assert.Equal(D(2026, 9, 30), r.To);
        Assert.Equal(D(2026, 9, 17), r.PreviousFrom);
        Assert.Equal(D(2026, 9, 23), r.PreviousTo);

        // Полуинтервал [from, to) в UTC: полночь МСК = 21:00 UTC предыдущего дня
        Assert.Equal(new DateTime(2026, 9, 23, 21, 0, 0, DateTimeKind.Utc), r.FromUtc);
        Assert.Equal(new DateTime(2026, 9, 30, 21, 0, 0, DateTimeKind.Utc), r.ToUtc);
    }

    [Fact]
    public void Today_is_determined_in_business_time_zone_not_utc()
    {
        // 01.10 00:30 МСК = 30.09 21:30 UTC → «сегодня» уже 1 октября
        var r = Resolve(At(2026, 10, 1, 0, 30), "today");

        Assert.Equal(D(2026, 10, 1), r.From);
        Assert.Equal(D(2026, 9, 30), r.PreviousFrom);
    }

    [Fact]
    public void This_month_compares_same_number_of_days_clamped_to_previous_month_end()
    {
        var r = Resolve(At(2026, 3, 31), "thisMonth");

        Assert.Equal(D(2026, 3, 1), r.From);
        Assert.Equal(D(2026, 3, 31), r.To);
        Assert.Equal(D(2026, 2, 1), r.PreviousFrom);
        Assert.Equal(D(2026, 2, 28), r.PreviousTo); // в феврале нет 31-го
    }

    [Fact]
    public void Last_month_compares_with_whole_month_before()
    {
        var r = Resolve(At(2026, 9, 30), "lastMonth");

        Assert.Equal(D(2026, 8, 1), r.From);
        Assert.Equal(D(2026, 8, 31), r.To);
        Assert.Equal(D(2026, 7, 1), r.PreviousFrom);
        Assert.Equal(D(2026, 7, 31), r.PreviousTo);
    }

    [Theory]
    [InlineData(1, Granularity.Day)]
    [InlineData(62, Granularity.Day)]
    [InlineData(63, Granularity.Week)]
    [InlineData(190, Granularity.Week)]
    [InlineData(191, Granularity.Month)]
    public void Granularity_depends_on_range_length(int days, Granularity expected)
    {
        var to = D(2026, 9, 30);
        var r = Resolve(At(2026, 9, 30), "custom", to.AddDays(-(days - 1)), to);

        Assert.Equal(expected, r.Granularity);
    }

    [Theory]
    [InlineData("custom", "2026-09-10", "2026-09-01", "to")]     // from > to
    [InlineData("custom", "2026-09-10", null, "to")]             // нет to
    [InlineData("custom", null, "2026-09-10", "from")]           // нет from
    [InlineData("custom", "2024-01-01", "2026-09-01", "to")]     // длиннее 731 дня
    [InlineData("yesterday", null, null, "period")]              // неизвестный пресет
    public void Invalid_input_is_rejected_with_field_error(string period, string? from, string? to, string field)
    {
        var errors = new Dictionary<string, string[]>();

        var ok = At(2026, 9, 30).TryResolve(period, Parse(from), Parse(to), errors, out _);

        Assert.False(ok);
        Assert.Contains(field, errors.Keys);
    }

    private static DateOnly? Parse(string? value) =>
        value is null ? null : DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}