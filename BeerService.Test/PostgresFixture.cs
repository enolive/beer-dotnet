using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BeerService.Test;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public AppDbContext DbContext { get; private set; } = null!; // initialized on async lifetime

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await postgres.StartAsync(ct);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        DbContext = new AppDbContext(options);
        await DbContext.Database.MigrateAsync(ct); // Create tables
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await postgres.StopAsync(CancellationToken.None);
    }
}
