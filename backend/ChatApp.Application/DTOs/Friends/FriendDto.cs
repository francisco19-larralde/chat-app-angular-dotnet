namespace ChatApp.Application.DTOs.Friends;


public class FriendDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}