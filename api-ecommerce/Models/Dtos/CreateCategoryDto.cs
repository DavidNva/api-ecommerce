using System.ComponentModel.DataAnnotations;

namespace api_ecommerce.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    [MinLength(3, ErrorMessage = "El nombre no puede tener menos de 3 caracteres")]
    public string Name { get; set; } = string.Empty;
}
