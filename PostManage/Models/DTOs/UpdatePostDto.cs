using System.ComponentModel.DataAnnotations;

namespace PostManage.Models.DTOs;

public class UpdatePostDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Url(ErrorMessage = "Image must be a valid URL")]
    public string? Image { get; set; }
}

