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

// Миграции + seed при старте (требование ТЗ: запуск одной командой)
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/api/health");
app.MapDashboardEndpoints();

app.Run();
