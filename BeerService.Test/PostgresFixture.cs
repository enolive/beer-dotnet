using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BeerService.Test;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await postgres.StartAsync(ct);
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await postgres.StopAsync(CancellationToken.None);
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }
}
