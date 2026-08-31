using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Groups;

public class UpdateGroupDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}