using APICatalog.Models;
using AutoMapper;

namespace APICatalog.DTOs.Mappings;

public class ProductDTOMappingProfile : Profile
{
    public ProductDTOMappingProfile()
    {
        CreateMap<Product, ProductDTO>().ReverseMap();
        CreateMap<Category, CategoryDTO>().ReverseMap();
        CreateMap<Product, ProductDTOUpdateRequest>().ReverseMap();
        CreateMap<Product, ProductDTOUpdateResponse>().ReverseMap();
    }
}
