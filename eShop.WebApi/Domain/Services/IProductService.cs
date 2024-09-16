using System.Net.Http.Headers;
using eShop.WebApi.Contracts;
using eShop.WebApi.Database.Repositories;

namespace eShop.WebApi.Domain.Services;

public interface IProductService
{
    Task<Product> GetProductAsync(Guid productId);
    Task CreateProductAsync(Product product);
    Task UpdateProduct(Product product);
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

    public async Task<Product> GetProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting product with ID {ProductId}", productId);
        
        HttpClient client = new HttpClient();
        var responseString = await client.GetStringAsync("http://www.google.com?q=sonal satpute");

        // _logger.LogInformation("Response from Google: {ResponseString}", responseString);
        
        
        return await _productRepository.GetProductAsync(productId);
    }

    public Task CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating product with ID {ProductId}", product.Id);
        return _productRepository.CreateProductAsync(product);
    }

    public Task UpdateProduct(Product product)
    {
        _logger.LogInformation("Updating product with ID {ProductId}", product.Id);
        return _productRepository.UpdateProduct(product);
    }
}