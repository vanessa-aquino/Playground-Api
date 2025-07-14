using APICatalog.Data;
using APICatalog.Interfaces;

namespace APICatalog.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private IProductRepository? _productRepo;
    private ICategoryRepository? _categoryRepo;
    public AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // Eu implemento essa prop para obter uma instância do repositorio de produtos.
    public IProductRepository ProductRepository
    {
        get
        {
            // Toda vez que eu invocar uma instância do repositório, vou verificar se eu tenho uma instância de ProductRepository pronta, caso eu não tenha, vou criar uma.
            return _productRepo = _productRepo ?? new ProductRepository(_context);
        }
    }

    public ICategoryRepository CategoryRepository
    {
        get
        {
            return _categoryRepo = _categoryRepo ?? new CategoryRepository(_context);
        }
    }

    public async Task CommitAsync() => await _context.SaveChangesAsync();

    public async Task Dispose() => await _context.DisposeAsync();
}
