namespace ProductCatalogAPI.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string productName, int requested, int available)
        : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}")
    {
    }
}