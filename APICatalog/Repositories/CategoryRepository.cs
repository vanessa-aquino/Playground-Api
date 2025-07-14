using APICatalog.Data;
using APICatalog.Interfaces;
using APICatalog.Models;
using APICatalog.Pagination;


namespace APICatalog.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context)
        : base(context) { }

    public async Task<PagedList<Category>> GetCategoriesAsync(CategoriesParameters categoriesParameters)
    {
        var categories = await GetAllAsync();
        var sortedcategories = categories.OrderBy(p => p.CategoryId).AsQueryable();

        var paginationCategories = PagedList<Category>.ToPagedList(sortedcategories, categoriesParameters.PageNumber, categoriesParameters.PageSize);

        return paginationCategories;
    }

    public async Task<PagedList<Category>> GetCategoriesFilteredByNameAsync(CategoriesFilterName categoriesFilterName)
    {
        var categories = await GetAllAsync();

        if (!string.IsNullOrEmpty(categoriesFilterName.Name))
            categories = categories.Where(c => c.Name.ToLower().Contains(categoriesFilterName.Name.ToLower()));

        var filteredCategories = PagedList<Category>.ToPagedList(categories.AsQueryable(), categoriesFilterName.PageNumber, categoriesFilterName.PageSize);

        return filteredCategories;
    }
}
