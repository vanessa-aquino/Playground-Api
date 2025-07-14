namespace APICatalog.Pagination;

public class ProductFilterPrice : QueryStringParameters
{
    public decimal? Price { get; set; }
    public string? PriceCriteria { get; set; } // "maior", "menor" ou "igual"
}
