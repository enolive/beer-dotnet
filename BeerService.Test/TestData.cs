#region

using FsCheck;
using FsCheck.Fluent;
using JetBrains.Annotations;

#endregion

namespace BeerService.Test;

public static class Arbitraries
{
    [UsedImplicitly]
    public static Arbitrary<BeerEntity> BeerEntity() => Generators.BeerEntity().ToArbitrary();

    [UsedImplicitly]
    public static Arbitrary<BeerPayload> BeerPayload() => Generators.BeerPayload().ToArbitrary();
}

public static class Generators
{
    private static Gen<string> AlphanumericString()
    {
        var alphaNumChar = Gen.Elements(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray()
        );
        var alphaNumStringGen =
            alphaNumChar.ListOf().Select(chars => new string(chars.ToArray()));
        return alphaNumStringGen;
    }

    public static Gen<BeerEntity> BeerEntity()
    {
        var names = AlphanumericString();
        var brands = AlphanumericString();
        var strengths = ArbMap.Default.ArbFor<NormalFloat>().Generator;
        var ids = ArbMap.Default.ArbFor<PositiveInt>().Generator;
        // applicative way of constructing the type that is usually superb to a monadic chain using LINQ
        return names.Zip(brands).Zip(strengths).Zip(ids).Select(it =>
        {
            // unroll the tuple craziness
            var (((name, brand), strength), id) = it;
            return new BeerEntity
            {
                Id = id.Get,
                Details = new BeerDetails { Name = name, Brand = brand, Strength = strength.Get }
            };
        });
    }

    public static Gen<BeerPayload> BeerPayload() =>
        BeerEntity()
            .Select(x => x.Details)
            .Select(x => new BeerPayload(x.Name, x.Brand, x.Strength));
}