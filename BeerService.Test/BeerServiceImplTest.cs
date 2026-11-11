#region

using AwesomeAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

#endregion

namespace BeerService.Test;

public class BeerServiceImplTest(PostgresFixture postgres)
    : IClassFixture<PostgresFixture>,
        IAsyncLifetime
{
    private readonly BeerServiceImpl sut = new(postgres.DbContext);
    private readonly CancellationToken ct = TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await postgres.DbContext.Beers.ExecuteDeleteAsync(ct);
    }

    public class Examples(PostgresFixture postgres) : BeerServiceImplTest(postgres)
    {
        [Fact]
        public async Task Should_create_beer()
        {
            var createBeer = new BeerPayload("Wasser", "Nestle", 0.0);
            var result = await sut.Create(createBeer, ct);

            result.Id.Should().BeGreaterThan(0);
            result.Should().BeEquivalentTo(createBeer, x => x.ExcludingMissingMembers());
            var beers = await sut.FindAll().ToListAsync(ct);
            beers.Should().Contain(result);
            var found = await sut.FindById(result.Id, ct);
            found.Should().BeEquivalentTo(result);
        }
    }

    [Properties(MaxTest = 10, Arbitrary = [typeof(Arbs)])]
    public class Properties(PostgresFixture postgres) : BeerServiceImplTest(postgres)
    {
        [Property]
        public async Task Should_create_beer(BeerPayload createBeer)
        {
            var result = await sut.Create(createBeer, ct);

            result.Id.Should().BeGreaterThan(0);
            result.Should().BeEquivalentTo(createBeer, x => x.ExcludingMissingMembers());
            var beers = await sut.FindAll().ToListAsync(ct);
            beers.Should().Contain(result);
            var found = await sut.FindById(result.Id, ct);
            found.Should().BeEquivalentTo(result);
        }

        [Property]
        public async Task Should_delete_beer(BeerPayload createBeer)
        {
            var created = await sut.Create(createBeer, ct);

            await sut.Delete(created.Id, ct);

            var found = await sut.FindById(created.Id, ct);
            found.Should().BeNull();
        }

        [Property]
        public async Task Should_update_beer(BeerPayload createBeer, BeerPayload updateBeer)
        {
            var created = await sut.Create(createBeer, ct);

            await sut.Update(created.Id, updateBeer, ct);

            var found = await sut.FindById(created.Id, ct);
            found.Should().BeEquivalentTo(updateBeer, x => x.ExcludingMissingMembers());
        }

        [Property]
        public async Task Should_not_update_missing_beer(PositiveInt id, BeerPayload updateBeer)
        {
            // make sure that the entry does not exist from a previous run
            await sut.Delete(id.Get, ct);

            var updated = await sut.Update(id.Get, updateBeer, ct);

            updated.Should().BeNull();
        }
    }

    public static class Arbs
    {
        private static readonly IArbMap Map = ArbMap.Default;

        [UsedImplicitly]
        public static Arbitrary<BeerPayload> ValidPayload() =>
            (
                from name in String()
                from brand in String()
                from strength in Map.GeneratorFor<NormalFloat>()
                select new BeerPayload(name, brand, strength.Get)
            ).ToArbitrary();

        private static Gen<string> String() =>
            ArbMap.Default.GeneratorFor<string>().Where(s => s is not null && !s.Contains('\0'));
    }
}
