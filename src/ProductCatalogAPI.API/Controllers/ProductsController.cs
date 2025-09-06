using Microsoft.AspNetCore.Mvc;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Services;

namespace ProductCatalogAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameFilter = null)
    {
        var result = await _productService.GetProductsAsync(pageNumber, pageSize, nameFilter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var product = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _productService.DeleteProductAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}