using SalesDashboard.Api.Domain;

namespace SalesDashboard.Api.Data.Seed;

/// <summary>
/// Детерминированный генератор демо-данных: фиксированный seed Random + все даты относительно anchor-дня.
/// Для одного и того же anchor результат идентичен.
/// </summary>
internal sealed class SalesGenerator(DateOnly today, TimeZoneInfo timeZone)
{
    public const int RandomSeed = 20_260_930;
    private const int HistoryDays = 365;
    private const int OnboardingDays = 14;
    private const int AnyTier = -1;

    // Янв … Дек: зимний спад, весенне-летний сезон полётов, пик Black Friday и Нового года
    private static readonly double[] MonthFactor = [0.55, 0.70, 0.90, 1.10, 1.25, 1.30, 1.20, 1.10, 1.00, 0.95, 1.25, 1.45];
    private static readonly CustomerSegment[] Segments =
        [CustomerSegment.Enterprise, CustomerSegment.MidMarket, CustomerSegment.Smb];

    private readonly Random _rng = new(RandomSeed);
    private readonly List<CatalogItem> _catalog = [];

    private sealed record CatalogItem(ProductSpec Spec, Product Entity);

    private sealed class ManagerProfile(ManagerSpec spec, Manager entity, Dictionary<CustomerSegment, List<Customer>> book)
    {
        public ManagerSpec Spec { get; } = spec;
        public Manager Entity { get; } = entity;
        /// <summary>Клиенты сегмента в персональном порядке менеджера: первые — «постоянные».</summary>
        public Dictionary<CustomerSegment, List<Customer>> Book { get; } = book;
    }

    public sealed record SeedData(
        List<Category> Categories, List<Product> Products, List<Manager> Managers,
        List<Customer> Customers, List<Sale> Sales);

    public SeedData Generate()
    {
        var categories = SeedCatalog.Categories.Select(name => new Category { Name = name }).ToList();
        var categoryByName = categories.ToDictionary(c => c.Name);

        foreach (var spec in SeedCatalog.Products)
        {
            _catalog.Add(new CatalogItem(spec, new Product
            {
                Sku = spec.Sku,
                Name = spec.Name,
                Category = categoryByName[spec.Category],
                ListPrice = spec.ListPrice,
                UnitCost = Round100(spec.ListPrice * spec.CostRatio),
            }));
        }

        var customers = GenerateCustomers();
        var bySegment = Segments.ToDictionary(s => s, s => customers.Where(c => c.Segment == s).ToList());

        var managers = SeedCatalog.Managers
            .Select(spec => new ManagerProfile(
                spec,
                new Manager
                {
                    FullName = spec.FullName,
                    Team = spec.Team,
                    Position = spec.Position,
                    HiredAt = today.AddDays(-spec.HiredDaysAgo),
                    IsActive = spec.FiredDaysAgo is null,
                },
                Segments.ToDictionary(s => s, s => Shuffle(bySegment[s]))))
            .ToList();

        var sales = new List<Sale>();
        var start = today.AddDays(-HistoryDays);
        var yesterday = today.AddDays(-1);

        // Полные дни до вчера включительно; «сегодня» содержит только edge case в полночь,
        // чтобы не было продаж «из будущего» относительно текущего времени
        for (var day = start; day < today; day = day.AddDays(1))
        {
            var progress = (double)(day.DayNumber - start.DayNumber) / HistoryDays;
            var demand = Seasonality(day) * WeekdayFactor(day) * (0.9 + 0.2 * progress); // + рост бизнеса за год

            foreach (var m in managers)
            {
                if (!CanSell(m.Spec, day)) continue;
                if (day == yesterday && SeedCatalog.TieManagers.Contains(m.Spec.FullName)) continue; // см. edge case «ничья»

                var deals = Poisson(m.Spec.DealsPerDay * demand);
                for (var i = 0; i < deals; i++)
                    sales.Add(RandomSale(m, day));
            }
        }

        AddEdgeCases(sales, managers, bySegment);

        return new SeedData(
            categories,
            _catalog.Select(c => c.Entity).ToList(),
            managers.Select(m => m.Entity).ToList(),
            customers,
            sales);
    }

    // ------------------------------------------------------------------ участники

    private List<Customer> GenerateCustomers()
    {
        var result = new List<Customer>();

        foreach (var company in SeedCatalog.EnterpriseCompanies)
            result.Add(new Customer { Company = $"АО «{company}»", Name = ContactName(), Segment = CustomerSegment.Enterprise });

        foreach (var company in SeedCatalog.CorporateCompanies)
            result.Add(new Customer { Company = $"ООО «{company}»", Name = ContactName(), Segment = CustomerSegment.MidMarket });

        foreach (var company in SeedCatalog.SmbCompanies)
            result.Add(new Customer { Company = $"ООО «{company}»", Name = ContactName(), Segment = CustomerSegment.Smb });

        for (var i = 0; i < 30; i++)
        {
            var contact = ContactName();          // «Имя Фамилия»
            var parts = contact.Split(' ');
            result.Add(new Customer
            {
                Company = $"ИП {parts[1]} {parts[0][0]}. {Pick(SeedCatalog.Initials)}.",
                Name = contact,
                Segment = CustomerSegment.Smb,
            });
        }

        return result;
    }

    private string ContactName()
    {
        var female = _rng.Next(2) == 0;
        var first = Pick(female ? SeedCatalog.FemaleFirstNames : SeedCatalog.MaleFirstNames);
        var last = Pick(SeedCatalog.SurnameStems) + (female ? "а" : "");
        return $"{first} {last}";
    }

    private bool CanSell(ManagerSpec m, DateOnly day)
    {
        var daysAgo = today.DayNumber - day.DayNumber;
        if (daysAgo > m.HiredDaysAgo - OnboardingDays) return false;                   // ещё не нанят или на онбординге
        if (m.FiredDaysAgo is { } fired && daysAgo <= fired) return false;             // уже уволен
        if (m.VacationFromDaysAgo is { } from && m.VacationToDaysAgo is { } to
            && daysAgo <= from && daysAgo >= to) return false;                         // в отпуске
        return true;
    }

    // ------------------------------------------------------------------ сделка

    private Sale RandomSale(ManagerProfile m, DateOnly day)
    {
        var segment = PickSegment(m.Spec.Team);
        var sale = new Sale
        {
            Manager = m.Entity,
            Customer = PickCustomer(m.Book[segment]),
            SoldAt = ToUtc(day, new TimeOnly(_rng.Next(9, 20), _rng.Next(60), _rng.Next(60))),
            Status = PickStatus(m.Spec.CancelRate, segment),
        };
        FillBasket(sale, segment, m.Spec.DiscountBias);
        return sale;
    }

    private CustomerSegment PickSegment(string team)
    {
        var r = _rng.NextDouble();
        return team switch
        {
            SeedCatalog.TeamEnterprise => r < 0.85 ? CustomerSegment.Enterprise : CustomerSegment.MidMarket,
            SeedCatalog.TeamCorporate => r < 0.80 ? CustomerSegment.MidMarket : r < 0.90 ? CustomerSegment.Enterprise : CustomerSegment.Smb,
            _ => r < 0.90 ? CustomerSegment.Smb : CustomerSegment.MidMarket,
        };
    }

    private Customer PickCustomer(List<Customer> book)
    {
        // Степенное распределение: клиенты в начале «книги» менеджера покупают заметно чаще
        var index = (int)(Math.Pow(_rng.NextDouble(), 2.2) * book.Count);
        return book[Math.Min(index, book.Count - 1)];
    }

    private SaleStatus PickStatus(double cancelRate, CustomerSegment segment)
    {
        var refundRate = segment switch
        {
            CustomerSegment.Smb => 0.05,
            CustomerSegment.MidMarket => 0.03,
            _ => 0.015,
        };
        var r = _rng.NextDouble();
        if (r < cancelRate) return SaleStatus.Cancelled;
        return r < cancelRate + refundRate ? SaleStatus.Refunded : SaleStatus.Paid;
    }

    private void FillBasket(Sale sale, CustomerSegment segment, double discountBias)
    {
        var (role, tier) = PickMainGroup(segment);
        var main = PickProduct(role, tier);
        var qty = PickQuantity(segment, main.Spec.Role);
        AddLine(sale, main, qty, segment, discountBias);

        if (main.Spec.Role == Role.Drone)
        {
            if (Chance(0.55)) AddLine(sale, PickProduct(Role.Battery, main.Spec.Tier), qty * _rng.Next(1, 3), segment, discountBias);
            if (Chance(0.35)) AddLine(sale, PickProduct(Role.Service, main.Spec.Tier), qty, segment, discountBias);
            if (main.Spec.Tier == 0 && Chance(0.30)) AddLine(sale, PickProduct(Role.Accessory, 0), _rng.Next(1, 3), segment, discountBias);
        }
        else if (main.Spec.Role == Role.Camera && Chance(0.40))
        {
            AddLine(sale, PickProduct(Role.Accessory, 0), 1, segment, discountBias);
        }
    }

    private (Role Role, int Tier) PickMainGroup(CustomerSegment segment)
    {
        var r = _rng.Next(100);
        return segment switch
        {
            CustomerSegment.Smb =>
                r < 45 ? (Role.Drone, 0) : r < 75 ? (Role.Camera, AnyTier) : r < 90 ? (Role.Accessory, AnyTier) : (Role.Drone, 1),
            CustomerSegment.MidMarket =>
                r < 20 ? (Role.Drone, 0) : r < 60 ? (Role.Drone, 1) : r < 75 ? (Role.Drone, 2) : r < 90 ? (Role.Camera, AnyTier) : (Role.Battery, AnyTier),
            _ =>
                r < 20 ? (Role.Drone, 1) : r < 90 ? (Role.Drone, 2) : (Role.Service, 2),
        };
    }

    private int PickQuantity(CustomerSegment segment, Role role)
    {
        if (role is Role.Accessory or Role.Battery)
            return _rng.Next(1, segment == CustomerSegment.Smb ? 4 : 8);

        var r = _rng.NextDouble();
        return segment switch
        {
            CustomerSegment.Smb => r < 0.80 ? 1 : r < 0.95 ? 2 : 3,
            CustomerSegment.MidMarket => r < 0.50 ? 1 : r < 0.85 ? _rng.Next(2, 4) : _rng.Next(4, 7),
            _ => r < 0.55 ? 1 : r < 0.90 ? _rng.Next(2, 4) : _rng.Next(4, 9),
        };
    }

    private CatalogItem PickProduct(Role role, int tier)
    {
        var candidates = _catalog.Where(c => c.Spec.Role == role && (tier == AnyTier || c.Spec.Tier == tier)).ToList();
        if (candidates.Count == 0)
            candidates = _catalog.Where(c => c.Spec.Role == role).ToList();
        return candidates[_rng.Next(candidates.Count)];
    }

    private void AddLine(Sale sale, CatalogItem item, int qty, CustomerSegment segment, double discountBias)
    {
        decimal price;
        if (Chance(0.01))
        {
            price = Round100(item.Entity.UnitCost * 0.92m); // распродажа ниже себестоимости
        }
        else
        {
            var segmentDiscount = segment switch
            {
                CustomerSegment.Enterprise => 0.04,
                CustomerSegment.MidMarket => 0.02,
                _ => 0.0,
            };
            var volumeDiscount = Math.Min(0.05, 0.01 * Math.Max(0, qty - 3));
            var discount = Math.Min(0.30, discountBias + segmentDiscount + volumeDiscount + _rng.NextDouble() * 0.03);
            price = Round100(item.Entity.ListPrice * (decimal)(1 - discount));
        }

        sale.AddItem(item.Entity, qty, price);
    }

    // ------------------------------------------------------------------ edge cases

    private void AddEdgeCases(
        List<Sale> sales, List<ManagerProfile> managers, Dictionary<CustomerSegment, List<Customer>> bySegment)
    {
        Manager M(string name) => managers.Single(m => m.Spec.FullName == name).Entity;
        Product P(string sku) => _catalog.Single(c => c.Spec.Sku == sku).Entity;
        decimal AtList(string sku, decimal k) => Round100(P(sku).ListPrice * k);

        Sale Deal(string manager, Customer customer, DateOnly day, TimeOnly time, SaleStatus status,
                  params (string Sku, int Qty, decimal Price)[] lines)
        {
            var sale = new Sale { Manager = M(manager), Customer = customer, SoldAt = ToUtc(day, time), Status = status };
            foreach (var (sku, qty, price) in lines) sale.AddItem(P(sku), qty, price);
            return sale;
        }

        var ent = bySegment[CustomerSegment.Enterprise];
        var mid = bySegment[CustomerSegment.MidMarket];
        var smb = bySegment[CustomerSegment.Smb];
        var yesterday = today.AddDays(-1);

        // 1. Одна очень крупная сделка (~28 млн ₽) — агрохолдинг закупает парк Agras
        sales.Add(Deal(SeedCatalog.BigDealManager, ent[0], today.AddDays(-100), new TimeOnly(14, 30), SaleStatus.Paid,
            ("DJI-AGRAS-T50", 12, AtList("DJI-AGRAS-T50", 0.90m)),
            ("DJI-BAT-DB1560", 24, AtList("DJI-BAT-DB1560", 0.92m)),
            ("DJI-CARE-ENT", 12, AtList("DJI-CARE-ENT", 0.85m))));

        // 2. Крупный возврат — не должен попасть в выручку
        sales.Add(Deal(SeedCatalog.BigRefundManager, ent[1], today.AddDays(-60), new TimeOnly(11, 15), SaleStatus.Refunded,
            ("DJI-M350", 2, AtList("DJI-M350", 0.93m))));

        // 3. Продажа ниже себестоимости — отрицательная валовая прибыль
        sales.Add(Deal(SeedCatalog.BelowCostManager, mid[0], today.AddDays(-20), new TimeOnly(16, 0), SaleStatus.Paid,
            ("DJI-MAVIC3C", 3, Round100(P("DJI-MAVIC3C").UnitCost * 0.90m))));

        // 4–5. Границы суток: ровно 00:00:00 сегодня и 23:59:59 вчера
        sales.Add(Deal(SeedCatalog.MidnightManager, smb[0], today, new TimeOnly(0, 0, 0), SaleStatus.Paid,
            ("DJI-OSMO-M7P", 1, AtList("DJI-OSMO-M7P", 1.00m))));
        sales.Add(Deal(SeedCatalog.LateEveningManager, smb[1], yesterday, new TimeOnly(23, 59, 59), SaleStatus.Paid,
            ("SD-256", 2, AtList("SD-256", 1.00m))));

        // 6. Ничья: вчера у двух менеджеров ровно по одной одинаковой продаже
        sales.Add(Deal(SeedCatalog.TieManagers[0], smb[2], yesterday, new TimeOnly(12, 0), SaleStatus.Paid,
            ("DJI-OSMO-P3", 2, 52_900m)));
        sales.Add(Deal(SeedCatalog.TieManagers[1], smb[3], yesterday, new TimeOnly(15, 0), SaleStatus.Paid,
            ("DJI-OSMO-P3", 2, 52_900m)));
    }

    // ------------------------------------------------------------------ утилиты

    private static double Seasonality(DateOnly d) =>
        d.Month == 1 && d.Day <= 8 ? 0.10 : MonthFactor[d.Month - 1]; // новогодние каникулы

    private static double WeekdayFactor(DateOnly d) => d.DayOfWeek switch
    {
        DayOfWeek.Saturday => 0.35,
        DayOfWeek.Sunday => 0.15,
        _ => 1.0,
    };

    /// Алгоритм Кнута: число событий за день при среднем lambda
    private int Poisson(double lambda)
    {
        var limit = Math.Exp(-lambda);
        var k = 0;
        var p = 1.0;
        do
        {
            k++;
            p *= _rng.NextDouble();
        } while (p > limit);
        return k - 1;
    }

    private bool Chance(double probability) => _rng.NextDouble() < probability;

    private T Pick<T>(IReadOnlyList<T> items) => items[_rng.Next(items.Count)];

    private List<T> Shuffle<T>(List<T> source)
    {
        var list = new List<T>(source);
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private DateTime ToUtc(DateOnly day, TimeOnly time) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(time), timeZone);

    private static decimal Round100(decimal value) =>
        Math.Round(value / 100m, MidpointRounding.AwayFromZero) * 100m;
}