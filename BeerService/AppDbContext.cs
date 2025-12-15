#region

using Microsoft.EntityFrameworkCore;

#endregion

namespace BeerService;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BeerEntity> Beers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BeerEntity>()
            .ComplexProperty(x => x.Details, b => b.ToJson());
    }
}