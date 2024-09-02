using eShop.WebApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace eShop.WebApi.Database;

public class eShopDbContext : DbContext
{
    public eShopDbContext(DbContextOptions<eShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Bookings { get; set; } = null!;
}