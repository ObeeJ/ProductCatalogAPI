using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Exceptions;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Application.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            var productIds = createOrderDto.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var orderItemDto in createOrderDto.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == orderItemDto.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {orderItemDto.ProductId} not found");

                if (product.StockQuantity < orderItemDto.Quantity)
                    throw new InsufficientStockException(product.Name, orderItemDto.Quantity, product.StockQuantity);

                product.StockQuantity -= orderItemDto.Quantity;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = orderItemDto.Quantity,
                    UnitPrice = product.Price,
                    OrderId = order.Id
                };

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstAsync(o => o.Id == order.Id);

            return _mapper.Map<OrderDto>(result);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}