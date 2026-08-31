using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Groups;

public class CreateGroupDto
{
    [Required(ErrorMessage = "El grupo necesita un nombre")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Elegí al menos un miembro para el grupo")]
    public List<int> MemberIds { get; set; } = new();
}