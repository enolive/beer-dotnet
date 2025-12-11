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
    public static Arbitrary<BeerPayload> CreateBeer() => Generators.CreateBeer().ToArbitrary();
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
        var strengths = ArbMap.Default.ArbFor<double>().Generator;
        var ids = ArbMap.Default.ArbFor<PositiveInt>().Generator;
        return from name in names
            from brand in brands
            from strength in strengths
            from id in ids
            select new BeerEntity { Name = name, Brand = brand, Strength = strength, Id = id.Get };
    }

    public static Gen<BeerPayload> CreateBeer() =>
        BeerEntity().Select(x => new BeerPayload(x.Name, x.Brand, x.Strength));
}