namespace BeerService;

public static class BeerApi
{
    public static void ConfigureBeerApiEndpoints(this WebApplication webApplication)
    {
        webApplication
            .MapGet("/beers", (IBeerService beerService) => beerService.FindAll())
            .WithName("GetAllBeers");

        webApplication
            .MapPost(
                "/beers",
                async (BeerPayload beer, IBeerService beerService, CancellationToken ct) =>
                {
                    var created = await beerService.Create(beer, ct);
                    return Results.Created($"/beers/{created.Id}", created);
                }
            )
            .WithName("SaveBeer");

        webApplication
            .MapDelete(
                "/beers/{id:int}",
                async (int id, IBeerService beerService, CancellationToken ct) =>
                {
                    await beerService.Delete(id, ct);
                    return Results.NoContent();
                }
            )
            .WithName("DeleteBeer");

        webApplication
            .MapGet(
                "/beers/{id:int}",
                async (int id, IBeerService beerService, CancellationToken ct) =>
                    await beerService.FindById(id, ct) switch
                    {
                        null => Results.NoContent(),
                        var beer => Results.Ok(beer),
                    }
            )
            .WithName("GetBeerById");

        webApplication
            .MapPut(
                "/beers/{id:int}",
                async (int id, BeerPayload beer, IBeerService beerService, CancellationToken ct) =>
                    await beerService.Update(id, beer, ct) switch
                    {
                        null => Results.NoContent(),
                        var updated => Results.Ok(updated),
                    }
            )
            .WithName("UpdateBeer");
    }
}
