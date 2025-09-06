namespace ProductCatalogAPI.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateOrderDto
{
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}