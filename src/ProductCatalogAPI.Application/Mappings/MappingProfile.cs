using AutoMapper;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
    }
}