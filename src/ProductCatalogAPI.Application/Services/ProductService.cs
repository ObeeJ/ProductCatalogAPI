using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Application.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int pageNumber = 1, int pageSize = 10, string? nameFilter = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(nameFilter))
        {
            query = query.Where(p => p.Name.Contains(nameFilter));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(products),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = _mapper.Map<Product>(createProductDto);
        product.Id = Guid.NewGuid();
        product.LastModified = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        _context.Entry(product).OriginalValues["LastModified"] = updateProductDto.LastModified;
        _mapper.Map(updateProductDto, product);
        product.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}