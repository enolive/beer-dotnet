#region

using Microsoft.EntityFrameworkCore;

#endregion

namespace BeerService;

public interface IBeerService
{
    IEnumerable<BeerEntity> FindAll();
    Task<BeerEntity> Create(BeerPayload beer);
    Task Delete(int id);
    Task<BeerEntity?> FindById(int id);
    Task<BeerEntity?> Update(int id, BeerPayload toUpdate);
}

public class BeerServiceImpl(AppDbContext dbContext) : IBeerService
{
    public IEnumerable<BeerEntity> FindAll() => dbContext.Beers.ToList();

    public async Task<BeerEntity> Create(BeerPayload beer)
    {
        var entity = new BeerEntity
        {
            Name = beer.Name,
            Brand = beer.Brand,
            Strength = beer.Strength
        };
        dbContext.Beers.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public Task Delete(int id) => dbContext.Beers.Where(x => x.Id == id).ExecuteDeleteAsync();

    public Task<BeerEntity?> FindById(int id) => dbContext.Beers.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<BeerEntity?> Update(int id, BeerPayload toUpdate)
    {
        var found = await FindById(id);
        if (found == null) return null;

        found.Name = toUpdate.Name;
        found.Brand = toUpdate.Brand;
        found.Strength = toUpdate.Strength;
        await dbContext.SaveChangesAsync();
        return found;
    }
}