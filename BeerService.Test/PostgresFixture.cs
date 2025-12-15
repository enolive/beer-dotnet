using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BeerService.Test;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public AppDbContext DbContext { get; private set; }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        DbContext = new AppDbContext(options);
        await DbContext.Database.EnsureCreatedAsync(); // Create tables
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgres.StopAsync();
    }
}