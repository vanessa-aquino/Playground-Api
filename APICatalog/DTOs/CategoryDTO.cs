using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs;

public class CategoryDTO
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Name { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImageUrl { get; set; }
}
