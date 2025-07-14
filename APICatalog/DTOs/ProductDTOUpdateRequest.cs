using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs;

public class ProductDTOUpdateRequest : IValidatableObject
{
    [Range(1,9999, ErrorMessage = "Stock must be between 1 and 9999.")]
    public float Stock { get; set; }

    public DateTime RegistrationDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RegistrationDate.Date <= DateTime.Now.Date)
        {
            yield return new ValidationResult("The date must be greater than the current date.",
                new[] { nameof(this.RegistrationDate) });
        }
    }
}
