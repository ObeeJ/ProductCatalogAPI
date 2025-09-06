# Product Catalog API

A .NET Core Web API for managing product catalogs and processing orders. This project demonstrates clean architecture principles and handles concurrent order processing without overselling inventory.

## Problem Solved

E-commerce platforms often struggle with inventory management during high traffic periods. When multiple customers try to purchase the same product simultaneously, systems can oversell inventory, leading to customer dissatisfaction and operational issues. This API solves that problem through proper concurrency control.

## Key Features

- **Product Management**: Full CRUD operations for product catalog
- **Order Processing**: Place orders with automatic stock validation
- **Concurrency Control**: Prevents overselling during simultaneous orders
- **Clean Architecture**: Organized codebase following SOLID principles
- **Database Flexibility**: Supports both SQLite (development) and PostgreSQL (production)

## Technical Stack

- **.NET 9.0** - Latest LTS framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL/SQLite** - Database options
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **xUnit** - Testing framework

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Git
- Docker (optional)

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/ObeeJ/ProductCatalogAPI.git
   cd ProductCatalogAPI
   ```

2. Start the application:
   ```bash
   dotnet run --project src/ProductCatalogAPI.API
   ```

3. Access the API:
   - API: https://localhost:7000
   - Documentation: https://localhost:7000/swagger

### Using Docker

For a complete setup with PostgreSQL:

```bash
docker-compose up --build
```

The API will be available at http://localhost:8080

## API Endpoints

### Products
- `GET /api/products` - List products (with pagination)
- `GET /api/products/{id}` - Get specific product
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Orders
- `POST /api/orders` - Place an order

## Example Usage

### Create a Product
```bash
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Headphones",
    "description": "Noise-cancelling wireless headphones",
    "price": 199.99,
    "stockQuantity": 50
  }'
```

### Place an Order
```bash
curl -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "orderItems": [
      {
        "productId": "your-product-id-here",
        "quantity": 2
      }
    ]
  }'
```

## Concurrency Control

The system prevents overselling through database transactions and optimistic concurrency control. When multiple orders are placed simultaneously:

1. Each request starts a database transaction
2. Stock levels are validated atomically
3. Only orders with sufficient stock succeed
4. Failed orders receive clear error messages
5. Stock is never reduced below zero

## Architecture

The project follows Clean Architecture with four layers:

- **Domain**: Core business entities and rules
- **Application**: Business logic and use cases  
- **Infrastructure**: Data access and external services
- **API**: HTTP endpoints and presentation logic

This structure ensures the code is testable, maintainable, and follows separation of concerns.

## Testing

Run the test suite:
```bash
dotnet test
```

The project includes:
- Unit tests for business logic
- Integration tests for API endpoints
- Concurrency tests to verify stock management

## Configuration

By default, the application uses SQLite for development. To use PostgreSQL:

```json
{
  "UsePostgreSQL": true,
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ProductCatalog;Username=postgres;Password=yourpassword"
  }
}
```

## Design Decisions

### Why Optimistic Concurrency?
Optimistic concurrency was chosen over pessimistic locking because:
- Better performance under normal load
- Avoids deadlock scenarios
- Scales better with multiple users
- Provides clear error messages when conflicts occur

### Database Choice
- SQLite for development: No setup required, easy testing
- PostgreSQL for production: Robust, scalable, handles concurrency well

### Architecture Pattern
Clean Architecture provides:
- Clear separation of concerns
- Easy unit testing
- Framework independence
- Maintainable codebase

## License

This project is licensed under the MIT License.