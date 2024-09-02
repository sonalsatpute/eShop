using eShop.WebApi.Contracts;

namespace eShop.WebApi.Database.Repositories;

public interface IProductRepository
{
    Task<Product> GetProductAsync(Guid productId);
}

public class ProductRepository : IProductRepository
{
    private readonly eShopDbContext _dbContext;

    public ProductRepository(eShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Product> GetProductAsync(Guid productId)
    {
        Product product = (await _dbContext.Products.FindAsync(productId))!;
        return product;
    }
}