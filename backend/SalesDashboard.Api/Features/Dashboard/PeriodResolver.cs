using System.Diagnostics.CodeAnalysis;

namespace SalesDashboard.Api.Features.Dashboard;

public enum Granularity { Day, Week, Month }

/// <summary>
/// Даты — включительно, в бизнес-часовом поясе (для отображения).
/// UTC-моменты — полуинтервал [FromUtc, ToUtc) для запросов к БД.
/// </summary>
public sealed record DateRange(
    DateOnly From, DateOnly To,
    DateOnly PreviousFrom, DateOnly PreviousTo,
    DateTime FromUtc, DateTime ToUtc,
    DateTime PreviousFromUtc, DateTime PreviousToUtc,
    Granularity Granularity)
{
    public int Days => To.DayNumber - From.DayNumber + 1;
}

public sealed class PeriodResolver(TimeProvider clock, TimeZoneInfo timeZone)
{
    public const int MaxRangeDays = 731;

    private readonly record struct Bounds(DateOnly From, DateOnly To, DateOnly PrevFrom, DateOnly PrevTo);

    public TimeZoneInfo TimeZone => timeZone;

    public DateOnly Today()
    {
        var local = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), timeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public bool TryResolve(
        string? period, DateOnly? from, DateOnly? to,
        IDictionary<string, string[]> errors,
        [NotNullWhen(true)] out DateRange? range)
    {
        range = null;
        var today = Today();

        Bounds? bounds = (period ?? "30d").ToLowerInvariant() switch
        {
            "today" => SameLengthBefore(today, today),
            "7d" => SameLengthBefore(today.AddDays(-6), today),
            "30d" => SameLengthBefore(today.AddDays(-29), today),
            "thismonth" => MonthToDate(today),
            "lastmonth" => PreviousMonth(today),
            "custom" => Custom(from, to, errors),
            _ => Invalid(errors)
        };

        if (bounds is not { } b)
            return false;

        var days = b.To.DayNumber - b.From.DayNumber + 1;
        range = new DateRange(
            b.From, b.To, b.PrevFrom, b.PrevTo,
            ToUtc(b.From), ToUtc(b.To.AddDays(1)),
            ToUtc(b.PrevFrom), ToUtc(b.PrevTo.AddDays(1)),
            days <= 62 ? Granularity.Day : days <= 190 ? Granularity.Week : Granularity.Month);
        return true;
    }

    private DateTime ToUtc(DateOnly localDate) =>
        TimeZoneInfo.ConvertTimeToUtc(localDate.ToDateTime(TimeOnly.MinValue), timeZone);

    /// Предыдущий период — той же длины, непосредственно перед текущим.
    private static Bounds SameLengthBefore(DateOnly from, DateOnly to)
    {
        var days = to.DayNumber - from.DayNumber + 1;
        return new Bounds(from, to, from.AddDays(-days), from.AddDays(-1));
    }

    /// Месяц-к-дате сравниваем с тем же числом дней прошлого месяца (1–29 сент ↔ 1–29 авг).
    private static Bounds MonthToDate(DateOnly today)
    {
        var start = new DateOnly(today.Year, today.Month, 1);
        var prevStart = start.AddMonths(-1);
        var prevEnd = prevStart.AddDays(today.Day - 1);
        var prevMonthEnd = start.AddDays(-1);
        return new Bounds(start, today, prevStart, prevEnd > prevMonthEnd ? prevMonthEnd : prevEnd);
    }

    /// Прошлый месяц сравниваем с позапрошлым целиком.
    private static Bounds PreviousMonth(DateOnly today)
    {
        var thisStart = new DateOnly(today.Year, today.Month, 1);
        var start = thisStart.AddMonths(-1);
        return new Bounds(start, thisStart.AddDays(-1), start.AddMonths(-1), start.AddDays(-1));
    }

    private static Bounds? Custom(DateOnly? from, DateOnly? to, IDictionary<string, string[]> errors)
    {
        if (from is null) errors["from"] = ["Для period=custom параметр from обязателен."];
        if (to is null) errors["to"] = ["Для period=custom параметр to обязателен."];
        if (from is null || to is null) return null;

        if (from > to)
        {
            errors["to"] = ["Дата to должна быть не раньше from."];
            return null;
        }
        if (to.Value.DayNumber - from.Value.DayNumber + 1 > MaxRangeDays)
        {
            errors["to"] = [$"Диапазон не может превышать {MaxRangeDays} дней."];
            return null;
        }
        return SameLengthBefore(from.Value, to.Value);
    }

    private static Bounds? Invalid(IDictionary<string, string[]> errors)
    {
        errors["period"] = ["Допустимые значения: today, 7d, 30d, thisMonth, lastMonth, custom."];
        return null;
    }
}