namespace SalesDashboard.Api.Data.Seed;

internal enum Role { Drone, Camera, Battery, Accessory, Service }

/// <param name="Tier">0 — потребительский, 1 — профессиональный, 2 — промышленный</param>
internal sealed record ProductSpec(
    string Sku, string Name, string Category, Role Role, int Tier, decimal ListPrice, decimal CostRatio);

internal sealed record ManagerSpec(
    string FullName, string Team, string Position,
    int HiredDaysAgo, double DealsPerDay, double DiscountBias, double CancelRate,
    int? FiredDaysAgo = null, int? VacationFromDaysAgo = null, int? VacationToDaysAgo = null);

internal static class SeedCatalog
{
    public const string TeamEnterprise = "Enterprise";
    public const string TeamCorporate = "Корпоративные клиенты";
    public const string TeamSmb = "Малый бизнес";

    private const string Consumer = "Потребительские дроны";
    private const string Industrial = "Промышленные дроны";
    private const string Cameras = "Камеры и стабилизаторы";
    private const string Power = "Аккумуляторы и питание";
    private const string Accessories = "Аксессуары";
    private const string Services = "Сервис и обучение";

    public static readonly string[] Categories = [Consumer, Industrial, Cameras, Power, Accessories, Services];

    // Цены ориентировочные; CostRatio задаёт маржинальность категории
    public static readonly ProductSpec[] Products =
    [
        new("DJI-NEO",          "DJI Neo",                          Consumer,    Role.Drone,     0,    22_900m, 0.80m),
        new("DJI-FLIP",         "DJI Flip",                         Consumer,    Role.Drone,     0,    45_900m, 0.80m),
        new("DJI-MINI4K",       "DJI Mini 4K",                      Consumer,    Role.Drone,     0,    34_900m, 0.82m),
        new("DJI-MINI4PRO",     "DJI Mini 4 Pro",                   Consumer,    Role.Drone,     0,    94_900m, 0.78m),
        new("DJI-AIR3S",        "DJI Air 3S",                       Consumer,    Role.Drone,     0,   139_900m, 0.79m),
        new("DJI-AVATA2",       "DJI Avata 2",                      Consumer,    Role.Drone,     0,    89_900m, 0.80m),
        new("DJI-MAVIC3C",      "DJI Mavic 3 Classic",              Consumer,    Role.Drone,     1,   189_900m, 0.80m),
        new("DJI-MAVIC4PRO",    "DJI Mavic 4 Pro",                  Consumer,    Role.Drone,     1,   259_900m, 0.81m),

        new("DJI-M3E",          "DJI Mavic 3 Enterprise",           Industrial,  Role.Drone,     2,   449_000m, 0.76m),
        new("DJI-M3T",          "DJI Mavic 3 Thermal",              Industrial,  Role.Drone,     2,   699_000m, 0.76m),
        new("DJI-M4E",          "DJI Matrice 4E",                   Industrial,  Role.Drone,     2,   599_000m, 0.77m),
        new("DJI-M4T",          "DJI Matrice 4T",                   Industrial,  Role.Drone,     2,   849_000m, 0.77m),
        new("DJI-M350",         "DJI Matrice 350 RTK",              Industrial,  Role.Drone,     2, 1_290_000m, 0.78m),
        new("DJI-AGRAS-T25",    "DJI Agras T25",                    Industrial,  Role.Drone,     2, 1_390_000m, 0.82m),
        new("DJI-AGRAS-T50",    "DJI Agras T50",                    Industrial,  Role.Drone,     2, 2_150_000m, 0.82m),

        new("DJI-OSMO-P3",      "DJI Osmo Pocket 3",                Cameras,     Role.Camera,    0,    54_900m, 0.74m),
        new("DJI-OSMO-A5P",     "DJI Osmo Action 5 Pro",            Cameras,     Role.Camera,    0,    44_900m, 0.74m),
        new("DJI-OSMO-360",     "DJI Osmo 360",                     Cameras,     Role.Camera,    0,    52_900m, 0.75m),
        new("DJI-OSMO-M7P",     "DJI Osmo Mobile 7P",               Cameras,     Role.Camera,    0,    14_900m, 0.65m),
        new("DJI-RS4MINI",      "DJI RS 4 Mini",                    Cameras,     Role.Camera,    0,    44_900m, 0.72m),
        new("DJI-RS4PRO",       "DJI RS 4 Pro",                     Cameras,     Role.Camera,    1,    99_900m, 0.73m),
        new("DJI-MIC2",         "DJI Mic 2",                        Cameras,     Role.Camera,    0,    34_900m, 0.70m),

        new("DJI-BAT-MINI4",    "Аккумулятор Mini 4 Pro Plus",      Power,       Role.Battery,   0,     9_900m, 0.60m),
        new("DJI-BAT-MAVIC3",   "Аккумулятор Mavic 3",              Power,       Role.Battery,   1,    21_900m, 0.62m),
        new("DJI-HUB-MAVIC3",   "Зарядный хаб Mavic 3",             Power,       Role.Battery,   1,     8_900m, 0.55m),
        new("DJI-BAT-TB65",     "Аккумулятор TB65 (Matrice)",       Power,       Role.Battery,   2,   119_000m, 0.65m),
        new("DJI-BAT-DB1560",   "Аккумулятор DB1560 (Agras)",       Power,       Role.Battery,   2,   189_000m, 0.70m),
        new("DJI-POWER1000",    "Станция DJI Power 1000",           Power,       Role.Battery,   0,    89_900m, 0.72m),

        new("DJI-RC2",          "Пульт DJI RC 2",                   Accessories, Role.Accessory, 0,    39_900m, 0.66m),
        new("DJI-GOGGLES3",     "Очки DJI Goggles 3",               Accessories, Role.Accessory, 0,    69_900m, 0.70m),
        new("DJI-ND-MINI4",     "ND-фильтры Mini 4 Pro",            Accessories, Role.Accessory, 0,     5_900m, 0.45m),
        new("DJI-PROP-MAVIC3",  "Пропеллеры Mavic 3 (пара)",        Accessories, Role.Accessory, 0,     1_900m, 0.40m),
        new("DJI-BAG-MINI",     "Сумка DJI Mini",                   Accessories, Role.Accessory, 0,     4_900m, 0.45m),
        new("DJI-GUARD-AVATA2", "Защита пропеллеров Avata 2",       Accessories, Role.Accessory, 0,     2_900m, 0.42m),
        new("SD-256",           "Карта microSD 256 ГБ",             Accessories, Role.Accessory, 0,     3_900m, 0.50m),

        new("DJI-CARE-MINI",    "DJI Care Refresh (Mini), 1 год",   Services,    Role.Service,   0,     6_900m, 0.35m),
        new("DJI-CARE-MAVIC",   "DJI Care Refresh (Mavic), 1 год",  Services,    Role.Service,   1,    19_900m, 0.35m),
        new("DJI-CARE-ENT",     "DJI Care Enterprise Plus",         Services,    Role.Service,   2,    89_000m, 0.40m),
        new("EDU-PILOT",        "Курс пилота БАС (1 чел.)",         Services,    Role.Service,   2,    45_000m, 0.30m),
        new("REG-BAS",          "Регистрация и страхование БАС",    Services,    Role.Service,   1,    12_000m, 0.30m),
    ];

    // DealsPerDay — ожидаемое число сделок в рабочий день при сезонном коэффициенте 1.0
    public static readonly ManagerSpec[] Managers =
    [
        new("Анна Соколова",      TeamEnterprise, "Руководитель группы", 2100, 0.22, 0.03, 0.04),
        new("Максим Белов",       TeamEnterprise, "Старший менеджер",    1500, 0.18, 0.05, 0.05),
        new("Екатерина Лебедева", TeamEnterprise, "Менеджер",             900, 0.16, 0.10, 0.06), // много скидок → низкая маржа
        new("Артём Никитин",      TeamEnterprise, "Менеджер",             700, 0.12, 0.05, 0.12), // много отмен
        new("Ольга Воронцова",    TeamEnterprise, "Старший менеджер",    1300, 0.18, 0.05, 0.05, VacationFromDaysAgo: 55, VacationToDaysAgo: 40),

        new("Игорь Петров",       TeamCorporate,  "Руководитель группы", 1900, 0.55, 0.04, 0.04),
        new("Сергей Морозов",     TeamCorporate,  "Старший менеджер",    1400, 0.50, 0.05, 0.05),
        new("Наталья Козлова",    TeamCorporate,  "Менеджер",            1000, 0.42, 0.07, 0.05),
        new("Павел Зайцев",       TeamCorporate,  "Менеджер",             800, 0.38, 0.11, 0.07),
        new("Юлия Новикова",      TeamCorporate,  "Менеджер",             150, 0.40, 0.05, 0.05), // пришла ~5 мес. назад
        new("Роман Волков",       TeamCorporate,  "Менеджер",            1100, 0.35, 0.06, 0.06, FiredDaysAgo: 120),
        new("Алина Фёдорова",     TeamCorporate,  "Младший менеджер",     500, 0.25, 0.08, 0.14),

        new("Мария Кузнецова",    TeamSmb,        "Руководитель группы", 1700, 1.20, 0.02, 0.03),
        new("Алексей Смирнов",    TeamSmb,        "Старший менеджер",    1200, 1.10, 0.03, 0.04),
        new("Елена Попова",       TeamSmb,        "Менеджер",             900, 0.90, 0.05, 0.05),
        new("Кирилл Васильев",    TeamSmb,        "Менеджер",             850, 0.85, 0.05, 0.05),
        new("Татьяна Михайлова",  TeamSmb,        "Менеджер",             600, 0.80, 0.04, 0.05),
        new("Андрей Соловьёв",    TeamSmb,        "Менеджер",             450, 0.70, 0.07, 0.08),
        new("Виктория Егорова",   TeamSmb,        "Младший менеджер",     300, 0.50, 0.06, 0.16),
        new("Дмитрий Орлов",      TeamSmb,        "Младший менеджер",      10, 0.80, 0.05, 0.05), // на онбординге, продаж нет
    ];

    // Участники детерминированных edge-case сценариев
    public const string BigDealManager = "Анна Соколова";
    public const string BigRefundManager = "Екатерина Лебедева";
    public const string BelowCostManager = "Павел Зайцев";
    public const string MidnightManager = "Мария Кузнецова";
    public const string LateEveningManager = "Алексей Смирнов";
    public static readonly string[] TieManagers = ["Елена Попова", "Кирилл Васильев"];

    public static readonly string[] EnterpriseCompanies =
    [
        "АгроПром Юг", "ГеоСкан Проект", "ЭнергоИнспект", "Северная горнорудная компания", "СтройМонтаж Холдинг",
        "ТрансЛинк Логистик", "Лесной мониторинг", "АгроВектор", "НефтеГазСервис Групп", "УралМаркшейдер",
    ];

    public static readonly string[] CorporateCompanies =
    [
        "Медиа Продакшн", "Кадр 360", "АэроВидео", "ГеоТочка", "Кадастр Плюс", "СтройКонтроль", "ФотоПро",
        "ТурАэро", "Кровля Инспект", "Агро Мониторинг", "Вектор Съёмка", "Небо Медиа", "ТехноСкан", "ПромАльп",
        "Сити Фильм", "Лидар Системс", "ФермаТех", "Периметр Охрана", "Студия Север", "ГородСтрой",
    ];

    public static readonly string[] SmbCompanies =
    [
        "Свадебное видео", "Аэрофото Мастер", "Дрон Шоп", "Точка обзора", "Горизонт", "Видеомикс", "Пиксель",
        "Ракурс", "Летний кадр", "Фотосфера", "Высота", "Кадр за кадром", "Вертикаль", "Панорама", "Облако",
    ];

    public static readonly string[] MaleFirstNames =
        ["Александр", "Дмитрий", "Максим", "Сергей", "Андрей", "Алексей", "Артём", "Илья", "Кирилл", "Михаил", "Никита", "Иван", "Евгений", "Владимир", "Олег"];

    public static readonly string[] FemaleFirstNames =
        ["Анна", "Мария", "Елена", "Ольга", "Наталья", "Екатерина", "Татьяна", "Ирина", "Юлия", "Светлана", "Марина", "Дарья", "Ксения", "Виктория", "Алина"];

    // Мужская форма; женская = + «а»
    public static readonly string[] SurnameStems =
    [
        "Иванов", "Смирнов", "Кузнецов", "Попов", "Васильев", "Петров", "Соколов", "Михайлов", "Новиков", "Фёдоров",
        "Морозов", "Волков", "Алексеев", "Лебедев", "Семёнов", "Егоров", "Павлов", "Козлов", "Степанов", "Николаев",
    ];

    public static readonly string[] Initials = ["А", "В", "Г", "Д", "Е", "И", "Н", "О", "П", "С"];
}