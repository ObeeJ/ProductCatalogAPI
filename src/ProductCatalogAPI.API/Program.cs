using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.API.Middleware;
using ProductCatalogAPI.Application.Mappings;
using ProductCatalogAPI.Application.Services;
using ProductCatalogAPI.Application.Validators;
using ProductCatalogAPI.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure for Render deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var usePostgreSQL = builder.Configuration.GetValue<bool>("UsePostgreSQL");

if (usePostgreSQL)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

// Application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Root endpoint for Render health checks
app.MapGet("/", () => "ProductCatalogAPI is running");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

public partial class Program { }