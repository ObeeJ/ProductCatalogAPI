using ProductCatalogAPI.Application.DTOs;

namespace ProductCatalogAPI.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(int pageNumber = 1, int pageSize = 10, string? nameFilter = null);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(Guid id);
}