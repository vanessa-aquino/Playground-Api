using APICatalog.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APICatalog.Models;

public class Product
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
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public float Stock { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int CategoryId { get; set; }

    [JsonIgnore]
    public Category? Category { get; set; }
}
