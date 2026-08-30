namespace ChatApp.Application.DTOs.Chats;

public class MessageDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string? SenderProfilePictureUrl { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
}