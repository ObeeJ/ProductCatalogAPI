namespace ProductCatalogAPI.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}