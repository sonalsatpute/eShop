using eShop.WebApi.Contracts;

namespace eShop.WebApi.Database.Repositories;

public interface IProductRepository
{
    Task<Product> GetProductAsync(Guid productId);
    Task CreateProductAsync(Product product);
    Task UpdateProduct(Product product);
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
        return product;
    }

    public Task CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating product with id {ProductId} in database", product.Id);
        // await _dbContext.Products.AddAsync(product);
        return Task.CompletedTask;
        
    }

    public Task UpdateProduct(Product product)
    {
        _logger.LogInformation("Updating product with id {ProductId} in database", product.Id);
        // _dbContext.Products.Update(product);
        return Task.CompletedTask;
    }
}