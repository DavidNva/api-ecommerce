using api_ecommerce.Models;
using api_ecommerce.Models.Dtos;
using AutoMapper;

namespace api_ecommerce.Mapping;

public class ProductProfile: Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Product, CreateProductDto>().ReverseMap();
        CreateMap<Product, UpdateProductDto>().ReverseMap();

        // CreateMap<Product, ProductDto>().ReverseMap();
        // CreateMap<CreateProductDto, Product>();
        // CreateMap<UpdateProductDto, Product>();
    }
}
