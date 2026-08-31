namespace ChatApp.Application.DTOs.Groups;

public class GroupMemberDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsOnline { get; set; }
    public string Role { get; set; } = string.Empty;
}