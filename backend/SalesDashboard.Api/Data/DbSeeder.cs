using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Data.Seed;

namespace SalesDashboard.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        SalesDbContext db, DateOnly today, TimeZoneInfo timeZone, ILogger logger, CancellationToken ct = default)
    {
        if (await db.Managers.AnyAsync(ct))
        {
            logger.LogInformation("Seed пропущен: в БД уже есть данные");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var data = new SalesGenerator(today, timeZone).Generate();

        db.Categories.AddRange(data.Categories);
        db.Products.AddRange(data.Products);
        db.Managers.AddRange(data.Managers);
        db.Customers.AddRange(data.Customers);
        db.Sales.AddRange(data.Sales);

        // Один SaveChanges = одна транзакция: БД либо заполнена целиком, либо пуста
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seed: {Sales} продаж ({Items} позиций), {Managers} менеджеров, {Customers} клиентов, {Products} товаров за {Elapsed} мс, anchor {Anchor}",
            data.Sales.Count, data.Sales.Sum(s => s.Items.Count), data.Managers.Count,
            data.Customers.Count, data.Products.Count, stopwatch.ElapsedMilliseconds, today);
    }
}