using APICatalog.Models;
using APICatalog.Pagination;

namespace APICatalog.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<PagedList<Category>> GetCategoriesAsync(CategoriesParameters categoriesParameters);
    Task<PagedList<Category>> GetCategoriesFilteredByNameAsync(CategoriesFilterName categoriesFilterName);
}
