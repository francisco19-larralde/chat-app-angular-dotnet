namespace ChatApp.Application.DTOs.Friends;

public class FriendRequestDto
{
    public int FriendshipId { get; set; }
    public int RequesterId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
    public string? RequesterProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}