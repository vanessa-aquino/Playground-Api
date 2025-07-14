using APICatalog.Validations;
using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs;

public class ProductDTO
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Nome obrigatório")]
    [StringLength(80, ErrorMessage = "O nome deve ter no máximo {1} e no mínimo {2} caracteres", MinimumLength = 5)]
    [FirstLetterCapitalized]
    public string? Name { get; set; }

    [Required]
    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}
