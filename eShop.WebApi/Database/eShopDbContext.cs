using eShop.WebApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace eShop.WebApi.Database;

public class eShopDbContext : DbContext
{
    public eShopDbContext(DbContextOptions<eShopDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(e => e.Id);
    }

    public DbSet<Product> Products { get; set; } = null!;
}