namespace ChatApp.Application.DTOs.Friends;

// Resultado de buscar usuarios (para agregar como amigo)
public class UserSearchResultDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    public string FriendshipStatus { get; set; } = "None";
}