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

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<Product> GetProductAsync(Guid productId)
    {
        return _productRepository.GetProductAsync(productId);
    }
}