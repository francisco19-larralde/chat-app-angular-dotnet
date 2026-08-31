using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTOs.Chats;


public class SendMessageDto
{

    [StringLength(4000, ErrorMessage = "El mensaje es demasiado largo")]
    public string Content { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
}