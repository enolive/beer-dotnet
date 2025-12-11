namespace BeerService;

public static class BeerApi
{
    public static void ConfigureBeerApiEndpoints(this WebApplication webApplication)
    {
        webApplication.MapGet("/beers", (IBeerService beerService) => beerService.FindAll())
            .WithName("GetAllBeers");

        webApplication.MapPost("/beers", async (BeerPayload beer, IBeerService beerService) =>
            {
                var created = await beerService.Create(beer);
                return Results.Created($"/beers/{created.Id}", created);
            })
            .WithName("SaveBeer");

        webApplication.MapDelete("/beers/{id:int}", async (int id, IBeerService beerService) =>
            {
                await beerService.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeleteBeer");

        webApplication.MapGet("/beers/{id:int}", async (int id, IBeerService beerService) =>
                await beerService.FindById(id) switch
                {
                    null => Results.NoContent(),
                    var beer => Results.Ok(beer)
                })
            .WithName("GetBeerById");

        webApplication.MapPut("/beers/{id:int}",
                async (int id, BeerPayload beer, IBeerService beerService) =>
                    await beerService.Update(id, beer) switch
                    {
                        null => Results.NoContent(),
                        var updated => Results.Ok(updated)
                    })
            .WithName("UpdateBeer");
    }
}