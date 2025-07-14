using APICatalog.Models;
using APICatalog.Pagination;

namespace APICatalog.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedList<Product>> GetProductsAsync(ProductsParameters productsParameters);
    Task<PagedList<Product>> GetProductsFilteredByPriceAsync(ProductFilterPrice productFilterPrice);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int id);
}
