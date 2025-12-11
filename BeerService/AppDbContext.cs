#region

using Microsoft.EntityFrameworkCore;

#endregion

namespace BeerService;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BeerEntity> Beers { get; set; }
}