using eShop.WebApi.Contracts;
using eShop.WebApi.Database.Repositories;

namespace eShop.WebApi.Domain.Services;

public interface IProductService
{
    Task<Product> GetProductAsync(Guid productId);
}

public class ProductService : IProductService
{
    readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public Task<Product> GetProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting product with ID {ProductId}", productId);
        return _productRepository.GetProductAsync(productId);
    }
}