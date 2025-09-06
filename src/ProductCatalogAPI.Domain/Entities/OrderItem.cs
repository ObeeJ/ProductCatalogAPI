namespace ProductCatalogAPI.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid OrderId { get; set; }
    
    public Product Product { get; set; } = null!;
    public Order Order { get; set; } = null!;
}