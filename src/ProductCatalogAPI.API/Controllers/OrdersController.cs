using Microsoft.AspNetCore.Mvc;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Services;
using ProductCatalogAPI.Domain.Exceptions;

namespace ProductCatalogAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(createOrderDto);
            return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order);
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}