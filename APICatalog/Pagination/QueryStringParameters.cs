namespace APICatalog.Pagination;

public abstract class QueryStringParameters
{
    const int maxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = maxPageSize;
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            // Com isso eu permito que os clientes da API especifiquem o número da página desejado e o tamanho, não podendo exceder o meu maxPageSize.
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
