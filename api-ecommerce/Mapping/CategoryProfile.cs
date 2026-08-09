using System;
using api_ecommerce.Models;
using api_ecommerce.Models.Dtos;
using AutoMapper;

namespace api_ecommerce.Mapping;

public class CategoryProfile: Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>().ReverseMap();
    }
}
