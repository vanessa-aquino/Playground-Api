using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APICatalog.Models;

public class Category : IValidatableObject
{
    public Category()
    {
        Products = new Collection<Product>();
    }

    public int CategoryId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Name { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public ICollection<Product>? Products { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Name))
        {
            var firstLetter = Name[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                yield return new // A palavra reserva "yield" indica que o método é um iterador, e é usado o "yield return" para retornar cada elemento individualmente.
                    ValidationResult("The first letter of the product must be capitalized.",
                    new[]
                    { nameof(Name) }
                    );
            }
        }
    }
}

