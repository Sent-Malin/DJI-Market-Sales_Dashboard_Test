using SalesDashboard.Api.Data;
using SalesDashboard.Api.Domain;

namespace SalesDashboard.Tests.Infrastructure;

/// <summary>
/// Минимальные данные с прозрачной арифметикой: один товар с себестоимостью 600,
/// один клиент, менеджеры и продажи создаются явно в каждом тесте.
/// </summary>
public sealed class TestData(SalesDbContext db, TimeZoneInfo timeZone)
{
    public const decimal UnitCost = 600m;

    private readonly Product _product = new()
    {
        Sku = "TEST-1",
        Name = "Тестовый товар",
        ListPrice = 1000m,
        UnitCost = UnitCost,
        Category = new Category { Name = "Тест" },
    };

    private readonly Customer _customer = new()
    {
        Name = "Тестовый клиент",
        Company = "ООО «Тест»",
        Segment = CustomerSegment.Smb,
    };

    public Manager Manager(string fullName, bool isActive = true)
    {
        var manager = new Manager
        {
            FullName = fullName,
            Team = "Тест",
            Position = "Менеджер",
            IsActive = isActive,
            HiredAt = new DateOnly(2020, 1, 1),
        };
        db.Managers.Add(manager);
        return manager;
    }

    /// <param name="moscowTime">Локальное время бизнеса (DateTimeKind.Unspecified).</param>
    public Sale Sale(Manager manager, DateTime moscowTime, SaleStatus status, decimal price, int quantity = 1)
    {
        var sale = new Sale
        {
            Manager = manager,
            Customer = _customer,
            Status = status,
            SoldAt = TimeZoneInfo.ConvertTimeToUtc(moscowTime, timeZone),
        };
        sale.AddItem(_product, quantity, price);
        db.Sales.Add(sale);
        return sale;
    }

    public Task SaveAsync() => db.SaveChangesAsync();
}