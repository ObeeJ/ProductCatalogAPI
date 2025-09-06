namespace ProductCatalogAPI.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}