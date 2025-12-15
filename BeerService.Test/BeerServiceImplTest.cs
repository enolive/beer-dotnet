#region

using AwesomeAssertions;
using FsCheck;
using FsCheck.Xunit;
using JetBrains.Annotations;

#endregion

namespace BeerService.Test;

[TestSubject(typeof(BeerServiceImpl))]
public class BeerServiceImplTest(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private const int MaxIterations = 10;

    private readonly BeerServiceImpl _sut = new(postgres.DbContext);

    [Property(MaxTest = MaxIterations)]
    public async Task Should_create_beer(BeerPayload createBeer)
    {
        var result = await _sut.Create(createBeer);

        result.Id.Should().BeGreaterThan(0);
        result.Should().BeEquivalentTo(createBeer, x => x.ExcludingMissingMembers());
        _sut.FindAll().Should().Contain(result);
        var found = await _sut.FindById(result.Id);
        found.Should().BeEquivalentTo(result);
    }

    [Property(MaxTest = MaxIterations)]
    public async Task Should_delete_beer(BeerPayload createBeer)
    {
        var created = await _sut.Create(createBeer);

        await _sut.Delete(created.Id);

        var found = await _sut.FindById(created.Id);
        found.Should().BeNull();
    }

    [Property(MaxTest = MaxIterations)]
    public async Task Should_update_beer(BeerPayload createBeer, BeerPayload updateBeer)
    {
        var created = await _sut.Create(createBeer);

        await _sut.Update(created.Id, updateBeer);

        var found = await _sut.FindById(created.Id);
        found.Should().BeEquivalentTo(updateBeer, x => x.ExcludingMissingMembers());
    }

    [Property(MaxTest = MaxIterations)]
    public async Task Should_not_update_missing_beer(PositiveInt id, BeerPayload updateBeer)
    {
        var updated = await _sut.Update(id.Get, updateBeer);

        updated.Should().BeNull();
    }
}