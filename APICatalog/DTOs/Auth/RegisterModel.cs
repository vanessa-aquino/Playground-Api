using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs.Auth;

public class RegisterModel
{
    [Required(ErrorMessage = "{0} is required")]
    public string? UserName { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "{0} is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public string? Password { get; set; }
}
