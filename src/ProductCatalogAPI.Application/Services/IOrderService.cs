using ProductCatalogAPI.Application.DTOs;

namespace ProductCatalogAPI.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
}