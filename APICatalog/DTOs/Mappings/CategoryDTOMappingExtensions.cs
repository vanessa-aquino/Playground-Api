using APICatalog.Models;

namespace APICatalog.DTOs.Mappings;

public static class CategoryDTOMappingExtensions
{
    public static CategoryDTO? ToCategoryDTO(this Category category)
    {
        if (category == null) return null;

        return new CategoryDTO
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            ImageUrl = category.ImageUrl
        };
    }

    public static Category? ToCategory(this CategoryDTO categoryDTO)
    {
        if (categoryDTO == null) return null;

        return new Category
        {
            CategoryId = categoryDTO.CategoryId,
            Name = categoryDTO.Name,
            ImageUrl = categoryDTO.ImageUrl
        };
    }

    public static IEnumerable<CategoryDTO> ToCategoryDTOList(this IEnumerable<Category> categories)
    {
        if (categories == null || !categories.Any())
            return new List<CategoryDTO>();

        return categories.Select(category => new CategoryDTO
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            ImageUrl = category.ImageUrl
        }).ToList();
    }
}
