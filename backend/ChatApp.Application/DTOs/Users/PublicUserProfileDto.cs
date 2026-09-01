namespace ChatApp.Application.DTOs.Users;


public class PublicUserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPictureUrl { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}