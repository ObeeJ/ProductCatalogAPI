using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ProductCatalogAPI.Tests.Integration;

public class ConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentOrders_ShouldPreventOverselling()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Clear database
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Limited Product",
            Description = "Only 10 in stock",
            Price = 99.99m,
            StockQuantity = 10
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act - Create 20 concurrent orders, each trying to buy 1 item
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 20; i++)
        {
            var orderDto = new CreateOrderDto
            {
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            };

            tasks.Add(_client.PostAsJsonAsync("/api/orders", orderDto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var successfulOrders = responses.Count(r => r.IsSuccessStatusCode);
        var failedOrders = responses.Count(r => !r.IsSuccessStatusCode);

        successfulOrders.Should().Be(10, "Only 10 orders should succeed due to stock limit");
        failedOrders.Should().Be(10, "10 orders should fail due to insufficient stock");

        // Verify final stock quantity
        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(0, "All stock should be consumed");
    }

    [Fact]
    public async Task ConcurrentOrders_WithLargeQuantities_ShouldPreventOverselling()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Bulk Product",
            Description = "100 in stock",
            Price = 50.00m,
            StockQuantity = 100
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act - Create 10 concurrent orders, each trying to buy 15 items (total 150 > 100 available)
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 10; i++)
        {
            var orderDto = new CreateOrderDto
            {
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 15 }
                }
            };

            tasks.Add(_client.PostAsJsonAsync("/api/orders", orderDto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var successfulOrders = responses.Count(r => r.IsSuccessStatusCode);
        var failedOrders = responses.Count(r => !r.IsSuccessStatusCode);

        // At most 6 orders should succeed (6 * 15 = 90, 7 * 15 = 105 > 100)
        successfulOrders.Should().BeLessOrEqualTo(6);
        failedOrders.Should().BeGreaterOrEqualTo(4);

        // Verify no overselling occurred
        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct!.StockQuantity.Should().BeGreaterOrEqualTo(0, "Stock should never go negative");
        
        var totalSold = 100 - updatedProduct.StockQuantity;
        totalSold.Should().Be(successfulOrders * 15, "Total sold should match successful orders");
    }
}