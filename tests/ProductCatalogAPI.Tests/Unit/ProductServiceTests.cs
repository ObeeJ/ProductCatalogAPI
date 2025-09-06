using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Mappings;
using ProductCatalogAPI.Application.Services;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;
using Xunit;

namespace ProductCatalogAPI.Tests.Unit;

public class ProductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _productService = new ProductService(_context, _mapper);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct_WhenValidDataProvided()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.99m,
            StockQuantity = 100
        };

        // Act
        var result = await _productService.CreateProductAsync(createProductDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createProductDto.Name);
        result.Price.Should().Be(createProductDto.Price);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnPagedResults_WhenProductsExist()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.99m, StockQuantity = 10 },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.99m, StockQuantity = 20 }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.GetProductsAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}