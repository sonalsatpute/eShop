using eShop.WebApi.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace eShop.WebApi.Database;

public static class ServiceConfigurations
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<eShopDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddTransient<IProductRepository, ProductRepository>();

        return services;
    }
}