using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "{0} is required")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public string? Password { get; set; }
}
