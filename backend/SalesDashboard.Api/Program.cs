using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Data;
using SalesDashboard.Api.Features.Dashboard;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddDbContext<SalesDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention();

    if (builder.Environment.IsDevelopment())
        o.EnableSensitiveDataLogging();
});

var timeZoneId = builder.Configuration["Business:TimeZone"] ?? "Europe/Moscow";
builder.Services.AddHealthChecks().AddDbContextCheck<SalesDbContext>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new PeriodResolver(
    sp.GetRequiredService<TimeProvider>(),
    TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)));
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<SalesDbContext>();
    var periods = services.GetRequiredService<PeriodResolver>();
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

    // Seed:AnchorDate фиксирует «сегодня» генератора → идентичные данные в любой день
    var anchor = app.Configuration.GetValue<DateOnly?>("Seed:AnchorDate") ?? periods.Today();

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, anchor, periods.TimeZone, logger);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/api/health");
app.MapDashboardEndpoints();

app.Run();
