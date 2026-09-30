using Microsoft.EntityFrameworkCore;
using Npgsql;
using SalesDashboard.Api.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace SalesDashboard.Tests.Infrastructure;

/// <summary>Один контейнер PostgreSQL на весь прогон, отдельная БД на каждый тест.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<SalesDbContext> CreateDatabaseAsync()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        var db = new SalesDbContext(options);
        await db.Database.MigrateAsync(); // создаёт БД и применяет те же миграции, что в production
        return db;
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}