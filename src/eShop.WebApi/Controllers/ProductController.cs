using eShop.WebApi.Contracts;
using eShop.WebApi.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShop.WebApi.Controllers;

[Route("/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{productId}", Name = nameof(GetProductById))]
    [ProducesResponseType(404)]
    [ProducesResponseType(200)]
    public async Task<ActionResult<Product>> GetProductById(Guid productId)
    {
        Product? product = await _productService.GetProductAsync(productId);
        if (product == null) return NotFound();

        return Ok(product);
    }
}