#region

using FluentAssertions;
using FsCheck.Xunit;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

#endregion

namespace BeerService.Test;

[TestSubject(typeof(BeerServiceImpl))]
public class BeerServiceImplTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    private AppDbContext _context = default!;
    private BeerServiceImpl _sut = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync(); // Create tables
        _sut = new BeerServiceImpl(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.StopAsync();
    }

    [Property(MaxTest = 1)]
    public async Task Should_create_beer(BeerPayload createBeer)
    {
        var result = await _sut.Create(createBeer);

        result.Id.Should().BeGreaterThan(0);
        result.Should().BeEquivalentTo(createBeer, x => x.ExcludingMissingMembers());
        _sut.FindAll().Should().Contain(result);
        var found = await _sut.FindById(result.Id);
        found.Should().BeEquivalentTo(result);
    }

    [Property(MaxTest = 1)]
    public async Task Should_delete_beer(BeerPayload createBeer)
    {
        var created = await _sut.Create(createBeer);

        await _sut.Delete(created.Id);

        var found = await _sut.FindById(created.Id);
        found.Should().BeNull();
    }

    [Property(MaxTest = 1)]
    public async Task Should_update_beer(BeerPayload createBeer, BeerPayload updateBeer)
    {
        var created = await _sut.Create(createBeer);

        await _sut.Update(created.Id, updateBeer);

        var found = await _sut.FindById(created.Id);
        found.Should().BeEquivalentTo(updateBeer, x => x.ExcludingMissingMembers());
    }
}