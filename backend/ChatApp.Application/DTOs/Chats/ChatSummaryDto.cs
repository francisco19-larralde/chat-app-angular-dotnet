namespace ChatApp.Application.DTOs.Chats;


public class ChatSummaryDto
{
    public int ChatId { get; set; }
    public bool IsGroup { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string? DisplayPictureUrl { get; set; }

    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public bool IsOtherUserOnline { get; set; }
}