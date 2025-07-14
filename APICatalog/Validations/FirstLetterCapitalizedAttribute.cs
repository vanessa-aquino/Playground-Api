using System.ComponentModel.DataAnnotations;

namespace APICatalog.Validations;

public class FirstLetterCapitalizedAttribute : ValidationAttribute
{
    // Vou sobreescrever o método "IsValid" que é do tipo "ValidationResult":
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) // O primeiro parâmetro é o valor da propriedade, e o segundo traz informações do contexto onde estamos executando a validação.
    {
        // Tenho que fazer primeiro, passar por uma validação que já está sendo feita:
        // Essa validação é feita no meu model product, na prop Name, que não deixa meu valor ser null, nem vazio, através do atributo Required.
        if (value == null || string.IsNullOrEmpty(value.ToString()))
            return ValidationResult.Success;

        var firstLetter = value.ToString()[0].ToString();
        if (firstLetter != firstLetter.ToUpper())
            return new ValidationResult("The first letter of the product must be capitalized.");

        return ValidationResult.Success;

    }
}
