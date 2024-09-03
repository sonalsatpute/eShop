using eShop.WebApi.Contracts;

namespace eShop.WebApi.Database.Repositories;

public interface IProductRepository
{
    Task<Product> GetProductAsync(Guid productId);
}

public class ProductRepository : IProductRepository
{
    private readonly eShopDbContext _dbContext;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(eShopDbContext dbContext, ILogger<ProductRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Product> GetProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting product with id {ProductId} from database", productId);
        Random random = new Random();
        // Product? product = await _dbContext.Products.FindAsync(productId);
        
        Product product = new Product(productId, $"Product {productId}", (decimal)random.NextDouble());
        return product!;
    }
}