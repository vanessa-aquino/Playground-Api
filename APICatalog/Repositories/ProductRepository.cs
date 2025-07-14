using APICatalog.Data;
using APICatalog.Interfaces;
using APICatalog.Models;
using APICatalog.Pagination;

namespace APICatalog.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context)
        : base(context) { }

    public async Task<PagedList<Product>> GetProductsAsync(ProductsParameters productsParameters)
    {
        var products = await GetAllAsync();
        var sortedProducts = products.OrderBy(p => p.ProductId).AsQueryable();

        var paginationProducts = PagedList<Product>.ToPagedList(sortedProducts, productsParameters.PageNumber, productsParameters.PageSize);

        return paginationProducts;
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int id)
    {
        var products = await GetAllAsync();
        return products.Where(c => c.CategoryId == id);
    }

    public async Task<PagedList<Product>> GetProductsFilteredByPriceAsync(ProductFilterPrice productFilterPrice)
    {
        var products = await GetAllAsync();

        if (productFilterPrice.Price.HasValue && !string.IsNullOrEmpty(productFilterPrice.PriceCriteria))
        {
            if (productFilterPrice.PriceCriteria.Equals("maior", StringComparison.OrdinalIgnoreCase))
                products = products.Where(p => p.Price > productFilterPrice.Price.Value).OrderBy(p => p.Price);
            else if (productFilterPrice.PriceCriteria.Equals("menor", StringComparison.OrdinalIgnoreCase))
                products = products.Where(p => p.Price < productFilterPrice.Price.Value).OrderBy(p => p.Price);
            else if (productFilterPrice.PriceCriteria.Equals("igual", StringComparison.OrdinalIgnoreCase))
                products = products.Where(p => p.Price == productFilterPrice.Price.Value).OrderBy(p => p.Price);
        }

        var filteredProducts = PagedList<Product>.ToPagedList(products.AsQueryable(), productFilterPrice.PageNumber, productFilterPrice.PageSize);

        return filteredProducts;
    }
}
