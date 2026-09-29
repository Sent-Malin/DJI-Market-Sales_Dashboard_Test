using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain;

namespace SalesDashboard.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(SalesDbContext db, CancellationToken ct = default)
    {
        if (await db.Managers.AnyAsync(ct))
            return; // БД уже заполнена — повторный старт контейнера

        var anna = new Manager { FullName = "Анна Соколова", Team = "Enterprise", Position = "Senior Account Manager", HiredAt = new DateOnly(2021, 3, 1) };
        var igor = new Manager { FullName = "Игорь Петров", Team = "SMB", Position = "Account Manager", HiredAt = new DateOnly(2023, 6, 15) };
        var dmitry = new Manager { FullName = "Дмитрий Орлов", Team = "SMB", Position = "Junior Account Manager", HiredAt = new DateOnly(2026, 9, 1) }; // без продаж

        var aero = new Customer { Name = "Сергей Волков", Company = "АО «АэроГеоСервис»", Segment = CustomerSegment.Enterprise };
        var media = new Customer { Name = "Мария Ковалёва", Company = "ООО «Медиа Продакшн»", Segment = CustomerSegment.MidMarket };
        var ip = new Customer { Name = "Олег Смирнов", Company = "ИП Смирнов О.В.", Segment = CustomerSegment.Smb };

        var drones = new Category { Name = "Дроны" };
        var accessories = new Category { Name = "Аксессуары" };

        var mavic = new Product { Sku = "DJI-M3P", Name = "DJI Mavic 3 Pro", Category = drones, ListPrice = 250_000, UnitCost = 190_000 };
        var mini = new Product { Sku = "DJI-MINI4", Name = "DJI Mini 4 Pro", Category = drones, ListPrice = 95_000, UnitCost = 72_000 };
        var battery = new Product { Sku = "DJI-BAT-M3", Name = "Аккумулятор Mavic 3", Category = accessories, ListPrice = 15_000, UnitCost = 9_000 };
        var nd = new Product { Sku = "DJI-ND-SET", Name = "Набор ND-фильтров", Category = accessories, ListPrice = 8_000, UnitCost = 3_500 };

        // Даты считаем от «сегодня» по Москве, чтобы пресеты периода сразу показывали данные
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var todayMsk = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        DateTime At(int daysAgo, int hour) =>
            TimeZoneInfo.ConvertTimeToUtc(todayMsk.AddDays(-daysAgo).AddHours(hour), tz);

        Sale NewSale(Manager m, Customer c, DateTime at, SaleStatus status,
                     params (Product p, int qty, decimal price)[] items)
        {
            var sale = new Sale { Manager = m, Customer = c, SoldAt = at, Status = status };
            foreach (var (p, qty, price) in items) sale.AddItem(p, qty, price);
            return sale;
        }

        var sales = new[]
        {
            NewSale(anna, aero,  At(0, 0),  SaleStatus.Paid,      (mavic, 2, 245_000)),                    // ровно полночь «сегодня»
            NewSale(igor, ip,    At(0, 11), SaleStatus.Paid,      (mini, 1, 95_000), (battery, 2, 15_000)),
            NewSale(anna, media, At(1, 15), SaleStatus.Paid,      (mini, 3, 92_000)),
            NewSale(igor, ip,    At(2, 10), SaleStatus.Cancelled, (mavic, 1, 250_000)),
            NewSale(anna, aero,  At(3, 12), SaleStatus.Paid,      (mavic, 5, 235_000), (battery, 10, 14_000)), // крупная сделка
            NewSale(igor, media, At(4, 16), SaleStatus.Refunded,  (mini, 1, 95_000)),
            NewSale(igor, ip,    At(5, 13), SaleStatus.Paid,      (nd, 3, 8_000)),
            NewSale(anna, media, At(6, 0),  SaleStatus.Paid,      (battery, 4, 15_000)),                   // граница «7 дней»
            NewSale(igor, ip,    At(7, 23), SaleStatus.Paid,      (nd, 1, 8_000)),                         // сразу за границей
            NewSale(anna, ip,    At(10, 14), SaleStatus.Paid,     (mini, 1, 95_000)),
            NewSale(igor, media, At(15, 11), SaleStatus.Paid,     (mavic, 1, 250_000)),
            NewSale(anna, aero,  At(20, 10), SaleStatus.Refunded, (mavic, 2, 240_000)),
            NewSale(igor, ip,    At(25, 17), SaleStatus.Paid,     (battery, 3, 15_000), (nd, 2, 7_500)),
            NewSale(anna, media, At(35, 12), SaleStatus.Paid,     (mini, 2, 93_000)),                      // предыдущий 30-дн. период
            NewSale(igor, aero,  At(45, 15), SaleStatus.Cancelled,(mini, 4, 90_000)),
            NewSale(anna, ip,    At(50, 10), SaleStatus.Paid,     (nd, 5, 7_000)),
            NewSale(igor, media, At(60, 14), SaleStatus.Paid,     (mavic, 1, 248_000)),
            NewSale(anna, aero,  At(70, 11), SaleStatus.Paid,     (mavic, 3, 238_000), (mini, 2, 91_000)),
        };

        db.Managers.Add(dmitry);   // у него нет продаж, поэтому граф через sales его не добавит
        db.Sales.AddRange(sales);  // менеджеры, клиенты, товары, категории добавятся по графу
        await db.SaveChangesAsync(ct);
    }
}