using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Auth;

public class GoogleLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}