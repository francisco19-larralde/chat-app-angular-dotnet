using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Chats;

public class SendMessageDto
{
    [Required(ErrorMessage = "El mensaje no puede estar vacío")]
    [StringLength(4000, ErrorMessage = "El mensaje es demasiado largo")]
    public string Content { get; set; } = string.Empty;
}