using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Users;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 30 caracteres")]
    public string Username { get; set; } = string.Empty;
}