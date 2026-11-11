#region

using Microsoft.EntityFrameworkCore;

#endregion

namespace BeerService;

public interface IBeerService
{
    IAsyncEnumerable<BeerEntity> FindAll();
    Task<BeerEntity> Create(BeerPayload beer, CancellationToken ct);
    Task Delete(int id, CancellationToken ct);
    Task<BeerEntity?> FindById(int id, CancellationToken ct);
    Task<BeerEntity?> Update(int id, BeerPayload toUpdate, CancellationToken ct);
}

public class BeerServiceImpl(AppDbContext dbContext) : IBeerService
{
    public IAsyncEnumerable<BeerEntity> FindAll() => dbContext.Beers.AsAsyncEnumerable();

    public async Task<BeerEntity> Create(BeerPayload beer, CancellationToken ct)
    {
        var entity = new BeerEntity
        {
            Details = new BeerDetails
            {
                Brand = beer.Brand,
                Name = beer.Name,
                Strength = beer.Strength,
            },
        };
        await dbContext.Beers.AddAsync(entity, ct);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public Task Delete(int id, CancellationToken ct) =>
        dbContext.Beers.Where(x => x.Id == id).ExecuteDeleteAsync(ct);

    public Task<BeerEntity?> FindById(int id, CancellationToken ct) =>
        dbContext.Beers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<BeerEntity?> Update(int id, BeerPayload toUpdate, CancellationToken ct)
    {
        var found = await FindById(id, ct);
        if (found == null)
            return null;

        found.Details.Name = toUpdate.Name;
        found.Details.Brand = toUpdate.Brand;
        found.Details.Strength = toUpdate.Strength;
        await dbContext.SaveChangesAsync(ct);
        return found;
    }
}
